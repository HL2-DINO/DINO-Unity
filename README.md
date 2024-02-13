# DINO-Unity Homepage
![](img/logo_dark.png#gh-dark-mode-only)
![](img/logo_light.png#gh-light-mode-only)

<!DOCTYPE html>
<html>
<body>
   <h3 align="center"><strong>HoloLens 2 &amp; <ins>D</ins>etection for <ins>I</ins>nfrared <ins>N</ins>avigation with <ins>O</ins>ST AR headsets</strong></p>

  <p align="center">
    <a href="#overview">Overview</a> •
    <a href="#requirements">Requirements</a> •
    <a href="#getting-started">Getting Started</a> •
    <a href="#acknowledgements">Acknowledgements</a>
  </p>
</body>
</html>
 
## Overview

This repository supplies you with a sample Unity project which consumes the [`DINO-DLL`](https://github.com/HL2-DINO/DINO-DLL) project and creates a Unity app for the HoloLens 2 to position virtual models based on infrared tracking data.

<html>
<div align="center">
  <img src="img/toolWiggle.gif" width="50%" height="auto">
   <p align="center"><em>User app experience recorded with Mixed Reality Capture</em></p>
</div>
</html>

DINO-Unity has been developed and released with 3 LTS versions of Unity: 2019.4.22f1, 2020.3.42f1, and 2021.3.2f1. 

At the time of release of this project (2023), Microsoft's [recommendation for a Unity version is the Unity 2021.3 LTS release](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/choosing-unity-version).

***Note***: The main branch of this repo is just a landing page, if you want to take a look at implementations for the three major versions of Unity, use links below:

* [GitHub branch for Unity 2019](https://github.com/HL2-DINO/DINO-Unity/tree/unity-19)
* [GitHub branch for Unity 2020](https://github.com/HL2-DINO/DINO-Unity/tree/unity-20)
* [GitHub branch for Unity 2021](https://github.com/HL2-DINO/DINO-Unity/tree/unity-21)

To get up and running on your own machine, follow the instructions in [Getting Started](#getting-started).


## Getting Started

1. Determine which major version of Unity you're targeting (let's assume it's 2021).

1. Clone the repo and switch to the relevant branch (e.g. Unity 2021).

    ``` bash
    git clone https://github.com/HL2-DINO/DINO-Unity
    cd DINO-Unity
    git checkout unity-21
    ```
    You should now have a local copy of the source code needed to open and your chosen version of the `DINO-Unity` app using the Unity Editor.

1. Read the relevant README file for detailed instructions on building and deploying the Unity application to your HoloLens 2. You can read it locally on your machine, or access them from here:
    * [Link to DINO-Unity-2019 branch](https://github.com/HL2-DINO/DINO-Unity/tree/unity-19)
    * [Link to DINO-Unity-2020 branch](https://github.com/HL2-DINO/DINO-Unity/tree/unity-20)
    * [Link to DINO-Unity-2021 branch](https://github.com/HL2-DINO/DINO-Unity/tree/unity-21)

1. Read the [docs/wiki](https://hl2-dino.github.io/DINO-Unity/) for more info on how to use and customise the app.

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

***
[^1]: Iqbal H., Rodriguez y Baena, F. **(2022)** *Semi‑Automatic Calibration for Augmented Reality Systems in Surgery.*
2022 IEEE/RSJ International Conference on Intelligent Robots and Systems [dx.doi.org/10.1109/IROS47612.2022.9982215](https://dx.doi.org/10.1109/IROS47612.2022.9982215)
