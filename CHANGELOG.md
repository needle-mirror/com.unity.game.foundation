# Changelog

## [0.1.0-preview.5] - 2019-08-23
### Added
- Core Stats System

### Changed
- Local persistence implementation updated
- Internal renaming

## [0.1.0-preview.4] - 2019-08-09
_First external release_
### Added
- Inventory System
- Player Wallet

### This is the initial release of *Unity Package \<com.unity.game.foundation\>*.

## Current Features
* Inventory System
* Player Wallet
* Stats System Core

### Known Issues

* Store system is coming soon.
* Reference Definition isn’t removed from an InventoryItemDefinition when a GameItemDefinition is deleted from the catalog.
* Category is not removed from GameItemDefinition when the Category is deleted from the catalog.
* When removing a stat from the stat details section of an item, it throws an exception if you have the same StatDetailsDefinition ScriptableObject selected in the Project window and showing in the Inspector window.
* If you delete the Assets/GameFoundation folder and then recompile the project, you may receive a number of errors in the log related to the stat system. If you restart the Unity Editor, the errors should not come back.
