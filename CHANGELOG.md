# Changelog

## [0.6.0] - 2020-06-30

### Added
* New prefab and component created for creating popups advertising a special promotion.
* Static Properties have been added to all catalog items.\
  Use them to define fixed data that won't change at runtime.
* The new Game Parameters feature provide a solution to store your game configuration.
  Editor can be found at: __Unity→Window→Game Foundation→Game Parameters__.
* You now can define an initial allocation for any inventory item.
  Those items are instantiated when the player profile is created.
* Support for non-consumable IAP.
* Tools to let the dev process the "background" IAP queue (restoring purchases, delayed purchase successes, etc) on their own, instead of letting Game Foundation do it for them automatically.

### Changed

* `JsonDetail` has been flagged `Obsolete` and will be replaced by Static Properties as soon as they handle lists and dictionaries.
* Simplified initialization of TransactionManager (UnityPurchasingAdapter is now internal and no longer decoupled).

## [0.5.0] - 2020-06-03

### Added

* Properties can now store `long` and `double` values.
  Since `int` and `float` are covered by these types, they have been removed from the editor type dropdown but are still handled.
* Properties can now store `bool` and `string` values.
* Add a public API to manipulate the Default Catalog.

### Changed

* The code is now split into different assemblies.\
  The local `Catalog`, formerly in the `CatalogManager` namespace is now in the `DefaultCatalog` assembly and `DefaultCatalog` namespace.\
  The scripts of sample Prefabs also have their own assembly.
* `Category` has been renamed `Tag` and now exists in a separate `TagCatalog`.\
  All old `FindItemsByCategory` methods have been reimplemented as FindItemsByTag and behave as before.\
  Tags can be added or removed to/from all of GameFoundation in `Window\Game Foundation\Tag` window.\
  When tags are removed from Tag Catalog, they will be removed from all items throughout Game Foundation.
* Stats have been renamed to Properties.
* Properties have been directly integrated inside Inventory Items.\
  This means that `StatCatalog`, `StatDefinition`, `StatDetail`, `StatManager` and the Stat editor window no longer exist.\
  Properties are accessible directly through the items and definitions they are defined for.
  The `IStatDataLayer` has been merged into `IInventoryDataLayer` to match this change.
* `GameItem` has been merged into `InventoryItem` since they are the only instantiable objects in Game Foundation systems at the moment.
* We've removed IPurchasingAdapter, and UnityPurchasingAdapter is now internal. Use the TransactionManager class for anything related to IAP in Game Foundation.

## [0.4.0] - 2020-04-24

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
  Items are no longer quantities in inventories, but identifiable item instances with custom stat per item.
  Quantities, if needed, can be achieved using stats.

### Changed

* Inventories definition removed.
  All the items are instantiated in the player inventory.
  Item collections (list, map) will be introduced next release.
* Virtual Transactions is now a item.

## [0.3.0] - 2020-02-19

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

## [0.2.0] - 2019-12-11

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
