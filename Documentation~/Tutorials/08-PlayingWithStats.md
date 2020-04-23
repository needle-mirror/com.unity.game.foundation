# Game Foundation Tutorials

## Playing with stats at runtime

In [the previous tutorial], we learned how to configure mutable properties in the [Stat Detail], using [Stat Definitions].

In this one, we'll take a look at the code to get the [stat definition], and modify its value for an item instance.

### Prerequisites

We'll start from the end of [the inventory tutorial].
If you didn't follow this tutorial, please copy/paste [the final source code] in to a class file named `GFInit.cs` and associate it to a new `GameObject` of the new scene.

### Getting the stat definition

Open your `GFInit.cs` file and append the following code inside `OnInitSucceeded`:

```cs
var item = InventoryManager.CreateItem(definition);

Debug.Log($"Item {item.id} of definition '{item.definition.id}' created");

// <---- Insert the following code here

// Gets the stat catalog
var statCatalog = GameFoundation.catalogs.statCatalog;

// This is the id of the stat definition we've created
const string statDefinitionId = "myIntStat";

// Gets the stat definition from the catalog
var statDefinition = statCatalog.FindStatDefinition(statDefinitionId);

if (statDefinition is null)
{
    Debug.LogError($"Cannot find the {nameof(StatDefinition)} {statDefinitionId}");
    return;
}

Debug.Log($"{nameof(StatDefinition)} '{statDefinition.displayName}' found");
```

Compile and start the scene.
You should see the following log entry:

```
! StatDefinition 'My Int Stat' found
```

### Getting the default value of the stat

You can retrieve the default value of a [stat definition] within an [item definition] as following.
Back to your code, append the follow code:

```cs
// Gets the detail from the item definition
var statDetail = item.definition.GetDetail<StatDetail>();

// Gets the default value from the detail
var defaultValue = statDetail.GetDefaultValue(statDefinition);

Debug.Log($"The default value of '{statDefinition.displayName}' is {defaultValue}");
```

Compile and start the scene.
You should see the following log entry:

```
! The default value of 'My Int Stat' is 42
```

### Getting the current value of the stat for an item instance

While the default value of a [stat] cannot be changed, the item instance can overwrite it.

Append the following code:

```cs
// Tries to get the value of the stat for the created item
var found = StatManager.TryGetValue(item, statDefinition, out var statValue);
if (!found)
{
    Debug.LogError($"item '{item.definition.displayName}' doesn't seem to support the stat '{statDefinition.displayName}'");
}
else
{
    Debug.Log($"item '{item.definition.displayName}' value for '{statDefinition.displayName}' is {statValue}");
}
```

This new code should log the following message:

```
! Item 'My First Item' value for 'My Int Stat' is 42
```

As expected, the current value is the same as the default one.
That's because we didn't play with it yet.
That's what we are about to do, with the following code:

```cs
//StatManager.SetValue(item, statDefinition, 38);
item.SetStat(statDefinition, 38);

statValue = item.GetStat(statDefinition);

Debug.Log($"Item '{item.definition.displayName}' value for '{statDefinition.displayName}' is {statValue}");
```

The item also exposes `GetStat`/`SetStat` methods.

> The [StatManager] will soon be merged to the [Inventory Manager] and only the API using the item as a base will remain.

Start your scene.
Your console should have the following log:

```
! item 'My First Item' value for 'My Int Stat' is 38
```

As you can see, setting a stat is pretty straightforward.

### Conclusion

Playing with [static properties] and [mutable properties] of your [catalog items] will help you build a better economy for your game.

### Going forward

Speaking about economy, now is the time to turn your game into a successful business.

In [the next tutorial], we'll talk about transactions.
But we'll start smooth with the virtual transactions.

### Final code source of this tutorial

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
        GameFoundation.Initialize
            (dataLayer, OnInitSucceeded, OnInitFailed);
    }

    // Called when Game Foundation is successfully
    // initialized.
    void OnInitSucceeded()
    {
        Debug.Log("Game Foundation is successfully initialized");

        // Use the ID you've used in the previous tutorial.
        const string definitionId = "myFirstItem";

        // The inventory item definitions are available in the
        // inventoryCatalog of the database.
        var catalog = GameFoundation.catalogs.inventoryCatalog;

        // Finding a definition takes a non-null string parameter,
        // but it can fail to find the definition.
        var definition = catalog.FindItem(definitionId);

        if (definition is null)
        {
            Debug.Log($"Definition {definitionId} not found");
            return;
        }

        // You should be able to get information from your
        // definition now.
        Debug.Log($"Definition {definition.id} '{definition.displayName}' found.");

        var item = InventoryManager.CreateItem(definition);

        Debug.Log($"Item {item.id} of definition '{item.definition.id}' created");

        // ----------------> Beginning of this tutorial

        // Gets the stat catalog
        var statCatalog = GameFoundation.catalogs.statCatalog;

        // This is the id of the stat definition we've created
        const string statDefinitionId = "myIntStat";

        // Gets the stat definition from the catalog
        var statDefinition = statCatalog.FindStatDefinition(statDefinitionId);

        if (statDefinition is null)
        {
            Debug.LogError($"Cannot find the {nameof(StatDefinition)} {statDefinitionId}");
            return;
        }

        Debug.Log($"{nameof(StatDefinition)} '{statDefinition.displayName}' found.");

        // Gets the detail from the item definition
        var statDetail = item.definition.GetDetail<StatDetail>();

        // Gets the default value from the detail
        var defaultValue = statDetail.GetDefaultValue(statDefinition);

        Debug.Log($"The default value of '{statDefinition.displayName}' is {defaultValue}");

        // Tries to get the value of the stat for the created item
        var found = StatManager.TryGetValue(item, statDefinition, out var statValue);
        if (!found)
        {
            Debug.LogError($"Item '{item.definition.displayName}' doesn't seem to support the stat '{statDefinition.displayName}'");
        }
        else
        {
            Debug.Log($"Item '{item.definition.displayName}' value for '{statDefinition.displayName}' is {statValue}");
        }

        //StatManager.SetValue(item, statDefinition, 38);
        item.SetStat(statDefinition, 38);

        statValue = item.GetStat(statDefinition);

        Debug.Log($"Item '{item.definition.displayName}' value for '{statDefinition.displayName}' is {statValue}");

        // <---------------- End of this tutorial

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










[the previous tutorial]: 07-AddMutablePropertiesWithStats.md

[stat detail]:        ../Details/StatDetail.md
[mutable properties]: ../Details/StatDetail.md

[stat definitions]: ../StatDefinition.md
[stat definition]:  ../StatDefinition.md
[stat]:             ../StatDefinition.md

[the inventory tutorial]: 02-PlayingWithRuntimeItem.md

[the final source code]: 02-PlayingWithRuntimeItem.md#Final&#32;code&#32;source&#32;of&#32;this&#32;tutorial

[item definition]: ../CatalogItems/InventoryitemDefinition.md

[statmanager]: ../GameSystems/StatManager.md

[inventory manager]: ../GameSystems/InventoryManager.md

[static properties]: ../Details/JsonDetail.md

[catalog items]: ../Catalog.md#Catalog&#32;Items

[the next tutorial]: 09-CreatingAVirtualTransaction.md
