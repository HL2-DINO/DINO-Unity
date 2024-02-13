# Tutorials

## 1. Using the DINO-Unity app

### Scene Info
The project contains two scenes:

1. `SampleScene.scene` Bare-bones app, just displays info and positions virtual objects based on marker location
2. `SampleSceneMRTK.scene` Does everything that `SampleScene` does, but has MRTK interactions set up and enabled, so you can resize windows, and interact with the DLL through 3D buttons

Both scenes contain three main groups of objects. 

1. Displays of sensor data, grouped under an object called RMContainer. This is just a 2D display of sensor images from the AHAT sensor. The shader used for this image originates from [Hololens2-ResearchMode-Unity](https://github.com/petergu684/HoloLens2-ResearchMode-Unity/).
2. Two debug text canvases: 
    * `CanvasText` has been used as a very basic error printer, and is also used to retrieve a profiler string from `DINO-DLL`, giving you more info about how long each image processing step takes.
    * `ToolDictionaryPrinter` which prints a 4x4 matrix for each tool you're tracking. 

    > **Note**: The two canvases and sensor displays are all grouped under `RMContainer`, so you can move around sensor images and debug string canvases as one big object. This will be referred to as a 'visualiser' for the rest of the doc.
3. Tracked tools: virtual models which appear rigidly fixed to your tools     

### Running the App

By default, the app has been set up to compile and run `SampleSceneMRTK.scene` on the HoloLens 2. 

Once you've deployed the app to your HoloLens 2 (see README.md in the repo for instructions on this), you have a basic UI and some buttons which you can use.

![Bringing up the palm-attached UI, by showing the inside of your flattened left palm](img/uiscreengrab.jpg)

To use these buttons, open up and face your left palm towards the headset, and a little palm menu with 3 buttons should appear. You can press these buttons with air-taps or by physically poking them with your right index finger. 

<strong>(1) Print Profiler</strong>

This button will dump a profiler string to a file located at `ApplicationData.Current.LocalFolder.Path`. This provides timing info on processes being carried out on the C++ DLL side. You can decorate the DLL code with calls to the Shiny profiler, which will change what information is profiled. 

When you hit the button, the string will print to the canvas, and be automatically saved to the local data store for the app, which you can access from the Windows Device Portal. Once you connect to the headset through the Device Portal, go to the following location (e.g. for Unity 2019):

`LocalAppData\Dino-Unity-2019_1.....\LocalState\`

![](img/saved_file.PNG)

Some basic profiler info we've collected when tracking multiple tools is included at this [location??]()

<strong>(2) Fetch Visualiser</strong>

This button is intended to retrieve the visualiser for the sensor images and the debug text if you move around in space. It will simply move the visualiser to face the user and place it about a metre away.

<strong>(3) Toggle Sensor Data</strong>

In theory `DINO-DLL` will always be processing sensor images from the headset to detect tools. But we can choose to toggle whether these images are actually updated/processed for display on the Unity side. By toggling this off, you can save some basic image processing steps which can help speed the app up marginally. When the toggle is off, we stop displaying sensor images, and vice versa.

## 2. TrackedTool Definition {#tracked-tool}

Universally across the DINO Unity app, we describe a trackable IR instrument with the following properties:

```cs
namespace ToolTrackingUtils
{
    public class TrackedTool
    {
        public int ToolID;
        public string ToolName;
        public List<Vector3> ToolMarkerTriplets;
        public Transform ToolUnityTransform;
        public Matrix4x4 Tool_HoloFrame_LH;
        public bool VisibleToHoloLens;
        public float TimestampLastSeen;        
    }
}
```

In the lifecycle of the app, the properties are used as follows:

* `ToolID`: Used as a unique identifier in internal maps/dictionaries.
* `ToolName`: Used to name the GameObject associated with this tool in the Unity hierarchy.
* `ToolMarkerTriplets`: Internal storage of all the marker-centre locations (in left handed coordinates).
* `ToolUnityTransform`: Unity transform corresponding to the tool that we will position. 
This transform can be used as a parent for things we want to attach to the TrackedTool (3D models or other key-points).
* `Tool_HoloFrame_LH`: A 4x4 transform matrix which describes where the tool is with respect to the HoloLens 2's world frame 
(a coordinate frame located at the startup position and rotation the headset).
* `VisibleToHoloLens`: Bool which reflects the visiblity of the tool to the headset, set from the DLL side.
* `TimestampLastSeen`: A float value which is updated on the Unity side to track. 
At the moment, this is just used to track if the tool has not been seen for a number of seconds.

## 3. JSON Object Formatting {#json-object-formatting}
This app reads in a JSON object from the `StreamingAssets` folder in a particular format to understand the geometries of tools
we're interested in tracking.

```json
{
"fileSettings": {"units" : "mm"},
"tools": [
    {"name": "Stylus",
    "id": 8,
    "coordinates":
    [["0","0","0"],
    ["0","70","0"],
    ["0","137.01","0"],
    ["-39.39","91.75","0"],
    ["47.04","92.16","0"]]},
    
    {"name": "Triangle",
    "id": 9,
    "coordinates":
    [["0","0","0"],
    ["97.29","-11.77","0"],
    ["112.23","38.4","0"]]}
]
}
```
There are two top-level file-keys:
`fileSettings` and `tools`.

### File Settings

You can choose to specify file units as metres (`m`) or millimetres (`mm`).

### Tools

The `tools` field contains an array of key-value packets. Each array member, which is a single tool,
should be structured like this:

```json
    {
        "name": "Stylus",
        "id": 8,
        "coordinates":
            [
             ["0","0","0"],
             ["0","70","0"],
             ["0","137.01","0"],
             ["-39.39","91.75","0"],
             ["47.04","92.16","0"]
            ]
    }

```
| Key           |                                                       Description                                                  |
|---------------|--------------------------------------------------------------------------------------------------------------------|
|`name`         | A string identifier, used to label the Transform associated with this tool in Unity                                |
| `ID`          | A unique numeric value (should be between 0-255), which is used to identify this tool in internal maps/dictionaries|
| `coordinates` | An array composed of 'coordinate' values, telling you the 3D location of each marker attached to the tool.         |

> **Note:** A single 'coordinate' is a 3D vector. So a tool with 5 markers, would have 5 entries in the `coordinates` array. 
Each coordinate itself is an array, containing 3 string values, ordered as [x,y,z]. This value corresponds to the centre of each IR 
reflective marker. 
**IMPORTANT**: The units should be consistent within a single JSON file for all tools. These values should follow a right-handed convention. 
Left handed conversion is done inside Unity by inverting the 'z' component. 

## 4. Importing Custom 3D Models 

By default, an `.obj` file is defined to have right-handed vertex data. When importing into Unity, there appears to be an automatic process carried out by the Unity editor which inverts all x coordinate data.

In this app, the convention is to invert the z values of all right-handed coordinates to go into Unity's left-handed coordinate system ([check this link](https://learn.microsoft.com/en-us/windows/mixed-reality/design/coordinate-systems#spatial-coordinate-systems) for a good reference diagram).

So if you want to import in a 3D model into Unity for this app: you need to account for the following:

1. Make sure your 3D model is in the same coordinate system as your tool config data, and that they share the same origin.

2. Inside the inspector for your 3D model on Unity, you'll need to set a rotation around the y-axis of 180 degrees from the inspector on Unity, to account the automatic x inversion done on import.


## 5. Tracking Custom Geometries {#custom-geometries}

(1) Make sure you place [a properly formatted JSON file](#json-object-formatting) in the `StreamingAssets` folder

(2) In your Unity scene, make sure you have exactly one instance of `ResearchModeController.cs` and `UnityToolManager.cs`

(3) From the menu-bar: *[DINO Unity] -> [DINO Setup]*:

![Blank unpopulated DINO Setup menu](img/blankDinoEditorSetup.PNG)

(4) Populate fields 1 & 2, click on the little target icon on the right to automatically pop up a menu that lets you select your scene's instances of `ResearchModeController.cs` and `UnityToolManager.cs`

(5) Use the file-picker to select whichever config file inside your `StreamingAssets` folder you want to compile the app with

(6) Select a parent transform for a GameObject under which all your 'TrackedTools' will be grouped. The name or object you choose shouldn't matter too much, this is just so we can place all the TrackedTools under one parent object.

(7) Hit the button to populate objects, which will: 
    
  A. Tell your instance of `ResearchModeController.cs` where to look in the `StreamingAssets` folder for a config file, and 
    
  B. Properly format and populate the `ToolsTrackedByHololens` field in your instance of `UnityToolManager.cs`

  ![Populated DINO Settings](img/filledDinoEditorSetup.PNG)

(8) Review your Unity scene, you should see the above properties have changed in your scripts.

  ![Properly initialised UnityToolMananger.cs script](img/UnityToolManager_example.PNG)

  <br></br>

  ![Object hierarchy in Unity scene, with two tracked tools set up as per the example config file below](img/hierarchy_example.PNG)

Example config file used:

```json
{
"fileSettings": {"units" : "mm"},
"tools": [
    {"name": "Stylus",
    "id": 8,
    "coordinates":
    [["0","0","0"],
    ["0","70","0"],
    ["0","137.01","0"],
    ["-39.39","91.75","0"],
    ["47.04","92.16","0"]]},
    
    {"name": "Triangle",
    "id": 9,
    "coordinates":
    [["0","0","0"],
    ["97.29","-11.77","0"],
    ["112.23","38.4","0"]]}
]
}
```

