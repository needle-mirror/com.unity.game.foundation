# Virtual Transactions

## Overview

A __Virtual Transaction__ is a [catalog item] dedicated to purchases using virtual currencies and counterparts.

This type of transaction has 2 specifics fields:

- `Costs`: it describes the inputs from the transaction, the list of items and currencies the player needs to consume when processing this transaction.
- `Rewards`: it describes the outcome from the transaction, the list of items and currencies the player receives when processing this transaction.

The process of consuming the costs, and generating the rewards is done by the [Transaction Manager].

## Editor Overview

Open the __Transaction window__ by going to __Window → Game Foundation → Transaction__.
The Transaction window will let you configure both __Virtual Transactions__ and [IAP Transactions].

![The Virtual Transaction Editor Window](../images/virtualtransaction-editor.png)

The interface is similar to the other [catalog items].

(1) The Costs section show a list of [currencies] and [item definitions]. Those are the input of the transaction.
The amounts next to each entry are the number of those items the transaction will consume in order to produce the rewards.

(2) The Rewards section show a list of [currencies] and [item definitions]. Those are the output of the transaction.
The amounts next to each entry are the number of those items the player will get from the transaction.










[catalog item]:  ../Catalog.md#Catalog&#32;Items
[catalog items]: ../Catalog.md#Catalog&#32;Items

[transaction manager]: ../GameSystems/TransactionManager.md

[iap transactions]: IAPTransaction.md

[currencies]: Currency.md

[item definitions]: InventoryItemDefinition.md
