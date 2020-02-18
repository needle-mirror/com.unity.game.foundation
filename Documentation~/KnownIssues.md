# Game Foundation: Known issues/limitations

## 0.3.0-preview.5 (2020-02-19)
* After entering play mode and creating persistent data files, any new default inventories and default inventory items will no longer be automatically created at runtime. To work around this, you can delete your local persistent runtime data.
* In the editor for Stat Detail, in rare cases, deleting a stat entry will show an exception in the console. This exception can be ignored.
* Creating a new category in the inventory window may not save the category properly when closing and reopening the Unity project. Making other changes to the catalog after creating a new category is one way to work around this issue.
* The Game Foundation editor window UI is not yet updated to look better in Unity 2019.3.x.
* Debug Window: Removing an inventory in the middle of the inventories list causes new inventories to add themselves in the middle.
* Debug Window: Adding or Removing Items may change the foldouts state of other items that should be unaffected.
