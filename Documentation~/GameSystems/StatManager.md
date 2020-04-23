# The Stat Manager

## Overview

The __Stat Manager__ is dealing with the mutable fields of the item instances.

An item instance can modify its stats only if its [Item Definition] contains a [StatDetail] with the list of [StatDefinitions].

## Getting the value of a stat

A stat can be retrieved from:

- the __StatManager__
- the item instance itself

```cs
// The following blocks are similar

var statValue = StatManager.GetValue("id-of-my-item", "id-of-my-stat");

var myItem = InventoryManager.FindItem("id-of-my-item");
var statValue = myItem.GetStat("id-of-my-stat");

var statCatalog = GameFoundation.catalogs.statCatalog;
var myStat = statCatalog.FindStatDefinition("id-of-my-stat");

var statValue = StatManager.GetValue(myItem, myStat);
var statValue = myItem.GetStat(myStat);
```

Each method of the __StatManager__ API proposes multiple variants, using the references to the item instance and the [stat definition] or their `id`.

## Updating a stat

The API is similar to the getter methods

```cs
StatManager.SetValue("id-of-my-item", "id-of-my-stat", 42);

var statCatalog = GameFoundation.catalogs.statCatalog;
var myItem = InventoryManager.FindItem("id-of-my-item");
var myStat = statCatalog.FindStatDefinition("id-of-my-stat");
myItem.SetStat(myStat, 42);
```

> In the next version of Game Foundation, this manager will be merged into the [Inventory Manager].










[item definition]: ../CatalogItems/InventoryItemDefinition.md

[statdetail]: ../Details/StatDetail.md

[statdefinitions]: ../StatDefinition.md
[stat definition]: ../StatDefinition.md

[inventory manager]: InventoryManager.md
