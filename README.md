<a name="readme-top"></a>
<div align="center">
    <a href="https://github.com/Plateforme-VR-ENISE/PLUME">
        <picture>
            <source media="(prefers-color-scheme: dark)" srcset="/Documentation~/Images/plume_banner_dark.png">
            <source media="(prefers-color-scheme: light)" srcset="/Documentation~/Images/plume_banner_light.png">
            <img alt="PLUME banner." src="/Documentation~/Images/plume_banner_dark.png">
        </picture>
    </a>
    <br />
    <br />
    <p align="center">
        <strong>PLUME:</strong> <strong>PL</strong>ugin for <strong>U</strong>nity to <strong>M</strong>onitor <strong>E</strong>xperiments.
        <br />
        <a href="https://github.com/Plateforme-VR-ENISE/PLUME/Documentation~/"><strong>Explore the docs »</strong></a>
        <br />
        <br />
        <a href="https://github.com/Plateforme-VR-ENISE/PLUME">View Demo</a>
        ·
        <a href="https://github.com/Plateforme-VR-ENISE/PLUME/issues">Report Bug</a>
        ·
        <a href="https://github.com/Plateforme-VR-ENISE/PLUME/issues">Request Feature</a>
    </p>
</div>

<details>
    <summary>Table of Contents</summary>
    <ol>
        <li>
            <a href="#about-the-project">About The Project</a>
        </li>
        <li>
            <a href="#getting-started">Getting Started</a>
            <ul>
                <li><a href="#prerequisites">Prerequisites</a></li>
                <li><a href="#installation">Installation via Unity Package Manager</a></li>
                <li><a href="#manual-installation">Manual Installation</a></li>
                <li><a href="#development-installation">Installation for development</a></li>
            </ul>
        </li>
        <li><a href="#usage">Usage</a></li>
        <li><a href="#settings">Settings</a></li>
        <li><a href="#roadmap">Roadmap</a></li>
        <li><a href="#contributing">Contributing</a></li>
        <li><a href="#license">License</a></li>
        <li><a href="#contact">Contact</a></li>
    </ol>
</details>

## About

The PLUME Recorder is the cornerstone of the <a href="https://github.com/liris-xr/PLUME">PLUME</a> toolbox. It's a plugin for <a href="https://unity.com/">Unity</a> that continuously records the state of the virtual environment with minimal impact on performances. By default, the recorder will record as much data as possible, namely object position, appearance, sound, interactions, and physiological signals (through a LabStreamingLayer integration). The recorder is modular and allows custom data recording, such as event markers or custom-defined data structures in Google Protocol Buffer files. We use ProtoBuf for its fast and frugal serialization as well as being platform-neutral and can be de-serialized with any programming language. The PLUME Recorder is compatible with Windows, Android, and iOS and de facto with standalone devices. To record specific XR data, we rely on OpenXR as much as possible, making our plugin compatible with most HMDs. As the record files all follow the same serialization process and data format, they can be used interoperably across devices. For example, one could record an experiment on a standalone Android device and open the record file on a Windows machine.

## Getting Started

### Prerequisites
Before using installing this package, ensure you have the following:
* Unity 2022 or later installed

### Installation via Unity Package Manager
1. Open your Unity project.
2. Open the Package Manager window from `Window > Package Manager`.
3. Click on the `+` button at the top left of the Package Manager window.
4. Select "Add package from git URL...".
5. Paste the following URL into the text field: `https://github.com/liris-xr/PLUME-Recorder.`
6. Click on the `Add` button.
7. Unity will now download and import the package into your project.

### Manual Installation
1. Download latest release (`.unitypackage` extension)
2. Open your Unity project.
3. Locate the downloaded package file.
4. Double-click the package file to import it into your Unity project.

### Development
1. Clone or download the repository inside the `Packages` folder of your Unity project.
2. Unity will import the package into your project.
3. You can now edit the source code to adapt it to your needs. Feel free to <a href="#contributing">contribute</a> to this repository !

## Usage
PLUME Recorder is ready to use right after the installation. By default it automatically starts recording when running the application (Play Mode or Build). Recordings are located in the Application Path defined by Unity, e.g., on Windows the Application Path is: `%AppData%/LocalLow/Company Name/Project Name`.


## Settings
Open the PLUME Recorder settings window from `PLUME > Settings`.
From here, you can configure the recorder, e.g, set the recording folder, enable or disable recorder modules.

## Roadmap

See the [open issues](https://github.com/Plateforme-VR-ENISE/PLUME/issues) for a full list of proposed features (and
known issues).

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any
contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also
simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the <a href="https://github.com/liris-xr/PLUME-Recorder/blob/master/LICENSE.md">GPLv3 License</a>.

## Research Article
[ACCEPTED IEEEVR 2024 JOURNAL TRACK] C. Javerliat, S. Villenave, P. Raimbaud, et G. Lavoué, « PLUME: Record, Replay, Analyze and Share User Behavior in 6DoF XR Experiences », IEEE Trans. Visual. Comput. Graphics, p. 1‑9, 2024.


## Contact

Charles JAVERLIAT - charles.javerliat@gmail.com

Sophie VILLENAVE - sophie.villenave@enise.fr