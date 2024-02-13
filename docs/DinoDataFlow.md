# Data Flow For DINO Unity

This project interacts with `DINO-DLL` by providing some input configuration info about tools, and it also receives encoded information about 'TrackedTool' pose ([check here to see what 'defines' a TrackedTool](TrackedTools.md))

There are some important files for setting up and running your Unity project properly:

1. `DinoConfig.json`: A config file which contains information about your tool geometries and IDs. 
> **Note:**  For now, this is assumed to be stored in your `StreamingAssets` folder, as this will be directly copied over to your HL2, which stays synchronised with the state of your Unity project.

2. `DinoEditorSetup.cs`: An Editor script, which will setup your Unity scene to add a Unity GameObject & Transform for each TrackedTool declared in `DinoConfig.json`. It tells `ResearchModeController.cs` the name of the config file to use from the `StreamingAssets` folder, and it also sets up `UnityToolManager.cs`, which maintains a list of TrackedTools

3. `ResearchModeController.cs`: A script which communicates with the C++ [DINO DLL](https://github.com/HL2-DINO/DINO-DLL) running on the HoloLens 2.

4. `UnityToolManager.cs`: A file which will iterate over its own internal 'list' of TrackedTool objects and position Unity transforms based on information received from `ResearchModeController.cs`

## Config File Logic

```dot
digraph G {
  graph [fontname="Helvetica,Arial,sans-serif", fontsize=10];

    node [shape=box];
        node[shape=rectangle, height=0.7, width=1.2];
        
        {
        rank=same;
        api_b[label="UnityTool\nManager.cs" style="filled" 
                fontname="Helvetica,Arial,sans-serif", fontsize=12];

        api_c[label="DinoEditorSetup.cs" style="filled" 
                fontname="Helvetica,Arial,sans-serif", fontsize=12];

        api_d[label="ResearchMode\nController.cs" style="filled" 
                fontname="Helvetica,Arial,sans-serif", fontsize=12]
        }
        
        {
            api_a[label="config\nFile.json" shape=note 
            style="filled"
            fontname="Helvetica,Arial,sans-serif", fontsize=12];
            
            api_a -> api_c
            [labelloc=c label="  Read config file\n properties" 
            fontname="Helvetica,Arial,sans-serif", fontsize=10];
            
            api_c -> api_d
            [labelloc=c label="Set filepath for\n configFile.json" 
            fontname="Helvetica,Arial,sans-serif", 
            fontsize=10];
            
            api_b -> api_c
            [labelloc=c label="Populate internal\n 'TrackedToolList'" 
            fontname="Helvetica,Arial,sans-serif", fontsize=10 
            dir=back];
        }
}
```
<br>
<br>

## Tool Pose Logic
<br>
```dot
digraph SEQ_DIAGRAM {
    graph [overlap=true, splines=line, nodesep=1.0, ordering=out];
    edge [arrowhead=none];
    node [shape=none, width=0, height=0, label=""];

    {
        rank=same;
        node[shape=rectangle, height=0.7, width=1.2];
        api_a[label="HL2Dino\nPlugin.dll" style="filled" shape=component fontname="Helvetica,Arial,sans-serif", fontsize=12];
        api_b[label="ResearchMode\nController.cs" style="filled" fontname="Helvetica,Arial,sans-serif", fontsize=12];
        api_c[label="UnityTool\nManager.cs" style="filled" fontname="Helvetica,Arial,sans-serif", fontsize=12];
        api_d[label="Unity Transforms" style="filled" fontname="Helvetica,Arial,sans-serif", fontsize=12]
    }
    // Draw vertical lines
    {
        edge [style=dashed, weight=6];
        api_a -> a1;
        a1 -> a2 [penwidth=5, style=solid];
        a2 -> a3 -> a4 -> a5;
    }
    {
        edge [style=dashed, weight=6];
        api_b -> b1 -> b2;
        b2 -> b3 [penwidth=5; style=solid];
        b3 -> b4 -> b5;
    }
    {
        edge [style=dashed, weight=6];
        api_c -> c1 -> c2 -> c3; 
        c3 -> c4 [penwidth=5; style=solid];
        c4 -> c5;
    }
        {
        edge [style=dashed, weight=6];
        api_d -> d1 -> d2 -> d3 -> d4 -> d5;
    }
    // Draws activations
    { rank=same; a1 -> b1 [label="Query DLL for info" dir=back arrowhead=normal 
                           fontname="Helvetica,Arial,sans-serif", fontsize=10]; }
    { rank=same; a2 -> b2 [label="Expose encoded tool\n data and sensor images", arrowhead=normal 
                           fontname="Helvetica,Arial,sans-serif", fontsize=10]; }
    { rank=same; b3 -> c3 [arrowhead=normal, label="Pass encoded double\n array of tool-pose data"
                           fontname="Helvetica,Arial,sans-serif", fontsize=10]}
    { rank=same; c4 -> d4 [arrowhead=normal, label="Parse data, set position/rotation\n of Unity GameObjects"
                           fontname="Helvetica,Arial,sans-serif", fontsize=10]}
    //{ rank=same; a4 -> b4 [label="distribute()", arrowhead=normal]; }
    //{ rank=same; a5 -> b5 [style=invis]; b5 -> c5 [label="bill_order()", arrowhead=normal]; }
}
```
