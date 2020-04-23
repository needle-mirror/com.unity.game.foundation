# Changelog

## [0.4.0-preview.4] - 2020-04-24

### Added

* Store System.
  Editor can be found at: __Unity->Window->Game Foundation->Store__.
  Manager/API can be found at `GameFoundation.catalogs.storeCatalog` to `GetCollectionDefinition` and/or `GetItem` as needed.
* Assets Detail.
  Attach assets to your item definitions and load them using `Resources.Load()` automatically.
* Json Detail.
  Provides the ability to add arbitrary typed fields to your item definition
* IAP Transaction with IAP SDK
* Item instances.
  Items are no longer quantities in inventories, but identifiable item instances with custon stat per item.
  Quantities, if needed, can be achieved using stats.

### Changed

* Inventories definition removed.
  All the items are instantiated in the player inventory.
  Item collections (list, map) will be introduced next release.
* Virtual Transactions is now a item.

## [0.3.0-preview.5] - 2020-02-19

### Added

* Data Access Layer
* Transaction System
* Transaction System Sample Scene
* Purchasable Detail

### Changed

* GameFoundation's Initialization changed to take an IDataAccessLayer as an argument instead of a persistence object.
* GameFoundationSettings ScriptableObject is now split into GameFoundationDatabaseSettings, which holds the reference to the database for the editor, and GameFoundationSettings, which continues to hold the other settings, like analytics flags.
* CatalogManager now holds the reference to the catalogs at Runtime. Any runtime code that was previously written as GameFoundationSettings.database.xCatalog should now be written as CatalogManager.xCatalog.
* Persistence and Serializer interfaces changed to handle only GameFoundation's data.

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
