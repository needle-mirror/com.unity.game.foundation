# Game Foundation Tutorials

## Playing with items at runtime

We've created an [inventory item definition] in the [previous tutorial].
Let's see what we can do with it.

### Initialization of Game Foundation at runtime

- Create a new scene
- Create a new `game object` and name it `Game Foundation Init` (the name doesn't really matter).
- Add a new `MonoBehaviour` to your project and name it `GFInit`.
- Copy the following code into this script file:

```cs
using System;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DataAccessLayers;

public class GFInit : MonoBehaviour
{
    void Start()
    {
        // Creates a new data layer for Game Foundation,
        // with the default parameters.
        var dataLayer = new MemoryDataLayer();

        // Initializes Game Foundation with the data layer.
        GameFoundation.Initialize(dataLayer, OnInitSucceeded, OnInitFailed);
    }

    // Called when Game Foundation is successfully initialized.
    void OnInitSucceeded()
    {
        Debug.Log("Game Foundation is successfully initialized");
    }

    // Called if Game Foundation initialization fails 
    void OnInitFailed(Exception error)
    {
        Debug.LogException(error);
    }
}
```

Come back to your editor, and start your game by pushing the `Play` button.
In the console, you should see two log entries:

```
! `Successfully initialized Game Foundation version x.y.z`
! `Game Foundation is successfully initialized`
```

The second is the one you've written, while the first is logged by Game Foundation automatically.

### The Data Layer

While Game Foundation provides general APIs to manipulate your items, it needs a source to load the data, and a destination to push the states of its items. 

Given the different needs in game development in terms of loading / saving data, for example storing it locally on the device vs. remotely on a cloud storage, we want to provide a good abstraction so that our implementation can be built without too much assumptions on the underlying data storage and protocal. 

The [data layer] fulfills this role.
It gives Game Foundation a [catalog] of the static data, and Game Foundation notifies it of all the instance data it creates, modifies, or destroys.

The default constructor of the [MemoryDataLayer] loads the database of the static data you've manipulated in the [previous tutorial].
You can find this database in the `Assets/GameFoundation` folder, with the name `GameFoundationDatabase.asset`.

### Getting the inventory item definition at runtime

Go back to the `OnInitSucceeded` method, and add the following code, right under the `Debug.Log` statement:

```cs
void OnInitSucceeded()
{
    Debug.Log("Game Foundation is successfully initialized");

    // Use the key you've used in the previous tutorial.
    const string definitionKey = "myFirstItem";

    // The inventory item definitions are available in the
    // inventoryCatalog of the database.
    var catalog = GameFoundation.catalogs.inventoryCatalog;

    // Finding a definition takes a non-null string parameter,
    // but it can fail to find the definition.
    var definition = catalog.FindItem(definitionKey);

    if (definition is null)
    {
        Debug.Log($"Definition {definitionKey} not found");
        return;
    }

    // You should be able to get information from your definition now.
    Debug.Log($"Definition {definition.key} '{definition.displayName}' found.");
}
```

Compile and start your scene.
You should see the following log in your console:

```
! Definition myFirstItem 'My First Item' found.
```

### The catalogs

The static data of Game Foundation can be found in the `database` property of Game Foundation.
The data is organized among different catalogs:

- `inventoryCatalog` for the [inventory item definition]
- `currencyCatalog` for the [currencies]
- `storeCatalog` for the [stores]
- `transactionCatalog` for the [transactions]
- `gameParameterCatalog` for the [game parameters]
- `tagCatalog` for the [tags]

### Creating an item instance

Now that we have an [inventory item definition], we can create an item instance.
Go back to your `OnInitSucceeded` method, then append the following lines:

```cs
var item = InventoryManager.CreateItem(definition);

Debug.Log($"Item {item.id} of definition '{item.definition.key}' created");
```

Compile and start your scene.
You should see a new log entry:

```
Item xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx of definition 'myFirstItem' created
```

When created, an item instance is assigned a unique id, as a string version of a GUID.
This ID ensure that the item will remain unique.

### Removing the item instance

Now that we have an item instance, why not trying to remove it.
Go back to your `OnInitSucceeded` method, then append the following lines:

```cs
var removed = InventoryManager.RemoveItem(item);

if (!removed)
{
    Debug.LogError($"Unable to remove item {item.id}");
    return;
}

Debug.Log($"Item {item.id} successfully removed. Its discarded value is {item.discarded}");
```

Compile and start your scene.
You should see a new log entry:

```
Item 52adebc3-cee6-4706-b0f9-847a8c10a512 successfully removed. Its discarded value is True
```

Removing an item from Game Foundation doesn't destroy the object.
It is a managed object, so it cannot be really removed as long as it is not garbage collected.
It just tells the [data layer] that the item has to be removed from the persistence, and it sets the `item.discarded property` to `true`.  
A lot of properties are still accessible, but you cannot modify your object anymore, nor use it in the other APIs of Game Foundation.
This is pretty convenient, as you'd need to keep an access to the properties of a removed item in case you need to propagate its removal in your gameplay code.

### Conclusion

Item instance can represent a lot of different aspects of your game.
As soon as you want to manipulate an object with static data and properties, being able to create it, but also destroying on demand, and if you want to persist it in the player profile, then item instance should be the feature you'd need.

It can be the player inventory: the character equipment, consumable items like potions or buffs.
It can be the character itself.  
The way you'll use [Inventory Item Definition] and their instance is up to you.

But what if what you want now, is to play with [currencies]?
Let's switch to the [next tutorial] then.

### Final source code of this tutorial

```cs
using System;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DataAccessLayers;

public class GFInit : MonoBehaviour
{
    void Start()
    {
        // Creates a new data layer for Game Foundation,
        // with the default parameters.
        var dataLayer = new MemoryDataLayer();

        // Initializes Game Foundation with the data layer.
        GameFoundation.Initialize(dataLayer, OnInitSucceeded, OnInitFailed);
    }

    // Called when Game Foundation is successfully initialized.
    void OnInitSucceeded()
    {
        Debug.Log("Game Foundation is successfully initialized");

        // Use the key you've used in the previous tutorial.
        const string definitionKey = "myFirstItem";

        // The inventory item definitions are available in the
        // inventoryCatalog of the database.
        var catalog = GameFoundation.catalogs.inventoryCatalog;

        // Finding a definition takes a non-null string parameter,
        // but it can fail to find the definition.
        var definition = catalog.FindItem(definitionKey);

        if (definition is null)
        {
            Debug.Log($"Definition {definitionKey} not found");
            return;
        }

        // You should be able to get information from your definition now.
        Debug.Log($"Definition {definition.key} '{definition.displayName}' found.");

        var item = InventoryManager.CreateItem(definition);

        Debug.Log($"Item {item.id} of definition '{item.definition.key}' created");

        var removed = InventoryManager.RemoveItem(item);

        if (!removed)
        {
            Debug.LogError($"Unable to remove item {item.id}");
            return;
        }

        Debug.Log($"Item {item.id} successfully removed. Its discarded value is {item.discarded}");
    }

    // Called if Game Foundation initialization fails 
    void OnInitFailed(Exception error)
    {
        Debug.LogException(error);
    }
}
```









[inventory item definition]: ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"
[definition]:                ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"
[definitions]:               ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"
[item definition]:           ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"
[item definitions]:          ../CatalogItems/InventoryItemDefinition.md "Go to Inventory Item Definition"

[tag]:   ../CatalogItems/Tag.md "Go to Tag"
[tags]:  ../CatalogItems/Tag.md "Go to Tag"

[previous tutorial]: 01-CreatingAnItemDefinition.md "Creating an Inventory Item Definition"

[catalog]: ../Catalog.md "Go to Catalog"

[data layer]: ../DataLayers.md "Go to Data Layers"

[memorydatalayer]: ../DataLayers.md#memory-data-layer "Go to Memory Data Layer"

[currencies]: ../CatalogItems/Currency.md "To to Currency"

[stores]: ../CatalogItems/Store.md "Go to Store"

[transactions]: ../CatalogItems/VirtualTransaction.md "Go to Virtual Transaction"

[game parameters]: ../CatalogItems/GameParameters.md "Go to Game Parameters"

[next tutorial]: 03-CreatingCurrency.md
