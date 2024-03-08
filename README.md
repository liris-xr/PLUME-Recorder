<a name="readme-top"></a>
<div align="center">
    <a href="https://github.com/liris-xr/PLUME">
        <picture>
            <source media="(prefers-color-scheme: dark)" srcset="/Documentation~/Images/plume_banner_dark.png">
            <source media="(prefers-color-scheme: light)" srcset="/Documentation~/Images/plume_banner_light.png">
            <img alt="PLUME banner." src="/Documentation~/Images/plume_banner_light.png">
        </picture>
    </a>
    <br />
    <br />
    <p align="center">
        <strong>PLUME: Record, Replay, Analyze and Share User Behavior in 6DoF XR Experiences</strong>
        <br />
        Charles Javerliat, Sophie Villenave, Pierre Raimbaud, Guillaume Lavoué
        <br />
        <em>(Journal Track) IEEE Conference on Virtual Reality and 3D User Interfaces</em>
        <br />
        <a href="https://www.youtube.com/watch?v=_6krSw7fNqg"><strong>Video »</strong><a>
        <a href="https://hal.science/hal-04488824"><strong>Paper »</strong></a>
        <a href="https://github.com/liris-xr/PLUME-Recorder/wiki/"><strong>Explore the docs »</strong></a>
        <br />
        <br />
        <a href="https://github.com/liris-xr/PLUME-Recorder/issues">Report Bug</a>
        ·
        <a href="https://github.com/liris-xr/PLUME-Recorder/issues">Request Feature</a>
    </p>
</div>

<img alt="PLUME Recorder in Package Manager" src="/Documentation~/Images/package_manager.png">

<details>
    <summary>Table of Contents</summary>
    <ol>
        <li>
            <a href="#about-plume-recorder">About PLUME Recorder</a>
        </li>
        <li>
            <a href="#installation">Installation</a>
            <ul>
                <li><a href="#prerequisites">Prerequisites</a></li>
                <li><a href="#installation-via-unity-package-manager">Installation via Unity Package Manager</a></li>
                <li><a href="#manual-installation">Manual Installation</a></li>
                <li><a href="#installation-for-development">Installation for development</a></li>
            </ul>
        </li>
        <li><a href="#getting-started">Getting Started</a></li>
            <ul>
                <li><a href="#settings">Settings</a></li>
                <li><a href="#recompile-with-hooks">Recompile with Hooks</a></li>
                <li><a href="#build-asset-bundle">Build Asset Bundle</a></li>
                <li><a href="#records-location">Records Location</a></li>
            </ul>
        <li><a href="#customization">Customization</a></li>
            <ul>
                <li><a href="#event-marker">Event Marker</a></li>
                <li><a href="#custom-sample">Custom Sample</a></li>
            </ul>
        <li><a href="#roadmap">Roadmap</a></li>
        <li><a href="#contributing">Contributing</a></li>
        <li><a href="#license">License</a></li>
        <li><a href="#contact">Contact</a></li>
        <li><a href="#citation">Citation</a></li>
    </ol>
</details>

## About PLUME Recorder

PLUME Recorder is the cornerstone of the <a href="https://github.com/liris-xr/PLUME">PLUME</a> toolbox. It's a plugin for <a href="https://unity.com/">Unity</a> that continuously records the state of the virtual environment with minimal impact on performances. By default, the recorder will record as much data as possible, namely object position, appearance, sound, interactions, and physiological signals (through a LabStreamingLayer integration). The recorder is modular and allows custom data recording, such as event markers or custom-defined data structures in Google Protocol Buffer files. We use ProtoBuf for its fast and frugal serialization as well as being platform-neutral and can be de-serialized with any programming language. The PLUME Recorder is compatible with Windows, Android, and iOS and de facto with standalone devices. To record specific XR data, we rely on OpenXR as much as possible, making our plugin compatible with most HMDs. As the record files all follow the same serialization process and data format, they can be used interoperably across devices. For example, one could record an experiment on a standalone Android device and open the record file on a Windows machine.

## Installation

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

### Installation for Development
1. Clone or download the repository inside the `Packages` folder of your Unity project.
2. Unity will import the package into your project.
3. You can now edit the source code to adapt it to your needs. Feel free to <a href="#contributing">contribute</a> to this repository !

## Getting Started
### Settings
Open the PLUME Recorder settings window from `PLUME > Settings`.
From here, you can configure the recorder, e.g, set the recording folder, enable or disable recorder modules.

### Recompile with Hooks
After installation, force recompilation from `PLUME > Force Recompile With Hooks`.

### Build Asset Bundle
To build your project Asset Bundle, click on `PLUME > Build Asset Bundle`. The built Asset Bundle can be found in the Assets folder of your Unity Project: `Assets/StreamingAssets/plume_asset_bundle_windows`.

### Records Location
Records are located in the Application Path defined by Unity, e.g., on Windows, the Application Path is: `%AppData%/LocalLow/Company Name/Project Name`.

## Customization
### Event Marker
To record a custom event marker, write anywhere in your code:
```
PlumeRecorder.RecordMarker("Marker");
```

### Custom Sample
To record custom data, you need to create your own protos by following the instructions in the <a href="https://github.com/liris-xr/PLUME-Protos">PLUME Proto repository</a>

Your custom proto will look something like this:
```
syntax = "proto3";

package plume.sample.custom;
option csharp_namespace = "PLUME.Sample.Custom";

message MyCustomSample {
    float myData = 1;
}
```

Once your C# protos are generated as instructed <a href="https://github.com/liris-xr/PLUME-Protos/?tab=readme-ov-file#how-to-build">here</a>, import your protos in your Unity project.

To record your sample, create a Recorder Module. In this example, the module creates a MyCustomSample when the record starts.
```C#
using System;
using PLUME.Sample.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace MyNamespace
{
    public class MyCustomRecorder : RecorderModule, IStartRecordingEventReceiver
    {
        public new void OnStartRecording()
        {
            base.OnStartRecording();

            var variable = 42f;

            var myCustomSample = new MyCustomSample
            {
                MyData = variable;
            };

            recorder.RecordSampleStamped(myCustomSample);
        }

        protected override void ResetCache()
        {
        }
    }
}
```
You can get inspiration from existing Recorder Modules to create more complex modules.

## Roadmap

See the [open issues](https://github.com/Plateforme-VR-ENISE/PLUME-Recorder/issues) for a full list of proposed features (and
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

## Contact

Charles JAVERLIAT - charles.javerliat@gmail.com

Sophie VILLENAVE - sophie.villenave@enise.fr

## Citation
```
@article{javerliat_plume_2024,
	title = {{PLUME}: {Record}, {Replay}, {Analyze} and {Share} {User} {Behavior} in {6DoF} {XR} {Experiences}},
	url = {https://ieeexplore.ieee.org/document/10458415},
	doi = {10.1109/TVCG.2024.3372107},
	journal = {IEEE Transactions on Visualization and Computer Graphics},
	author = {Javerliat, Charles and Villenave, Sophie and Raimbaud, Pierre and Lavoué, Guillaume},
	year = {2024},
	note = {Conference Name: IEEE Transactions on Visualization and Computer Graphics},
	pages = {1--11}
}
```