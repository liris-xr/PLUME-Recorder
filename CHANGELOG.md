# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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