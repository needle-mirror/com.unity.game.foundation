# Game Foundation Tutorials

## Playing with virtual transaction at runtime

Now that a [virtual transaction] has been [added your our catalog], we can initiate a transaction at runtime with a simple call which will do all the work for us.

### Initialize the source code

We'll start with the code resulting from [the first step of the Inventory tutorial].

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

        //
        // You'll put your code here.
        //
    }

    // Called if Game Foundation initialization fails 
    void OnInitFailed(Exception error)
    {
        Debug.LogException(error);
    }
}
```

### Getting the virtual transaction

You can get the virtual transaction from the [Transaction Catalog].  
This catalog contains all kinds of transaction: virtual, but also [IAP].

Please append the following code to the `OnInitSucceeded` method.

```cs
// Gets the transaction catalog
var catalog = GameFoundation.catalogs.transactionCatalog;

// Finds the transaction by its id.
var transaction = catalog.FindTransaction<VirtualTransaction>(transactionId);
if (transaction is null)
{
    Debug.LogError($"Transaction {transactionId} not found");
    return;
}
```

There are many ways to find a transaction.
The one we use here is the most precise, as `FindTransaction()` takes a generic type `<VirtualTransaction>`.

A `VirtualTransaction` is inherited from `BaseTransaction`.
If you only need to get a transaction, but you don't need to know if it is a virtual, or an IAP, then you can use:

```cs
var transaction = catalog.FindItem(transactionId);
```

> This statement works with this tutorial as it doesn't require the transaction to be specifically a virtual one.

### Initiating the transaction

Transactions are used by the [Transaction Manager], passed to the `BeginTransaction` method.

The transaction process is asynchronous.
We'll create a coroutine to wait for its completion.

Create the `InitiateTransaction()` method according to the following snippet:

```cs
IEnumerator InitiateTransaction(BaseTransaction transaction)
{
    // Gets the handle of the transaction.
    var deferredResult = TransactionManager.BeginTransaction(transaction);

    try
    {
        // Waits for the process to finish
        while (!deferredResult.isDone)
        {
            yield return null;
        }

        // The process failed
        if (!deferredResult.isFulfilled)
        {
            Debug.LogException(deferredResult.error);
        }

        // The process succeeded
        else
        {
            var result = deferredResult.result;

            // TODO: display the result
        }
    }
    finally
    {
        // A process handle can be released.
        deferredResult.Release();
    }
}
```

You can replace the `while` statement in this snippet by the following one:

```cs
yield return deferredResult.Wait();
```

It's easier to read, but it returns a coroutine the engine will focus while the `deferred` is not finished.
But the `while` version is better for perforance because it doesn't allocate an additional routine.

This method begins a transaction with the transaction object, which describes what to consume, and what to expect from this exchange.

> If you need more information about the [Promise] system, check the corresponding page of the documentation.

### The transaction result

The `deferredResult` from the `BeginTransaction` is an instance of `Deferred<TransactionResult>`.
Let's display its content in the console with the following code:

```cs
void DisplayResult(TransactionResult result)
{
    var builder = new StringBuilder();

    var paidCurrencies = result.costs.currencies;
    foreach (var exchange in paidCurrencies)
    {
        builder.AppendLine($"Paid '{exchange.currency.displayName}' X {-exchange.amount}");
    }

    var consumedItems = result.costs.itemIds;
    foreach (var consumedItemId in consumedItems)
    {
        builder.AppendLine($"Consumed item {consumedItemId}");
    }

    var rewardedCurrencies = result.rewards.currencies;
    foreach (var exchange in rewardedCurrencies)
    {
        builder.AppendLine($"Earned '{exchange.currency.displayName}' X {exchange.amount}");
    }

    var rewardedItems = result.rewards.items;
    foreach (var rewardedItem in rewardedItems)
    {
        builder.AppendLine($"Obtained '{rewardedItem.definition.displayName}' ({rewardedItem.id})");
    }

    Debug.Log(builder.ToString());
}
```

Then call this method by replacing the `TODO` comment of the snippet above, with the following one:

```cs
// The process succeeded
else
{
    var result = deferredResult.result;

    // TODO was here
    DisplayResult(result);
}
```

Now that everything is in place, start a coroutine at the end of the `OnInitSucceeded` method with `InitiateTransaction` and the `transaction` parameter:

```cs
StartCoroutine(InitiateTransaction(transaction));
```


Start your scene.
Your Console tab should show this last entry:

```
Paid 'My First Currency' X 10
Obtained 'My First Item' (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
Obtained 'My First Item' (yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy)
Obtained 'My First Item' (zzzzzzzz-zzzz-zzzz-zzzz-zzzzzzzzzzzz)
```

### Conclusion

The benefit of using [virtual transactions] instead of doing this process in your game code is that Game Foundation can make it as a single operation to synchronize with the [data layer], making the transaction consistent and preventing discrepancy with your game server.

Converting virtual goods and currencies is essential for your virtual game economy.  
Now what if we were trying to turn virtual goods into real money?

[In-App Purchasing Transaction] is the other kind of transaction Game Foundation supports and that's the subject of [the next tutorial].

### Final source code

```cs
using System;
using System.Collections;
using System.Text;
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
        const string transactionId = "myVirtualTransaction";

        // Gets the transaction catalog
        var catalog = GameFoundation.catalogs.transactionCatalog;

        // Finds the transaction by its id.
        var transaction = catalog.FindTransaction<VirtualTransaction>(transactionId);
        if (transaction is null)
        {
            Debug.LogError($"Transaction {transactionId} not found");
            return;
        }

        StartCoroutine(InitiateTransaction(transaction));
    }

    IEnumerator InitiateTransaction(BaseTransaction transaction)
    {
        // Gets the handle of the transaction.
        var deferredResult = TransactionManager.BeginTransaction(transaction);

        try
        {
            // Waits for the process to finish
            while (!deferredResult.isDone)
            {
                yield return null;
            }

            // The process failed
            if (!deferredResult.isFulfilled)
            {
                Debug.LogException(deferredResult.error);
            }

            // The process succeeded
            else
            {
                var result = deferredResult.result;

                DisplayResult(result);
            }
        }
        finally
        {
            // A process handle can be released.
            deferredResult.Release();
        }
    }

    void DisplayResult(TransactionResult result)
    {
        var builder = new StringBuilder();

        var paidCurrencies = result.costs.currencies;
        foreach (var exchange in paidCurrencies)
        {
            builder.AppendLine($"Paid '{exchange.currency.displayName}' X {exchange.amount}");
        }

        var consumedItems = result.costs.itemIds;
        foreach (var consumedItemId in consumedItems)
        {
            builder.AppendLine($"Consumed item {consumedItemId}");
        }

        var rewardedCurrencies = result.rewards.currencies;
        foreach (var exchange in rewardedCurrencies)
        {
            builder.AppendLine($"Earned '{exchange.currency.displayName}' X {exchange.amount}");
        }

        var rewardedItems = result.rewards.items;
        foreach (var rewardedItem in rewardedItems)
        {
            builder.AppendLine($"Obtained '{rewardedItem.definition.displayName}' ({rewardedItem.id})");
        }

        Debug.Log(builder.ToString());
    }

    // Called if Game Foundation initialization fails 
    void OnInitFailed(Exception error)
    {
        Debug.LogException(error);
    }
}
```










[virtual transaction]:  ../CatalogItems/VirtualTransaction.md
[virtual transactions]: ../CatalogItems/VirtualTransaction.md

[added your our catalog]: 09-CreatingAVirtualTransaction.md

[the first step of the Inventory tutorial]: 02-PlayingWithRuntimeItem.md#Initialization&#32;of&#32;Game&#32;Foundation&#32;at&#32;runtime

[transaction catalog]: ../Catalog.md

[iap]: ../CatalogItems/IAPTransaction.md

[transaction manager]: ../GameSystens/TransactionManager.md

[promise]: ../Promise.md

[data layer]: ../DataLayers.md

[in-app purchasing transaction]: ../CatalogItems/IAPTransaction.md

[the next tutorial]: 11-PlayingWithIAPTransaction.md
