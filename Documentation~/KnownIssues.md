# Game Foundation: Known issues/limitations

## 0.2.0-preview.1 (2019-12-05)
* After entering play mode and creating persistent data files, any new default inventories and default inventory items will no longer be automatically created at runtime. To work around this, you can delete your local persistent runtime data. As a convenience, we’ve added a menu item Window > Game Foundation > Tools > Delete Local Persistence Data.
* In the editor for Stat Detail, in rare cases, deleting a stat entry will show an exception in the console. This exception can be ignored.
* The Inventory Item category editor allows you to create a non-alphanumeric category name, which will cause errors.
* Creating a new category in the inventory window may not save the category properly when closing and reopening the Unity project. If you make other changes to the catalog after creating a new category, then you should avoid this issue.
* The Game Foundation editor window UI is not yet updated to look better in Unity 2019.3.x.
* Debug Window: Removing an inventory in the middle of the inventories list causes new inventories to add themselves in the middle.
* Debug Window: Adding or Removing Items may change the foldouts state of other items that should be unaffected.
