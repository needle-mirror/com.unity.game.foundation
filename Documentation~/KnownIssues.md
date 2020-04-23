# Game Foundation: Known issues/limitations

## 0.4.0-preview.4 (2020-04-24)

* Old catalogs must be deleted and recreated due to new Store System and the removal of hashes.
Simply delete your Assets/GameFoundation folder from Project window and reopen Window->Game Foundation->Inventory (or Store, Stat or Game Item) to reinitialize catalogs, then reenter your desired inventories, stats and game items.
* The IAP integration is limited to consumable products.
* When checking the Purchasing Enabled box in GameFoundationSettings, you may see an error in the console: `PlayerSettings Validation: Requested build target group (##) doesn't exist; #define symbols for scripting won't be added.` This error can be cleared and should not cause any problems.
* Inventory Item prefab needs to be toggled away from default selection before it starts working.
* When running in the Editor with an iOS build target, TransactionManager always returns the Google Store price instead of iOS (this is a known issue with the Unity Purchasing SDK).
* Editor doesn't block duplicate ids created in different windows/catalogs, but Game Foundation will throw error during initialization.
* Mixing prefabs and components from Codeless IAP with Game Foundation is untested and not recommended at this time.
* When running in the editor, all IAP transactions are sent to the data layer as iOS because there is not yet a platform-independent test store in the data layers.
* The Game Foundation editor window UI is not yet updated to look better in Unity 2019.3.x.
* Creating a new category in the inventory window may not save the category properly when closing and reopening the Unity project.
Making other changes to the catalog after creating a new category is one way to work around this issue.
* After entering play mode and creating persistent data files, any new default inventories and default inventory items will no longer be automatically created at runtime.
To work around this, you can delete your local persistent runtime data.
* Debug Window: Removing an inventory in the middle of the inventories list causes new inventories to add themselves in the middle.
* Debug Window: Adding or Removing Items may change the foldouts state of other items that should be unaffected.
