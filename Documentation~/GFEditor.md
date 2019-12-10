# Game Foundation Editor overview

## Game Item window

Open the **Game Item** window by going to **Window → Game Foundation → Game Item**. 
The Game Item window lets you configure base GameItemDefinitions. These are optional data objects that are good for two types of usages:

1. GameItemDefinitions serve as default values for similar items in multiple systems. For example, a ‘sword’ inventory item definition can refer to a ‘sword’ GameItemDefinition. In this hypothetical case, the sword GameItemDefinition has an IconDetailsDefinition, but the InventoryItemDefinition does not. At runtime, you can ask the InventoryItem for its icon, and it will default to getting the icon from the GameItemDefinition.
2. GameItemDefinitions serve as a place for data that doesn’t exist in any systems, but you would still like to configure and use at runtime.

## Inventory window
Open the **Inventory** window by going to **Window → Game Foundation → Inventory**.
The Inventory window will let you configure inventory definitions and inventory item definitions. Wallet and currencies are also configured using the Inventory window. 

### Inventories tab
The Inventories tab contains a list of all inventories that can be used in the game.

#### Default items
An inventory definition can be assigned a number of default items. When this inventory definition is instantiated at runtime, these items will be added to the inventory by default. This is useful when the player should start the game with a small amount of in-game currency, booster items, etc.

### Inventory items tab
The Inventory Items tab contains a list of Inventory Items. An Inventory Item is a data object that describes how an item should be represented when it’s in an inventory. An inventory item can also have details added.

#### Inventory Item Details
Inventory items can optionally have details assigned to them. Details can contain data fields that let you configure something specific about that item, such as an icon, some metadata, crafting rules, etc. The details could also have no data fields, and could just be used to identify the item as being useful in a specific system.

#### Currency Detail
The currency details are special details related to the wallet. Inventory items without a currency detail are not allowed in the wallet (though currencies are allowed in inventories other than the wallet).

## Categories tab
The Categories tab contains a list of categories specific to the inventory system. These categories can be assigned to inventory items in the Inventory Items tab. These can help you in locating inventory items in the game.

## Stat window
Open the Stat window by going to Window → Game Foundation → Stat. The Stat window will let you create StatDefinitions, which are used in StatDetailsDefinitions in the Game Item and Inventory windows.
