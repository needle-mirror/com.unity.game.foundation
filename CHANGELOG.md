# Changelog

## [0.2.0-preview.3] - 2019-12-11

### Added

* Samples
* Debug window for visualizing data during Play Mode in the Editor
* Three new detail definition types:
  * Sprite Assets Detail
  * Prefab Assets Detail
  * Audio Clip Assets Detail
* Tools for creating custom detail definitions
* Ability to choose a Reference Definition while creating a new Inventory Item (which also pre-populates the Display Name and ID fields)
* Menu items that link to the documentation and the forums

### Changed

* Improved Stat Detail UI
* Icon Detail is now marked as obsolete and will be removed in a future version (please use Sprite Assets Detail instead)
* Currency Detail Type is now broken into Type and Sub-Type (with related UI change)
* Minor API performance optimizations
* Minor editor UI/UX improvements and optimizations

## [0.1.0-preview.6] - 2019-09-18

### Added

* Analytics system
* Support for serialization of runtime stats data
* "Auto-Create Instance" feature for the Inventory system

### Changed

* Improvements to local runtime persistence system
* Some classes and members have been renamed
* Details renamed to Detail
* ScriptableObject database format changed to one file instead of multiple

## [0.1.0-preview.5] - 2019-08-23

### Added

* Core Stats System

### Changed

* Local persistence implementation updated
* Internal renaming

## [0.1.0-preview.4] - 2019-08-09

_First external release_

### Added

* Inventory System
* Player Wallet

#### This is the initial release of *Unity Package \<com.unity.game.foundation\>*.

### Features

* Inventory System
* Player Wallet
* Stats System Core
