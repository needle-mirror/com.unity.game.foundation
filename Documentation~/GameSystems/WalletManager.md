# The Wallet Manager

## Overview

The __Wallet Manager__ is dedicated to the management of the [currencies] and their balance.  
Contrary to the [Inventory Manager], which manages item instances, the __Wallet Manager__ doesn't create or destroy any object when adding or removing amounts.
It just changes the balance of the related [currency].

## Currency vs item instance

The __Wallet Manager__ is designed for items that the player can collect plenty of, but don't need individual [stats].

> You have two swords in your inventory, each having a different `damage` property value.

Swords are item instances in the __Inventory Manager__.

> I have `1500` gold coins, and each coin doesn't have any individual property.

Gold coin is a [currency] of the __Wallet Manager__, with a balance of `1500`.

## Initialization

The __Wallet Manager__ is initialized with all the available [currency] types defined in the [Catalog].  

Only new players can benefit from the `Initial Allocation` of a [currency].
If a new [currency] is created between two sessions of an existing player, their balance will be `0` for this new currency.

## API

The __Wallet Manager__ comes with a basic set of expected methods to:

- Get the balance of a [currency].
- Set the balance of a [currency].
- Adjust the balance by adding or removing an amount.










[currencies]: ../CatalogItems/Currency.md
[currency]:   ../CatalogItems/Currency.md

[inventory manager]: ../GameSystems/InventoryManager.md

[stats]: ../GameSystems/StatManager.md

[catalog]: ../Catalog.md
