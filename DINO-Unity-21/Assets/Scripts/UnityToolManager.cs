using System.Collections.Generic;
using UnityEngine;
using ToolTrackingUtils;
using System.Linq;

/** @file           UnityToolManager.cs
 *  @brief          Main Unity script for processing encoded double arrays passed in from the HL2-DINO-DLL
 *                  and using this info to update Unity GameObject transforms which are properly set up.
 *                  
 *                  Use the Editor scripts to properly configure ToolsTrackedByHololens to match the contents of
 *                  your config.json file. (Or manually set this up yourself at your own risk...!) Script
 *                  should help convert all of the right-handed information that comes out of the DLL and handle things
 *                  like unit and coordinate frame conversion when setting Transform rotation and positions.
 *
 *  @author         Hisham Iqbal
 *  @copyright      &copy; 2023 Hisham Iqbal
 */

public class UnityToolManager : MonoBehaviour
{
    /// <summary>
    /// Axis to flip when converting from right-handed (HL2 Research Mode) to left-handed (Unity). This needs to be 
    /// universally consistent everywhere you pass info in and out of Unity. So all right-handed information flowing
    /// into the app should be inverted in this direction. Similarly, to convert it to right-handed when flowing out 
    /// of Unity to some other app, invert in the direction you have specified.
    /// </summary>
    //public MatrixUtilities.Direction UnityInversionDirection = MatrixUtilities.Direction.z;

    /// <summary>
    /// List of structs which describe the tools the headset tracks.
    /// Used as both an input to the DLL (to describe the geometries of tools we track) and also used to 
    /// cache GameObject transforms, so we can move around holograms based on data we receive from the HL2.
    /// This should be configured and properly set up prior to app compilation and deployment.
    /// </summary>
    public List<TrackedTool> ToolsTrackedByHololens;

    private readonly object toolDictLock = new object();

    /// <summary>
    /// Stashes information about each tool we can see. Entries are initialised based on members contained in 
    /// \p ToolsTrackedByHololens on startup. During runtime this dictionary will be updated based on info received
    /// from HL2-DINO-DLL.
    /// </summary>
    public Dictionary<int, TrackedTool> ToolDictionary = new Dictionary<int, TrackedTool>();

    System.Diagnostics.Stopwatch ScriptTimer = new System.Diagnostics.Stopwatch();

    /// <summary>
    /// A double array to dump the latest encoded message from the HL2 into
    /// </summary>
    private double[] LatestDoubleArray;
    private readonly object doubleArrayLock = new object();
    private volatile bool NewToolArrayReceived;

    // Start is called before the first frame update
    void Start()
    {
        ScriptTimer.Start();
        InitialiseToolDictionary();
    }

    void InitialiseToolDictionary()
    {
        // walk through the entries of the List and initialise our dictionary
        foreach (var tool in ToolsTrackedByHololens)
        {
            ToolDictionary.Add(tool.ToolID, tool);
        }

        // 18 elements per tool (2 informational bits + 16 doubles for the transform matrix)
        LatestDoubleArray = new double[ToolDictionary.Count * 18];
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTrackedToolDictionary();
        UpdateHologramPositions();
    }

    /// <summary>
    /// Reads entries of the internal tool dictionary, and then updates GameObject transforms accordingly
    /// </summary>
    void UpdateHologramPositions()
    {
        // what's the time now?
        float currentTimestamp = ScriptTimer.ElapsedMilliseconds;

        foreach (var tool in ToolDictionary.ToArray())
        {
            Matrix4x4 targetPoseUnity = Matrix4x4.identity;
            TrackedTool trackedTool;

            trackedTool = tool.Value;

            if (trackedTool.VisibleToHoloLens)
            {
                targetPoseUnity = trackedTool.Tool_HoloFrame_LH;
                trackedTool.ToolUnityTransform.SetPositionAndRotation(targetPoseUnity.GetColumn(3), targetPoseUnity.rotation.normalized);
            }
            else // not visible to HL2
            {
                if ((currentTimestamp - trackedTool.TimestampLastSeen) < 2000f) // not been seen for some time threshold
                {
                    continue; // wait a little longer and leave the transform frozen in space
                }

                targetPoseUnity.SetColumn(3, new Vector4(0, -3, 0, 1)); // put it 6 feet under?
                trackedTool.ToolUnityTransform.SetPositionAndRotation(targetPoseUnity.GetColumn(3), targetPoseUnity.rotation);
            }

            lock (toolDictLock)
            {
                // we've updated the tool's transform, so update the tool's entry in the dictionary 
                if (ToolDictionary.ContainsKey(tool.Key)) ToolDictionary[tool.Key] = trackedTool;
            }
        }
    }

    /// <summary>
    /// Public function to dump the latest encoded double array received from HL2-DINO-DLL
    /// </summary>
    /// <param name="matTransforms">Encoded information about tool poses and visibility</param>
    public void EnqueueTrackingData(double[] matTransforms)
    {
        if (LatestDoubleArray.Length != matTransforms.Length) { return; }
        lock (doubleArrayLock)
        {
            System.Buffer.BlockCopy(matTransforms, 0, LatestDoubleArray, 0, matTransforms.Length * sizeof(double));
            NewToolArrayReceived = true;
        }
    }

    ///// <summary>
    ///// Should be called from wherever you manage parsing the config JSON file. Call with caution.
    ///// </summary>
    ///// <param name="dir">Direction to invert for left-right handed conversion</param>
    //public void SetUnityInversionDirection(MatrixUtilities.Direction dir)
    //{
    //    this.UnityInversionDirection = dir;
    //}

    /// <summary>
    /// Check if there has been any new information dumped to the member variable \p LatestDoubleArray, if so, then update 
    /// our internal dictionary.
    /// </summary>
    private void UpdateTrackedToolDictionary()
    {
        if (!NewToolArrayReceived) { return; }

        // variable for entire 'packet'
        double[] matTransforms = new double[LatestDoubleArray.Length];

        // variables per-tool
        int toolID;
        bool visible2Holo;
        double[] toolMatrixElements = new double[16];

        lock (doubleArrayLock)
        {
            // dump into matTransforms for processing
            System.Buffer.BlockCopy(LatestDoubleArray, 0, matTransforms, 0, LatestDoubleArray.Length * sizeof(double));
            NewToolArrayReceived = false; // consumed, so reset the flag
        }
        // expected packet format for each tool:
        // [1 element: tool ID, 1 element: Visibility (0 - False, 1 - True), 16 mat elements] | total 18 elements
        int numberOfTools = matTransforms.Length / 18;

        for (int i = 0; i < numberOfTools; i++)
        {
            toolID = (int)(matTransforms[i * 18]); // casting from double to int
            visible2Holo = ((int)(matTransforms[i * 18 + 1]) != 0); // cast from double to bool

            // move pose matrix from the main array into the 'tool' array
            for (int k = 0; k < 16; k++) toolMatrixElements[k] = matTransforms[i * 18 + 2 + k];

            // grab the right-handed tool pose matrix, which we are expecting to be in a column-major 
            // format, and also in metres
            Matrix4x4 tool_Holo_RH = MatrixUtilities.FillMatrixWithDoubles(toolMatrixElements,
                MatrixUtilities.MatrixEntryOrder.ColumnMajor, MatrixUtilities.MatrixUnits.m);

            // create a left-handed pose matrix, to be compatible with Unity transforms
            Matrix4x4 tool_Holo_LH = MatrixUtilities.ReturnZInvertedMatrix(tool_Holo_RH);

            // safety check, in case of some rogue information from the C++ dll?
            // if the tool doesn't exist in the dictionary, then we shouldn't try
            // to find/update this entry.
            if (!ToolDictionary.ContainsKey(toolID)) continue;

            // now dump all the decoded double data into a \p TrackedTool struct
            TrackedTool tool;
            lock (toolDictLock)
            {
                // grab the tool from the dictionary to update its values
                tool = ToolDictionary[toolID];

                // update relevant fields
                tool.Tool_HoloFrame_LH = tool_Holo_LH;
                tool.VisibleToHoloLens = visible2Holo;
                if (visible2Holo) tool.TimestampLastSeen = ScriptTimer.ElapsedMilliseconds;

                // update the dictionary's entry, as the tool struct is a value type
                ToolDictionary[toolID] = tool;
            }
        }
    }

    /// <summary>
    /// Get a copy of the dictionary to print elsewhere or to analyse
    /// </summary>
    /// <returns>Internally stored tool dictionary</returns>
    public IReadOnlyDictionary<int, TrackedTool> GetToolDictionary()
    {
        return ToolDictionary;
    }
}
