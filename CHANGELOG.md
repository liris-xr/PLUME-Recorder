# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.2-beta]

### Fixed

- Fixed issue [#4](https://github.com/liris-xr/PLUME-Recorder/issues/4) where the `Object.Instantiate(T obj, Scene scene)` function was causing a compilation error in Unity 2023 (missing from the Unity 2023 API).

## [1.2.1-beta]

### Fixed

- Fix missing import for URP samples in SampleUtils.cs

## [1.2.0-beta]

### Added

- Added recorder extensions `RecordTimestampedManagedSample`, `RecordTimestampedSample`, `RecordTimelessManagedSample` and `RecordTimelessSample` to easily record samples from any script.
- Print the record path in the console when starting recording.
- Implemented the auto-start feature.
- Added the default record prefix setting.
- Added the default record metadata setting.
- Added the audio recorder module enable/disable setting.
- Added the audio recorder module setting to log when silence is inserted in the WAV file for synchronization with the recorder clock.
- Added hooks settings to choose which assemblies are injected with the recorder hooks.

### Changed

- Updated the hooks injection system to make it more flexible, allowing for injecting generic methods more easily.
- Update submodule Unity-Runtime-GUID to version v1.0.2.
- Disable the audio recorder module by default.
- Improved performances when starting the recording.

### Removed

- Removed unused game object recorder module settings.

### Fixed

- Fixed custom recorder modules not being picked up by the recorder.
