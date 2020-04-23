# Game systems

## Data access layer

The data access layer is responsible for providing Game Foundation-related game state data to the Game Foundation systems.
This makes it easy for you to switch between no persistence, local persistence, and your own persistence, without changing how you use GameFoundation.

We provide ready-to-use implementations that match the most common cases: 
* Use a [MemoryDataLayer] if you want to play from a clean slate each session (especially useful for testing).
You can also get Game Foundation's raw data from it to serialize them however you want.
* Use a [PersistenceDataLayer] to save and load your progression on a local file.

You can also implement your own `IDataAccessLayer` if you have more specific requirements.

More info [in this page].

## Inventory

See the [Inventory Manager] page.

## Wallet

See the [Wallet Manager] page.

## Transaction

See the [Transaction Manager] page.

## Stats

See the [Stat Manager] page.

## Analytics

> This part is obsolete and will be heavily revamped in Game Foundation 0.5

This system lets you record game analytics without having to write code for data instrumentation, making it easy to analyze live game data to make data-informed decisions about your games. You can enable or disable both editor and runtime Analytics in the Game Foundation settings by adding or removing the Analytics Detail.










[inventory manager]: GameSystems/InventoryManager.md

[wallet manager]: GameSystems/WalletManager.md

[transaction manager]: GameSystems/TransactionManager.md

[stat manager]: GameSystems/StatManager.md

[memorydatalayer]: DataLayers.md#memory-data-layer

[persistencedatalayer]: DataLayers.md#persistence-data-layer

[in this page]: DataLayers.md