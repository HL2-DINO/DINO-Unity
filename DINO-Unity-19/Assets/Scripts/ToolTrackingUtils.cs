using System.Collections.Generic;
using UnityEngine;

/** @file           ToolTrackingUtils.cs
 *  @brief          Defines some app-wide specifics for IR tool tracking and also helper functions
 *
 *  @author         Hisham Iqbal
 *  @copyright      &copy; 2023 Hisham Iqbal
 */

namespace ToolTrackingUtils
{
    /// <summary>
    /// Struct universally used through the HL-DINO-Unity app to define a 'trackable' tool
    /// </summary>
    [System.Serializable]
    public class TrackedTool
    {
        /// <summary>
        /// Numeric ID which is passed into the HL2-DINO-DLL. This is cast into a uint8 on the C++ side.
        /// </summary>
        public int ToolID;

        /// <summary>
        /// String identifier, purely for labelling in the Unity Editor
        /// </summary>
        public string ToolName;

        /// <summary>
        /// XYZ triplets of marker-centres in metres (left-handed for Unity)
        /// </summary>
        public List<Vector3> ToolMarkerTriplets;

        /// <summary>
        /// Unity transform corresponding to the tool that we will position
        /// </summary>
        public Transform ToolUnityTransform;

        /// <summary>
        /// Transform matrix of tool pose, left-handed and in metres
        /// </summary>
        [HideInInspector]
        public Matrix4x4 Tool_HoloFrame_LH;

        /// <summary>
        /// Flag bool indicating if the HL2 can see the tool in the last set of sensor frames
        /// </summary>
        [HideInInspector]
        public bool VisibleToHoloLens;

        /// <summary>
        /// Timestamp updated on the Unity side to track how long a tool has been visible for
        /// </summary>
        [HideInInspector]
        public float TimestampLastSeen;        
    }

    public static class DinoPluginUtils
    {
        /// <summary>
        /// Function will generate a coded formatted string with tool IDs and triplets
        /// </summary>
        /// <returns>Formatted tool-string to be passed into DLL</returns>
        private static string GenerateToolString(List<TrackedTool> toolsToTrack)
        {
            // function will produce a formatted string as follows:
            // "toolID,x1,y1,z1,..xn,yn,zn;toolID2,...."

            string msg = "";

            foreach (TrackedTool tool in toolsToTrack)
            {
                msg += (tool.ToolID).ToString() + ",";

                List<Vector3> triplets = tool.ToolMarkerTriplets;

                for (int i = 0; i < triplets.Count - 1; i++)
                {
                    msg += string.Format("{0:F5},{1:F5},{2:F5},", triplets[i].x, triplets[i].y, triplets[i].z);
                }

                // add semi-colon after final triplet
                msg += string.Format("{0:F5},{1:F5},{2:F5};", triplets[triplets.Count - 1].x, triplets[triplets.Count - 1].y, triplets[triplets.Count - 1].z);

            }

            msg.TrimEnd(';');
            return msg;
        }
    }

    /// <summary>
    /// A helper class for parsing Tool Config data
    /// </summary>
    public static class UnitySceneSetup
    {
        /// <summary>
        /// Helper function for creating some sphere GameObjects, located at marker centres of a tool. These are attached properly 
        /// to object transforms so that you can visualise little spheres when wearing the HL2 and looking at a tracked tool through 
        /// the headset.
        /// </summary>
        /// <param name="toolsToAdd">A properly filled list of \p TrackedTool which we will use to instantiate marker objects</param>
        public static void AddMarkerCentreSpheres(List<TrackedTool> toolsToAdd)
        {
            foreach (var tool in toolsToAdd)
            {
                if (tool.ToolUnityTransform == null) continue;
                                
                // parent for all of our marker centre spheres
                GameObject markerParent = new GameObject("tracker");
                
                markerParent.transform.parent = tool.ToolUnityTransform;
                markerParent.transform.localPosition = Vector3.zero;
                markerParent.transform.localRotation = Quaternion.identity;
                
                int markerID = 0; // used to label each marker we add
                foreach (var coordinate in tool.ToolMarkerTriplets)
                {
                    // add a new sphere for each tool marker we have
                    GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    marker.name = "Marker_" + markerID.ToString();
                    marker.transform.parent = markerParent.transform;

                    float sphereRadiusMetres = 0.005f;
                    marker.transform.localScale = new Vector3(sphereRadiusMetres, sphereRadiusMetres, sphereRadiusMetres);
                    marker.transform.localRotation = Quaternion.identity;
                    marker.transform.localPosition = coordinate;

                    ++markerID;
                }
            }
        }
    }

}
