# The Inventory Manager

## Overview

The __Inventory Manager__ is a central piece of the Game Foundation architecture.
It creates and destroys item instances.

![Main inventory example](../images/image16.png)  
*Example: the main inventory contains characters, hats, bonuses, and themes.*

## Creating/Removing items

In order to create an item, the __Inventory Manager__ requires an [Inventory Item Definition].
This [definition] can be found in the [Catalog] and passed to the `CreateItem()` method, or specified with its `ID`.

When created, an item is assigned a unique identifier (`id`), and it is initialized with the default values of its [StatDetail].

## Getting items

The __Inventory Manager__ provides some methods to retrieve items, or find specific items by their `ID`, their [definition] or their [categories]

> Some of the methods allocate an array to return the collection of items found.
> You'll also find a non-allocating version of those methods, accepting a target collection.

## Removing items

To remove items, the __Inventory Manager__ exposes various methods:

- Removing one item by passing its reference.
- Removing one item by passing its `ID`.
- Removing items by their [definition].
- Removing all the items.

Removing items from the __Inventory Manager__ doesn't destroy the item object in memory.
It wouldn't be an expected behaviour for a managed language like C#.
Instead, the items are _discarded_: they are removed from the [data layer] and they cannot be part of any process, but their `ID`, `display name` and `definition` remain accessible.










[inventory item definition]: ../CatalogItems/InventoryItemDefinition.md
[definition]:                ../CatalogItems/InventoryItemDefinition.md

[catalog]: ../Catalog.md

[statdetail]: ../Details/StatDetail.md

[categories]: ../Category.md

[data layer]: ../DataLayers.md
