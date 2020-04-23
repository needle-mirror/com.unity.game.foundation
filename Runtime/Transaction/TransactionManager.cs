using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.GameFoundation.PurchasingAdapters;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This class contains methods to process virtual transactions and in-app purchases.
    /// </summary>
    public static class TransactionManager
    {
        /// <summary>
        /// Fired as soon as a valid transaction is initiated.
        /// </summary>
        public static event Action<BaseTransaction> transactionInitiated;

        /// <summary>
        /// Fired after every time the progress number is increased on a transaction's <see cref="Deferred{TResult}"/>.
        /// </summary>
        public static event Action<BaseTransaction, int, int> transactionProgressed;

        /// <summary>
        /// Fired after a transaction has succeeded at all levels.
        /// </summary>
        public static event Action<BaseTransaction, TransactionResult> transactionSucceeded;

        /// <summary>
        /// Fired after a transaction fails.
        /// </summary>
        public static event Action<BaseTransaction, Exception> transactionFailed;

        /// <summary>
        /// Fired after the purchasing adapter reports successfully being initialized.
        /// </summary>
        public static event Action purchasingAdapterInitializeSucceeded;

        /// <summary>
        /// Fired after the purchasing adapter reports failing to initialize.
        /// </summary>
        public static event Action<Exception> purchasingAdapterInitializeFailed;

        /// <summary>
        /// An instance of the optional in-app purchasing adapter.
        /// </summary>
        public static IPurchasingAdapter purchasingAdapter { get; private set; }

        /// <summary>
        /// Returns true after TransactionManager has been successfully initialized with a data layer.
        /// </summary>
        public static bool isInitialized { get; private set; }

        /// <summary>
        /// Returns true if the optional purchasing adapter has finished initializing.
        /// </summary>
        public static bool purchasingAdapterIsInitialized { get; private set; }

        static ITransactionDataLayer s_DataLayer;
        static VirtualTransaction s_CurrentVirtualTransaction;

        public static IAPTransaction pendingIapTransaction
        {
            get { return s_PendingIap.iap; }
        }
        static (IAPTransaction iap, bool isSuccessful, string failureMessage) s_PendingIap;

        internal static void Initialize(ITransactionDataLayer dataLayer)
        {
            if (isInitialized)
            {
                Debug.LogWarning("TransactionManager is already initialized and cannot be initialized again.");
                return;
            }

            s_DataLayer = dataLayer;
            isInitialized = true;
        }

        internal static void Uninitialize()
        {
            if (!isInitialized)
                return;

            if (s_PendingIap.iap != null)
            {
                Debug.LogWarning("An IAP is still pending while uninitializing GameFoundation." +
                    " This might cause unexpected behaviour.");
            }

            purchasingAdapter?.Uninitialize();
            purchasingAdapter = null;
            purchasingAdapterIsInitialized = false;

            s_DataLayer = null;
            s_CurrentVirtualTransaction = null;
            s_PendingIap = default;

            isInitialized = false;
        }

        /// <summary>
        /// Throws an exception if the TransactionManager has not been initialized.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if not initialized.</exception>
        private static void ThrowIfNotInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException(
                    "Error: GameFoundation.Initialize() must be called before the TransactionManager is used.");
            }
        }

        /// <summary>
        /// Tells TransactionManager to start using the specified in-app purchasing adapter to process all IAP transactions.
        /// </summary>
        /// <param name="newPurchasingAdapter">An instance of the chosen purchasing adapter.</param>
        /// <exception cref="System.InvalidOperationException">Thrown if not initialized.</exception>
        public static void InitializePurchasingAdapter(
            IPurchasingAdapter newPurchasingAdapter,
            Action onInitializeSucceededCallback = null,
            Action<Exception> onInitializeFailedCallback = null)
        {
            ThrowIfNotInitialized();

            if (purchasingAdapter != null)
            {
                PurchasingAdapterInitializationFailed(new Exception("You can only initialize one purchasing adapter."));
                return;
            }

            if (newPurchasingAdapter == null)
            {
                PurchasingAdapterInitializationFailed(new Exception("Purchasing adapter cannot be null."));
                return;
            }

            purchasingAdapter = newPurchasingAdapter;
            purchasingAdapter.Initialize(onInitializeSucceededCallback, onInitializeFailedCallback);
        }

        /// <summary>
        /// The in-app purchasing adapter should call this method after it's successfully initialized.
        /// </summary>
        internal static void PurchasingAdapterInitializationSucceeded()
        {
            purchasingAdapterIsInitialized = true;
            purchasingAdapterInitializeSucceeded?.Invoke();
        }

        /// <summary>
        /// The in-app purchasing adapter should call this if it fails to initialize.
        /// </summary>
        /// <param name="exception">The reason for the failure.</param>
        internal static void PurchasingAdapterInitializationFailed(Exception exception)
        {
            purchasingAdapterIsInitialized = false;
            purchasingAdapterInitializeFailed?.Invoke(exception);
        }

        /// <summary>
        ///     Process a transaction.
        /// </summary>
        /// <param name="transaction">
        ///     A <see cref="BaseTransaction"/> to process.
        /// </param>
        /// <param name="costItemIds">
        ///     If this is a virtual transaction with item costs, this is the list of items to consume.
        ///     If this argument is null or empty, the first inventory items that satisfy the cost will be consumed.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="Deferred" /> struct which can be used to track the state of the transaction.
        /// </returns>
        public static Deferred<TransactionResult> BeginTransaction(
            BaseTransaction transaction,
            List<string> costItemIds = null)
        {
            ThrowIfNotInitialized();

            GameFoundation.GetPromiseHandles<TransactionResult>(out var deferred, out var completer);

            switch (transaction)
            {
                case VirtualTransaction virtualTransaction:
                {
                    //The given transaction is already being processed.
                    if (ReferenceEquals(transaction, s_CurrentVirtualTransaction))
                    {
                        completer.Reject(new ArgumentException(
                            $"This transaction id ({transaction.id}) is already being processed."));

                        return deferred;
                    }

                    s_CurrentVirtualTransaction = virtualTransaction;

                    if (costItemIds == null) costItemIds = new List<string>();
                    GameFoundation.updater.StartCoroutine(
                        ProcessVirtualTransactionCoroutine(virtualTransaction, completer, costItemIds));

                    break;
                }

                case IAPTransaction iapTransaction:
                {
                    //The given transaction is already being processed.
                    if (ReferenceEquals(transaction, s_PendingIap.iap))
                    {
                        completer.Reject(new ArgumentException(
                            $"This transaction id ({transaction.id}) is already being processed."));

                        return deferred;
                    }

                    //Another IAP is already being processed.
                    if (s_PendingIap.iap != null)
                    {
                        completer.Reject(new ArgumentException(
                            $"Can't start the IAP \"{iapTransaction.productId}\" because"
                            + $" \"{s_PendingIap.iap.productId}\" is already being processed."));

                        return deferred;
                    }

                    //Make sure to reset the whole tuple to start on a clean slate.
                    s_PendingIap = (iapTransaction, false, null);

                    GameFoundation.updater.StartCoroutine(
                        ProcessIAPTransactionCoroutine(iapTransaction, completer));

                    break;
                }

                default:
                    completer.Reject(new ArgumentException(
                        $"Unknown or unsupported transaction type: {transaction.GetType()}"));
                    break;
            }

            return deferred;
        }

        static IEnumerator ProcessVirtualTransactionCoroutine(
            VirtualTransaction transaction,
            Completer<TransactionResult> completer,
            ICollection<string> costItemIds)
        {
            completer.SetProgression(0, 3);
            transactionInitiated?.Invoke(transaction);
            transactionProgressed?.Invoke(transaction, 0, 3);

            // pre-validate the transaction before sending it to the data layer

            var exceptionList = new List<Exception>();

            transaction.VerifyCost(exceptionList);

            if (exceptionList.Count > 0)
            {
                var exceptions = new AggregateException(exceptionList);
                completer.Reject(exceptions);
                transactionFailed?.Invoke(transaction, exceptions);
                s_CurrentVirtualTransaction = null;
                yield break;
            }

            completer.SetProgression(1, 3);
            transactionProgressed?.Invoke(transaction, 1, 3);

            // the data layer will re-validate the transaction and fulfill it if it's valid

            GameFoundation.GetPromiseHandles<VirtualTransactionExchangeData>(out var dalDeferred, out var dalCompleter);

            // if there are item costs, but no instance ids are supplied,
            // then we infer that we should go find some instances automatically
            if (transaction.costs.ItemExchangeCount > 0 && costItemIds.Count <= 0)
            {
                transaction.AutoFillCostItemIds(costItemIds);
            }

            s_DataLayer.MakeVirtualTransaction(transaction.id, costItemIds, dalCompleter);

            while (!dalDeferred.isDone)
            {
                yield return null;
            }

            completer.SetProgression(2, 3);
            transactionProgressed?.Invoke(transaction, 2, 3);

            // handle the response from the DAL
            // even if the pre-validation succeeded,
            // the data layer could still fail or reject it

            if (dalDeferred.isFulfilled)
            {
                // fulfill locally in GF

                // WALLET COST

                var currencyCostCount = dalDeferred.result.cost.currencies.Length;
                var currencyCosts = new CurrencyExchange[currencyCostCount];

                for (var i = 0; i < currencyCostCount; i++)
                {
                    // the amount in the currency exchange is negative,
                    // but RemoveBalance expects only positive numbers
                    // TODO: the cost exchange amounts should be positive instead of negative IMO -Rich
                    long amount = (long)Mathf.Abs(dalDeferred.result.cost.currencies[i].amount);
                    WalletManager.RemoveBalanceInternal(dalDeferred.result.cost.currencies[i].currencyId, amount);

                    currencyCosts[i] = new CurrencyExchange()
                    {
                        amount = dalDeferred.result.cost.currencies[i].amount,
                        currency = GameFoundation.catalogs.currencyCatalog.FindItem(dalDeferred.result.cost.currencies[i].currencyId)
                    };
                }

                // INVENTORY COST

                var itemCostCount = dalDeferred.result.cost.items.Length;
                var itemCosts = new string[itemCostCount];

                for (var i = 0; i < itemCostCount; i++)
                {
                    InventoryManager.RemoveItemInternal(dalDeferred.result.cost.items[i].id);

                    itemCosts[i] = dalDeferred.result.cost.items[i].definitionId;
                }

                // WALLET REWARD

                var currenciesLength = dalDeferred.result.rewards.currencies.Length;
                var currencyRewards = new CurrencyExchange[currenciesLength];
                for (var i = 0; i < currenciesLength; i++)
                {
                    WalletManager.AddBalanceInternal(
                        dalDeferred.result.rewards.currencies[i].currencyId,
                        dalDeferred.result.rewards.currencies[i].amount);

                    currencyRewards[i] = new CurrencyExchange
                    {
                        currency = GameFoundation.catalogs.currencyCatalog.FindItem(dalDeferred.result.rewards.currencies[i].currencyId),
                        amount = dalDeferred.result.rewards.currencies[i].amount
                    };
                }

                // INVENTORY REWARD

                var itemsLength = dalDeferred.result.rewards.items.Length;
                var itemRewards = new InventoryItem[itemsLength];
                for (var i = 0; i < itemsLength; i++)
                {
                    var itemData = dalDeferred.result.rewards.items[i];
                    itemRewards[i] = InventoryManager.CreateItemInternal(itemData.definitionId, itemData.id);
                }

                // RESULT SENT TO CALLER

                TransactionResult result = new TransactionResult(
                    new TransactionCosts(itemCosts, currencyCosts),
                    new TransactionRewards(itemRewards, currencyRewards)
                );

                completer.SetProgression(3, 3);
                completer.Resolve(result);
                transactionProgressed?.Invoke(transaction, 3, 3);
                transactionSucceeded?.Invoke(transaction, result);
            }
            else
            {
                completer.Reject(dalDeferred.error);
                transactionFailed?.Invoke(transaction, dalDeferred.error);
            }

            dalDeferred.Release();
            s_CurrentVirtualTransaction = null;
        }

        static IEnumerator ProcessIAPTransactionCoroutine(
            IAPTransaction transaction,
            Completer<TransactionResult> completer)
        {
            completer.SetProgression(0, 4);
            transactionInitiated?.Invoke(transaction);
            transactionProgressed?.Invoke(transaction, 0, 4);

            if (purchasingAdapter == null)
            {
                var exception = new Exception(
                    "Tried to process an in-app purchase transaction with no platform purchase handler configured.");

                completer.Reject(exception);
                transactionFailed?.Invoke(transaction, exception);

                s_PendingIap = default;

                yield break;
            }

            if (string.IsNullOrEmpty(transaction.productId))
            {
                var exception = new Exception(
                    $"Transaction definition with id {transaction.id} doesn't have a product id.");

                completer.Reject(exception);
                transactionFailed?.Invoke(transaction, exception);

                s_PendingIap = default;

                yield break;
            }

            purchasingAdapter.BeginPurchase(transaction.productId, transaction.id);

            completer.SetProgression(1, 4);
            transactionProgressed?.Invoke(transaction, 1, 4);

            // wait until the purchasing adapter gives an outcome back to transaction manager

            while (!s_PendingIap.isSuccessful
                && string.IsNullOrEmpty(s_PendingIap.failureMessage))
            {
                yield return null;
            }

            // now the purchasing manager has sent back an outcome - now see if it failed

            if (!string.IsNullOrEmpty(s_PendingIap.failureMessage))
            {
                var exception = new Exception(s_PendingIap.failureMessage);

                completer.Reject(exception);
                transactionFailed?.Invoke(transaction, exception);

                s_PendingIap = default;

                yield break;
            }

            // at this point, we assume the platform purchase was successful

            completer.SetProgression(2, 4);
            transactionProgressed?.Invoke(transaction, 2, 4);

            // time to tell the data layer to validate and fulfill the transaction

            GameFoundation.GetPromiseHandles<TransactionExchangeData>(out var dalDeferred, out var dalCompleter);

            var successfulPurchaseData = purchasingAdapter.GetCurrentPurchaseData();

            // TODO: I need to pass in the transaction key instead of successfulPurchaseData.productId
            // (it should be ok to look up the transaction key based on the product id)

            if (purchasingAdapter.isAppleIOS)
            {
                s_DataLayer.RedeemAppleIap(
                    id: transaction.id,
                    receipt: successfulPurchaseData.receiptParts[0],
                    completer: dalCompleter);
            }
            else if (purchasingAdapter.isGooglePlay)
            {
                s_DataLayer.RedeemGoogleIap(
                    id: transaction.id,
                    purchaseData: successfulPurchaseData.receiptParts[0],
                    purchaseDataSignature: successfulPurchaseData.receiptParts[1],
                    completer: dalCompleter);
            }
            else if (purchasingAdapter.isFakeStore)
            {
                // TODO: fake a result based on the transaction asset values
                // TODO: something like s_DataLayer.RedeemTestIap() maybe ?
                // for now, just resolve it using the original requirements...
                // var result = new TransactionExchangeData()
                // {
                //     items = new []{},
                //     currencies = default
                // };
                // dalCompleter.Resolve(result);

                // actually for now, just pretend we're trying with apple
                s_DataLayer.RedeemAppleIap(transaction.id, "", dalCompleter);
            }
            else
            {
                var exception = new Exception(
                    "Game Foundation currently cannot redeem IAP for platforms other than Apple iOS or Google Play.");

                completer.Reject(exception);
                transactionFailed?.Invoke(transaction, exception);

                dalDeferred.Release();

                s_PendingIap = default;

                yield break;
            }

            while (!dalDeferred.isDone)
            {
                yield return null;
            }

            completer.SetProgression(3, 4);
            transactionProgressed?.Invoke(transaction, 3, 4);

            // now handle the response from the DAL
            // even if the platform purchase succeeded,
            // the data layer could still fail or reject it

            if (dalDeferred.isFulfilled)
            {
                // redeem locally in GF

                // WALLET REWARD

                var currenciesLength = dalDeferred.result.currencies.Length;
                var currencyRewards = new CurrencyExchange[currenciesLength];
                for (var i = 0; i < currenciesLength; i++)
                {
                    WalletManager.AddBalanceInternal(
                        dalDeferred.result.currencies[i].currencyId,
                        dalDeferred.result.currencies[i].amount);

                    currencyRewards[i] = new CurrencyExchange
                    {
                        currency = GameFoundation.catalogs.currencyCatalog.FindItem(dalDeferred.result.currencies[i].currencyId),
                        amount = dalDeferred.result.currencies[i].amount
                    };
                }

                // INVENTORY REWARD

                var itemsLength = dalDeferred.result.items.Length;
                var itemRewards = new InventoryItem[itemsLength];
                for (var i = 0; i < itemsLength; i++)
                {
                    var itemData = dalDeferred.result.items[i];
                    itemRewards[i] = InventoryManager.CreateItemInternal(itemData.definitionId, itemData.id);
                }

                // RESULT SENT TO CALLER

                TransactionResult result = new TransactionResult(

                    // there should never be item or currency costs for IAP transactions
                    costs: default,
                    rewards: new TransactionRewards(itemRewards, currencyRewards)
                );

                // tell the purchasing adapter that redemption worked and it
                // can stop asking TransactionManager to fulfill the purchase

                purchasingAdapter.CompletePendingPurchase(transaction.productId);

                // tell the caller that the purchase and redemption are successfully finished

                completer.Resolve(result);
                transactionProgressed?.Invoke(transaction, 4, 4);
                transactionSucceeded?.Invoke(transaction, result);
            }
            else
            {
                completer.Reject(dalDeferred.error);
                transactionFailed?.Invoke(transaction, dalDeferred.error);
            }

            dalDeferred.Release();

            s_PendingIap = default;
        }

        /// <summary>
        /// The purchasing adapter should call this method any time a purchase succeeds.
        /// </summary>
        /// <param name="productId">The IAP product ID that was successfully purchased.</param>
        /// <returns>TBD</returns>
        internal static Deferred<TransactionResult> FinalizeSuccessfulIAP(string productId)
        {
            if (s_PendingIap.iap != null)
            {
                if (s_PendingIap.iap.productId == productId)
                {
                    // this is the transaction we have been currently waiting on
                    s_PendingIap.isSuccessful = true;
                    // TODO: somehow give the caller something to wait for
                    // for now return a fake and already finished deferred
                    GameFoundation.GetPromiseHandles<TransactionResult>(out var deferredTemp, out var completerTemp);
                    completerTemp.Resolve(default);
                    return deferredTemp; // temporary
                    // TODO: this is returning before current transaction is set, so the caller isn't waiting for anything
                }

                // a successful purchase is being processed
                // but this product id wasn't the one we were expecting

                throw new Exception(
                    "Tried to finalize a successful purchase " +
                    "while already in the process of finalizing a different successful purchase.");
            }

            // this is some other success we were not expecting at all
            // and we're not currently processing anything
            // so, we want to honor this transaction silently
            // (such as when Android restores purchases automatically after a reinstall)

            GameFoundation.GetPromiseHandles<TransactionResult>(out var deferred, out var completer);

            GameFoundation.updater.StartCoroutine(ProcessIAPWithoutTransactionCoroutine(completer));

            return deferred;
        }

        static IEnumerator ProcessIAPWithoutTransactionCoroutine(Completer<TransactionResult> completer)
        {
            GameFoundation.GetPromiseHandles<TransactionExchangeData>(out var dalDeferred, out var dalCompleter);

            var successfulPurchaseData = purchasingAdapter.GetCurrentPurchaseData();

            s_PendingIap.iap = GameFoundation.catalogs.transactionCatalog
                .FindIAPTransactionByProductId(successfulPurchaseData.productId);

            if (s_PendingIap.iap == null)
            {
                completer.Reject(new System.Exception(
                    $"Could not find a transaction using product id '{successfulPurchaseData.productId}'."));

                dalDeferred.Release();
                s_PendingIap.iap = null;
                yield break;
            }

            if (purchasingAdapter.isAppleIOS)
            {
                s_DataLayer.RedeemAppleIap(
                    id: s_PendingIap.iap.id,
                    receipt: successfulPurchaseData.receiptParts[0],
                    completer: dalCompleter);
            }
            else if (purchasingAdapter.isGooglePlay)
            {
                s_DataLayer.RedeemGoogleIap(
                    id: s_PendingIap.iap.id,
                    purchaseData: successfulPurchaseData.receiptParts[0],
                    purchaseDataSignature: successfulPurchaseData.receiptParts[1],
                    completer: dalCompleter);
            }
            else if (purchasingAdapter.isFakeStore)
            {
                // TODO: fake a result based on the transaction asset values
                // TODO: something like s_DataLayer.RedeemTestIap() maybe ?
                // for now, just resolve it using the original requirements...
                // var result = new TransactionExchangeData()
                // {
                //     items = new []{},
                //     currencies = default
                // };
                // dalCompleter.Resolve(result);

                // actually for now, just pretend we're trying with apple
                s_DataLayer.RedeemAppleIap(s_PendingIap.iap.id, "", dalCompleter);
            }
            else
            {
                var exception = new System.Exception(
                    "Game Foundation currently cannot redeem IAP for platforms other than Apple iOS or Google Play.");

                completer.Reject(exception);

                dalDeferred.Release();
                s_PendingIap = (null, false, null);
                yield break;
            }

            while (!dalDeferred.isDone)
            {
                yield return null;
            }

            completer.SetProgression(3, 4);

            // now handle the response from the DAL
            // even if the platform purchase succeeded,
            // the data layer could still fail or reject it

            if (dalDeferred.isFulfilled)
            {
                // redeem locally in GF

                // WALLET REWARD

                var currenciesLength = dalDeferred.result.currencies.Length;
                var currencyRewards = new CurrencyExchange[currenciesLength];
                for (var i = 0; i < currenciesLength; i++)
                {
                    WalletManager.AddBalanceInternal(
                        dalDeferred.result.currencies[i].currencyId,
                        dalDeferred.result.currencies[i].amount);

                    currencyRewards[i] = new CurrencyExchange
                    {
                        currency = GameFoundation.catalogs.currencyCatalog.FindItem(dalDeferred.result.currencies[i].currencyId),
                        amount = dalDeferred.result.currencies[i].amount
                    };
                }

                // INVENTORY REWARD

                var itemsLength = dalDeferred.result.items.Length;
                var itemRewards = new InventoryItem[itemsLength];
                for (var i = 0; i < itemsLength; i++)
                {
                    itemRewards[i] = InventoryManager.CreateItemInternal(dalDeferred.result.items[i].definitionId);
                }

                // RESULT SENT TO CALLER

                TransactionResult result = new TransactionResult(
                    // there should never be item or currency costs for IAP transactions
                    costs: default,
                    rewards: new TransactionRewards(itemRewards, currencyRewards)
                );

                // tell the purchasing adapter that redemption worked and it
                // can stop asking TransactionManager to fulfill the purchase

                purchasingAdapter.CompletePendingPurchase(successfulPurchaseData.productId);

                // tell the caller that the purchase and redemption are successfully finished

                completer.Resolve(result);
            }
            else
            {
                completer.Reject(dalDeferred.error);
            }

            s_PendingIap = (null, false, null);
            dalDeferred.Release();
        }

        /// <summary>
        /// The purchasing adapter should call this method when a purchase fails.
        /// </summary>
        /// <param name="productId">The IAP product ID for the purchase that failed.</param>
        /// <param name="message">The reason for the failure.</param>
        internal static void PlatformPurchaseFailure(string productId, string message)
        {
            // is this the transaction we're currently waiting on?
            if (s_PendingIap.iap != null
                && s_PendingIap.iap.productId == productId)
            {
                // we are currently waiting on this transaction result

                // by setting the error message, the current coroutine will discover there was a failure
                s_PendingIap.failureMessage = message;

                return;
            }

            // this is some other failure we were not expecting

            transactionFailed?.Invoke(
                s_PendingIap.iap,
                new Exception(
                    "TransactionManager received an unexpected platform purchase failure " +
                    $"for product id '{productId}' with message: {message}"));
        }

        /// <summary>
        /// This uses the purchasing adapter to get localized product info from the platform store.
        /// </summary>
        /// <param name="productId">The product ID for which you want localized info.</param>
        /// <returns>A struct containing localized name and price strings.</returns>
        /// <exception cref="System.Exception">
        /// Throws an exception if no purchasing adapter has been initialized.
        /// </exception>
        public static LocalizedProductMetadata GetLocalizedIAPProductInfo(string productId)
        {
            ThrowIfNotInitialized();

            return purchasingAdapter.GetLocalizedProductInfo(productId);
        }
    }
}
