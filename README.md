# HL2-DINO-Unity2019
<p align="center">
  <img src="docs/img/logo_dark.png#gh-dark-mode-only" width="95%"/>
  <img src="docs/img/logo_light.png#gh-light-mode-only" width="95%"/>  
</p>

<!DOCTYPE html>
<html>
<body>
   <h3 align="center"><strong>HoloLens 2 &amp; <ins>D</ins>etection for <ins>I</ins>nfrared <ins>N</ins>avigation with <ins>O</ins>ST AR headsets</strong></p>

  <p align="center">
    <a href="#overview">Overview</a> •
    <a href="#requirements">Requirements</a> •
    <a href="#getting-started">Getting Started</a> •
    <a href="#license">License</a> •
    <a href="#acknowledgements">Acknowledgements</a>
  </p>
</body>
</html>
 
## Overview

A sample Unity 2019 project which consumes the [`DINO-DLL`](https://github.com/HL2-DINO/DINO-DLL) project and creates a Unity app for the HoloLens 2 to position virtual models based on infrared tracking data.

<html>
<div align="center">
  <img src="docs/img/toolWiggle.gif" width="50%" height="auto">
   <p align="center"><em>User app experience recorded with Mixed Reality Capture</em></p>
</div>
</html>

### What does this app do?
This project can be used in its current state to allow the HoloLens 2 to track two tools which can be 3D printed and assembled as described in [`DINO-IR-Tools`](
https://github.com/HL2-DINO/DINO-IR-Tools). 

### How do I use and customise the app?
Check out the [docs/wiki]() for more information on topics like:

1. What's included in this app
1. How to print performance/profiling data for the IR tracking
1. Adding your own custom tools/geometries
1. How information is exchanged between the C++ [`DINO-DLL`](https://github.com/HL2-DINO/DINO-DLL) and `DINO-Unity`

## Requirements

1. Unity 2019 (2019.4.22f1 was used for this project)
1. Enable Research Mode on your HoloLens 2: [see tutorial here](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/advanced-concepts/research-mode#enabling-research-mode-hololens-first-gen-and-hololens-2)
1. Visual Studio (for this project, [Visual Studio 2022 configured for Unity development](https://learn.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/getting-started-with-visual-studio-tools-for-unity?pivots=windows) was used during testing)
1. The project has a dependency on **Newtonsoft JSON.NET**, which was included through Unity's Package Manager: *[Add package from git URL] -> 'com.unity.nuget.newtonsoft-json'*
1. 3-D printing and assembling two tools, as included in [`DINO-IR-Tools`](
https://github.com/HL2-DINO/DINO-IR-Tools).  


## Getting Started
> **Note:** These steps assume no changes to the Unity project. If you need to change tool geometries, [double check here](docs/Tutorials.md) to make sure you've configured everything properly.

1. If you have made any changes to [`DINO-DLL`](https://github.com/HL2-DINO/DINO-DLL), then make sure you copy over the latest `HL2DinoPlugin.winmd` and `HL2DinoPlugin.dll` files into the folder: `Assets/Plugins/WSA`

1. Make sure you're targeting `Universal Windows Platform` before building

1. Unity build settings: 
    - Target Device: HoloLens
    - Architecture: ARM64
    - Build Type: D3D Project

1. Build, and then open the Visual Studio `.sln` file. Add the 'restricted capability' to the `Package.appxmanifest` file.
    As per the instructions provided on the [HoloLens2-ResearchMode-Unity](https://github.com/petergu684/HoloLens2-ResearchMode-Unity/tree/master) README.md by petergu684, make the following modifications to your `Package.appxmanifest`.
    
    Add `xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities" ` and `IgnorableNamespaces="uap uap2 uap3 uap4 mp mobile iot rescap"` to the file, and add in the necessary capabilities.
    
    Your resulting app manifest should look something like:
    ```xml 
    <Package 
      xmlns:mp="http://schemas.microsoft.com/appx/2014/phone/manifest" 
      xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" 
      xmlns:uap2="http://schemas.microsoft.com/appx/manifest/uap/windows10/2" 
      xmlns:uap3="http://schemas.microsoft.com/appx/manifest/uap/windows10/3" 
      xmlns:uap4="http://schemas.microsoft.com/appx/manifest/uap/windows10/4" 
      xmlns:iot="http://schemas.microsoft.com/appx/manifest/iot/windows10" 
      xmlns:mobile="http://schemas.microsoft.com/appx/manifest/mobile/windows10" 
      xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities" 
      IgnorableNamespaces="uap uap2 uap3 uap4 mp mobile iot rescap" 
      xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"> 
    ```
    ```xml
      <Capabilities>
        <rescap:Capability Name="perceptionSensorsExperimental" />
        <Capability Name="internetClient" />
        <Capability Name="internetClientServer" />
        <Capability Name="privateNetworkClientServer" />
        <uap2:Capability Name="spatialPerception" />
        <DeviceCapability Name="webcam" />
      </Capabilities>
    ```

1. Ensure your `.sln` is set up to build for `Release` and `ARM64`, and then deploy to the HoloLens
1. You may need to restart your headset the first time you deploy. To do this, just use the standard voice command `"Restart device"`  
1. After restarting, if everything has gone to plan, you should see something like the GIF in the Overview section (e.g. updating sensor images and tool tracking)

## License

This project is licensed under the [BSD License](LICENSE). 

## Acknowledgements

* If this project is useful for your research or work, please considering citing the following publication<sup>[1]</sup>:
  ```bibtex
  @inproceedings{Iqbal2022,
  author = {Hisham Iqbal and Ferdinando Rodriguez y Baena},
  journal = {2022 IEEE/RSJ International Conference on Intelligent Robots and Systems (IROS 2022)},
  title = {Semi-Automatic Infrared Calibration for Augmented Reality Systems in Surgery},
  year = {2022},
  }
  ```

* A note of thanks for:
  * Prof Ferdinando Rodriguez y Baena: for project supervision and for being a source of great advice
  * [Andreas Keller](https://github.com/andreaskeller96): for kindly helping out with testing software compilation. See Andreas's excellent [HoloLens2-IRTracking](https://github.com/andreaskeller96/HoloLens2-IRTracking) project for an alternative setup which achieves similar goals.

* The project structure for consuming a C++/WinRT component and logic for handling incoming DLL data was inspired by [HoloLens2-ResearchMode-Unity](https://github.com/petergu684/HoloLens2-ResearchMode-Unity/tree/master) by petergu684. Check it out for a very good insight into how you can visualise other sensor-streams from the HoloLens 2.

## Additional Notes
### App Manifest
The following sources were all useful for understanding how to modify the appxmanifest file: 
  * The [HoloLens2-ResearchMode-Unity](https://github.com/petergu684/HoloLens2-ResearchMode-Unity/tree/master) project README. 
  * An example appxmanifest file from the [HoloLens2ForCV repo](https://github.com/microsoft/HoloLens2ForCV/blob/main/Samples/CalibrationVisualization/CalibrationVisualization/Package.appxmanifest) 
  * This [blog-post](https://mtaulty.com/2018/04/06/experimenting-with-research-mode-and-sensor-streams-on-hololens-redstone-4-preview/) by Mike Taulty

### To-Do List
There are a few features which are yet to be implemented and are left for future TODO's.

On the Unity side:
- [ ] Make the palm UI menu voice-activated
- [ ] Moving the tool config file location into a place where users can easily change tool geometries at runtime without having to recompile the app.

***
[^1]: Iqbal H., Rodriguez y Baena, F. (2022) Semi‑Automatic Calibration for Augmented Reality Systems in Surgery.
2022 IEEE/RSJ International Conference on Intelligent Robots and Systems dx.doi.org/10.1109/IROS47612.2022.9982215

