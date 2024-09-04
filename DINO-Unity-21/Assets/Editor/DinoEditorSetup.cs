using UnityEngine;
using UnityEditor;
using ToolTrackingUtils;
using ToolConfigUtilities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

/** @file           DinoEditorSetup.cs
 *  @brief          An Editor helper script which helps to properly populate the ToolsTrackedByHololens member of a chosen
 *                  \p UnityToolManager script that you specify. Generally, this will help setup your app based on data read
 *                  in from a correctly formatted JSON config object. Should be used as the only interaction point between Unity
 *                  and the tool config object.
 *
 *  @author         Hisham Iqbal
 *  @copyright      &copy; 2023 Hisham Iqbal
 */

public class DinoEditorSetup : EditorWindow
{
    /// <summary>
    /// A parent GameObject which will house all of the GameObjects associated with each of our tracked tools
    /// </summary>
    Transform TrackedToolsParentTransform;

    /// <summary>
    /// Full filepath for where the JSON config file is
    /// </summary>
    string JSONPath = "";

    /// <summary>
    /// A UnityToolManager script which should be part of your scene
    /// </summary>
    UnityToolManager toolManagerInstance;

    /// <summary>
    /// The script which communicates with the C++ DLL directly, should be part of your scene
    /// </summary>
    ResearchModeController rmControllerInstance;

    /// <summary>
    /// An internally stashed JObject equivalent of the config object contained in \p JSONPath
    /// </summary>
    private JObject ToolConfigJson;

    [MenuItem("DINO Unity/DINO Setup")]
    public static void Apply()
    {
        EditorWindow.GetWindow(typeof(DinoEditorSetup));
    }

    void LaunchFilePicker()
    {
        JSONPath = EditorUtility.OpenFilePanel("Pick tool config json file", Application.dataPath, "json");
    }

    /// <summary>
    /// Creates a bunch of GameObjects based on the tool config file info, and will also create some small spheres to 
    /// appear at the centre of each tool-marker centre
    /// </summary>
    void PopulateObjects()
    {
        if (toolManagerInstance == null) { Debug.LogError("No UnityToolManager specified"); return; }
        if (TrackedToolsParentTransform == null) { Debug.LogError("No parent transform specified"); return; }
        if (ToolConfigJson == null) { Debug.LogError("ToolConfig JSON was not read in properly"); return; }

        // set the public List after parsing the JSON
        toolManagerInstance.ToolsTrackedByHololens = JSONUtils.CreateTrackedToolsetFromJSON(ToolConfigJson);

        List<TrackedTool> toolsList = toolManagerInstance.ToolsTrackedByHololens;

        // populate a list of TrackedTools based on information read in from the config file
        for (int i = 0; i < toolsList.Count; i++)
        {
            var tool = toolsList[i];
            // does the tool exist?
            GameObject toolGameObject;

            var existingObject = TrackedToolsParentTransform.Find(tool.ToolName);

            // if you've already run this script in the editor, then grab the existing GameObject
            toolGameObject = (existingObject == null) ? 
                new GameObject(tool.ToolName) : existingObject.gameObject;

            toolGameObject.transform.parent = TrackedToolsParentTransform;
            toolGameObject.transform.localScale = Vector3.one;
            toolGameObject.transform.localPosition = Vector3.zero;
            toolGameObject.transform.localRotation = Quaternion.identity;
            tool.ToolUnityTransform = toolGameObject.transform;
        }

        // add in the marker centres so we can see them in Unity, note this function call will delete
        // any child objects under the name 'tracker'
        UnitySceneSetup.AddMarkerCentreSpheres(toolsList);
    }


    void OnGUI()
    {
        GUILayout.Label("DINO Settings", EditorStyles.boldLabel);

        GUILayout.Space(10);
        toolManagerInstance = (UnityToolManager)EditorGUILayout.ObjectField("(1) Pick ToolManager", toolManagerInstance, typeof(UnityToolManager), true);

        GUILayout.Space(10);
        rmControllerInstance = (ResearchModeController)EditorGUILayout.ObjectField("(2) Pick ResearchModeController",
            rmControllerInstance,
            typeof(ResearchModeController), true);

        GUILayout.Space(10); // Add some vertical spacing     
        EditorGUILayout.LabelField("(3) Select JSON file");
        if (GUILayout.Button("Open file picker"))
        {
            LaunchFilePicker();
            Debug.Log($"JSON path selected: {JSONPath}");
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("JSON selected filename:");
        EditorGUILayout.LabelField($"{System.IO.Path.GetFileName(JSONPath)}");

        GUILayout.Space(10); // Add some vertical spacing
        EditorGUILayout.LabelField("(4) Parent transform for all TrackedTools");
        TrackedToolsParentTransform = (Transform)EditorGUILayout.ObjectField(TrackedToolsParentTransform, typeof(Transform), true);

        GUILayout.Space(10); // Add some vertical spacing    
        EditorGUILayout.LabelField("(5) Populate Objects");
        if (GUILayout.Button("Create Objects & Apply JSON Settings"))
        {
            RetrieveJSONProperties();
            PopulateObjects();
        }
    }

    /// <summary>
    /// Will try to create our internal JObject by reading filepath
    /// </summary>
    private void RetrieveJSONProperties()
    {
        if (JSONPath == "") { Debug.LogError("No JSON filepath specified"); return; }
        if (toolManagerInstance == null) { Debug.LogError("No UnityToolManager specified"); return; }

        ToolConfigJson = JSONUtils.TryReadingToolConfigJSON(JSONPath);
        if (ToolConfigJson == null) { Debug.LogError("TryReadingToolConfigJSON error thrown"); return; }

        if (rmControllerInstance == null) { Debug.LogError("Please assign an instance of ResearchModeController from Dino Unity > Dino Editor Setup"); return; }
        // if it gets here, then it's passed the checks we placed, so set the filepath 
        rmControllerInstance.JSONFilename = System.IO.Path.GetFileName(JSONPath);

    }
}
