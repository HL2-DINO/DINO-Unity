using System.Collections.Generic;
using UnityEngine;

/** @file           MatrixUtilities.cs
 *  @brief          Helper matrix utility Functions handling UnityEngine's standard Matrix4x4/Vector3 types.
 *                  Mostly for help with conversion operations for left-right handed and units.
 *
 *  @author         Hisham Iqbal
 *  @copyright      &copy; 2023 Hisham Iqbal
 */

public class MatrixUtilities
{
    public enum MatrixEntryOrder
    {
        RowMajor = 0,
        ColumnMajor = 1
    }

    public enum MatrixUnits
    {
        mm = 0,
        m = 1
    }

    public enum Handedness
    {
        Left, // Unity
        Right // HoloLens ResearchMode / C++ API
    }

    /// <summary>
    /// Tries to parse a double array into a Matrix4x4
    /// </summary>
    /// <param name="vals">Raw double values for matrix, should be 16 elements</param>
    /// <param name="order">Enum describing if elements are in row/column major order</param>
    /// <param name="units">Enum describing units of matrix elements</param>
    /// <returns>Formatted Matrix4x4 filled with values</returns>
    public static Matrix4x4 FillMatrixWithDoubles(double[] vals, MatrixEntryOrder order, MatrixUnits units)
    {
        if (vals.Length < 16) { return Matrix4x4.identity; }

        Matrix4x4 returnMat = Matrix4x4.zero;

        Vector4 col1 = new Vector4((float)vals[0], (float)vals[1], (float)vals[2], (float)vals[3]);
        Vector4 col2 = new Vector4((float)vals[4], (float)vals[5], (float)vals[6], (float)vals[7]);
        Vector4 col3 = new Vector4((float)vals[8], (float)vals[9], (float)vals[10], (float)vals[11]);
        Vector4 col4 = new Vector4((float)vals[12], (float)vals[13], (float)vals[14], (float)vals[15]);

        returnMat.SetColumn(0, col1); returnMat.SetColumn(1, col2);
        returnMat.SetColumn(2, col3); returnMat.SetColumn(3, col4);

        // check if you need to transpose and correctly orient the matrix
        if (order == MatrixEntryOrder.ColumnMajor)
        {
            // no transpose do nothing
        }
        else if (order == MatrixEntryOrder.RowMajor)
        {
            returnMat = returnMat.transpose;
        }
        else
        {
            Debug.Log("Invalid matrix order passed in");
            return Matrix4x4.zero;
        }

        // Check and handle units to see if scaling is required
        if (units == MatrixUnits.m) { return returnMat; } //no scaling required, input was already in metres

        else if (units == MatrixUnits.mm)
        {
            returnMat.SetColumn(3, new Vector4(returnMat[0, 3] / 1000f, returnMat[1, 3] / 1000f, returnMat[2, 3] / 1000f, 1f));
            return returnMat;
        }

        else { Debug.Log("Invalid MatrixUnits enum value supplied"); return Matrix4x4.zero; }
    }

    /// <summary>
    /// Helper function to convert between left/right handed matrices, achieved by inverting the Z-related components in the matrix
    /// </summary>
    /// <param name="_m">Conventional right-handed matrix</param>
    /// <returns>Output matrix with swapped handedness</returns>
    public static Matrix4x4 ReturnZInvertedMatrix(Matrix4x4 _m)
    {
        /*
         *  Notation/labelling from Craig's Intro to Robotics
         *  Take two coordinate frames {A}, and {B}. 
         *  {A} and {B} are associated with 3 orthogonal direction vectors Xa,Ya,Za & Xb,Yb,Zb
         *  
         *  R_AB describes the rotation of B relative to A (e.g. from B -> A)
         * 
         *  R_AB = [r00  r01  r02]   =   [(Xb . Xa)  (Yb . Xa)  (Zb . Xa)]  
         *         |r10  r11  r12|       |(Xb . Ya)  (Yb . Ya)  (Zb . Ya)|  
         *         [r20  r21  r22]       [(Xb . Za)  (Yb . Za)  (Zb . Za)]
         *         
         *  We want to invert the Z component of any vectors in our system to go from right -> left handed
         *  So we treat it as inverting the 'z' component of all unit vectors (Xa,Ya,Za,Xb,Yb,Zb)
         *  This affects some of our dot products, and affects the following elements of our matrix if we invert Z:
         *  r20, r21, r02, r12 (but not r22 as the two negatives cancel each other out). 
         *  
         *  For the translation vector, just invert the z element in the transform matrix.
         *  
         *  Note: this works, but we have to follow this convention stringently now: e.g. right handed matrices passed
         *  into the app need to go through this function. 
         */

        // rotation elements
        _m[0, 2] *= -1;
        _m[1, 2] *= -1;
        _m[2, 0] *= -1; _m[2, 1] *= -1;

        // deal with translation vector
        _m[2, 3] *= -1;
        return _m;
    }

    public static List<Vector3> InvertCoordinatesZ(List<Vector3> coordinates)
    {
        Vector3 Scaler = new Vector3(1, 1, -1); // flips z values
        for (int i = 0; i < coordinates.Count; i++)
        {
            coordinates[i] = Vector3.Scale(coordinates[i], Scaler);
        }

        return coordinates;
    }

}