# Inventory Item Definitions

## Overview

An __Inventory Item Definition__ is a [catalog item] providing static data for your item instances.

It is the only item you can instantiate.

## Editor Overview

Open the __Inventory window__ by going to __Window → Game Foundation → Inventory__.
The Inventory window will let you configure inventory item definitions.

![An overview of the Inventory Window](../images/inventory-item-definition-editor.png)

The interface is similar to the other [catalog items editor].

(1) In the **Mutable Properties** section you can define a list of fields for the item instances to read and write at runtime.\
  A mutable property must define:
  - A value type. The current supported type are `long`, `double`, `bool` and `string`.
  - A unique key for you to access the property at runtime.
  - A default value.

  Mutable Properties belong only to the definition they are declared into.
  This means you can use the same property key in different definitions with a different type if you want to.










[catalog item]: ../Catalog.md#Catalog-Items

[catalog items editor]: ../Catalog.md#Editor-Overview

[tags]: Tag.md
