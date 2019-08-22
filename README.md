# Game Foundation

## Introduction

During game development, there are many basic gameplay systems that are common to most games. Developers have to take the time to build and rebuild these systems when they really want to focus on what’s unique and fun.

Game Foundation provides pre-built common game systems that are flexible and fully extensible so that developers can focus on building unique gameplay.

Please see [Getting Started](https://docs.google.com/document/d/1YCozGaVvJyt95DnFpk2v9eyfhBhKcbilm2HnpSWsgEM/) for more detailed information. A brief summary is provided below.
<hr>


## Getting Started

### Installing the Package

In order to use Game Foundation in your game, you’ll first have to install the package.

#### *From Unity package registry*

1. Open the Package Manager (Window → Package Manager).
1. Make sure preview packages are enabled (Advanced → Show preview packages).
1. Find Game Foundation in the left column and select it.
1. Click the `Install` button (in the lower right or upper right, depending on your Unity version).

#### *From manually downloaded package*

If you are using a manually downloaded package it will not appear in the Package Manager UI and should be installed as follows:

1. Extract the archive to a new directory
1. Open the Package Manager (Window → Package Manager).
1. In Package Manager UI select the `+` button (either upper left or bottom right corner) and then `Add package from disk...`
1. Navigate to your extracted archive and select the `package.json` file

After installing the package, you’ll have some new menu items tied to editor windows.

### Updating from Alpha 1 to Alpha 2

To update, replace the entire package folder with the contents of the latest archive file downloaded from the most recent email.

The ScriptableObject assets in your project that were created by Game Foundation should automatically update to use the newer scripts and newer nomenclature. However, if you were using methods in your code that had ‘Entity’ or ‘Component’ in the name, you will need to update them to the newer versions of the methods which are now named using ‘GameItem’ and ‘Details’, respectively.

### Creating a Soft Currency in the Wallet

1. Open the Inventory window by going to Window → Game Foundation → Inventory.  This is a tool for viewing and managing the contents of your “Inventory Catalog”.

1. Click the “+” button to create a new inventory item.  This will prompt you for an id and a display name.  The display name can be changed later, but the ID will be permanent.

1. Enter a name and ID and click the “Create” button. Now that the item is created and added to the catalog, you will be able to see additional configuration options: Categories and Details

1. In order for an item to be a currency, it must have Currency Details.  Click the “Add Details” button, and then click “Currency Details”.

1. In Currency Details you can choose what type of currency this item is, but for this one we can leave it as the default of “soft currency”.

1. Now that we have a currency item, we are able to add that item to the Wallet at runtime. We can also make it show up in the player’s wallet automatically when the game starts by making it a Default Item in the Wallet inventory. Click on the Inventories tab, and select Wallet in the left column.

1. The Main and Wallet inventories are built-in and can’t be deleted, but you can create additional custom inventories in this panel. In each inventory, there is a list of Default Items, which will be added to that inventory automatically when that inventory is first instantiated at runtime.  In this panel, find the Coin in Other Available Items, and click the Add To Default Items button next to it.

1. Once the Coin is added to the default items list, you can adjust the quantity that are added when the wallet is first created. In this case, the player will start with 100 coins in their wallet.

### Spending Coins In-Game

After creating the coin and adding to the wallet in the first walkthrough, add this script to a GameObject in a Scene, and hit Play.  In the console, you’ll see that the player’s wallet starts out with 100 coins in it, then we immediately take 25 of them, leaving them with 75 coins.

```
using UnityEngine;
using UnityEngine.GameFoundation;

public class WalletTest : MonoBehaviour
{
    void Awake()
    {
        InventoryManager.Initialize();
    }

    void Start()
    {
        InventoryItem coinWalletItem = Wallet.GetItem("coin");
        int coinQty = coinWalletItem.quantity;
        Debug.LogFormat("coins in wallet at start: {0}", coinQty);
        coinQty -= 25;
        Wallet.SetQuantity("coin", coinQty);
        Debug.LogFormat("coins in wallet: {0}",
            coinWalletItem.quantity);
    }
}
```

### Creating and Applying Stats

1. Open the Stat window (menu: Window → Game Foundation → Stat). This is a tool for viewing and managing the stats you want to adjust and persist at runtime.

1. For this example, we’ll have a sword that has a Damage stat. Create the Damage stat by clicking the + button to create a new inventory item and then enter a Display Name of “Damage”, Id of “damage”, and choose a Value Type of “Int”. Click the Create button.

1. Following the steps from the first walkthrough above, create a Sword GameItem.

1. Click Add Details to add Stat Details to the Sword.

1. In the new Stat Details section, you’ll see a popup menu with your only stat, Damage, already selected. Next to that, click Add to add the Damage stat to your Sword. Then select the Default Value field and enter 10 for the sword’s damage.

1. Follow the above steps, and steps learned from previous walkthroughs, to do the following:
- Create a “Damage Increase” stat with a Value Type of Int.
- Create a “Scroll of Damage Increase” GameItem.
- Give the scroll a “Damage Increase” stat with a default value of 1.
- Create a “Sword” Inventory Item with the Reference Definition set to Sword, and add it as a default item in the Main inventory.
- Create a “Scroll of Damage Increase” Inventory Item with the Reference Definition set to Scroll of Damage Increase and also add it to the Main inventory’s default items.

### Working with Stats at Runtime

Now that we have a sword and a scroll, let’s use those while playing the game.

```
// let's use a consumable scroll to buff our weapon

void SharpenSword()
{
    // get the sword from inventory
    InventoryItem swordItem = Inventory.main.GetItem("sword");

    // get the scroll from inventory
    InventoryItem scrollItem =
        Inventory.main.GetItem("scrollDamageIncrease");

    // get the sword’s current damage
    int swordDamage = StatManager.GetIntValue(swordItem, "damage");

    // find out how much the scroll will increase the damage
    int damageIncrease = StatManager.GetIntValue(
        scrollItem, "damageIncrease");

    // increase this sword’s damage permanently
    StatManager.SetIntValue(swordItem, "damage",
        swordDamage + damageIncrease);

    // consume the scroll by removing it from the inventory
    Inventory.main.RemoveItem(scrollItem);
}
```

## Game Systems

### Inventory

An inventory is a way of keeping track of a collection of items, as well as how many of those items exist in that collection. Some basic examples of inventories would be a backpack or a chest. But you could also think of other collections of items as inventories as well. For example, when loot is sitting on the ground, maybe it’s contained in the ‘ground’ inventory. If a sword has boosting gems installed in it, then you could say that sword has an inventory and the gems are contained in it.

Inventories will have a runtime instance and a definition.

Game Foundation automatically creates a ‘main’ inventory by default, so you don’t need to define and create more inventories if you don’t need more than one.

### Wallet

The Wallet is a special variation of inventory which only contains currencies. A common example for using a wallet is in casual mobile games where the player can accumulate one or more types of currency, such as coins, gems, etc. A default wallet is automatically created, and you can access and manage that default wallet conveniently with the Wallet class.

### Stat

With stats, you can track and manage numeric values used in the game, such as the health of the player’s character and enemies, weapon damage, tool durability, a consumable item’s healing amount, buff amount, etc. Extending the stat system will allow you to apply formulae to stats based on modifiers and player progression.

The numeric types currently supported are Float (System.Single) and Int (System.Int32). Once you choose which numeric type a stat is, it cannot be changed later.

