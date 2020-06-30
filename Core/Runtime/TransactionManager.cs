using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.GameFoundation.Data;
using UnityEngine.GameFoundation.DataAccessLayers;
using UnityEngine.Promise;
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
#endif

namespace UnityEngine.GameFoundation
{

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

    /// <summary>
    ///     Contains portable information about a successful IAP purchase.
    /// </summary>
    public struct PurchaseConfirmation
    {
        public string productId;
        public string[] receiptParts;
    }

    /// <summary>
    ///     Contains portable localized information about an IAP product.
    /// </summary>
    public struct LocalizedProductMetadata
    {
        public string name;
        public string price;
    }

#endif

    /// <summary>
    ///     This class contains methods to process virtual transactions and in-app purchases.
    /// </summary>
    public static class TransactionManager
    {
        /// <summary>
        ///     Invoked as soon as a valid transaction is initiated.
        /// </summary>
        public static event Action<BaseTransaction> transactionInitiated;

        /// <summary>
        ///     Invoked after every time the progress number is increased on a transaction's <see cref="Deferred{TResult}"/>.
        /// </summary>
        public static event Action<BaseTransaction, int, int> transactionProgressed;

        /// <summary>
        ///     Invoked after a transaction has succeeded at all levels.
        /// </summary>
        public static event Action<BaseTransaction, TransactionResult> transactionSucceeded;

        /// <summary>
        ///     Invoked after a transaction fails.
        /// </summary>
        public static event Action<BaseTransaction, Exception> transactionFailed;

        /// <summary>
        ///     Invoked after the purchasing adapter reports successfully being initialized.
        /// </summary>
        public static event Action purchasingAdapterInitializeSucceeded;

        /// <summary>
        ///     Invoked after the purchasing adapter reports failing to initialize.
        /// </summary>
        public static event Action<Exception> purchasingAdapterInitializeFailed;

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
        /// <summary>
        ///     An instance of the optional in-app purchasing adapter.
        /// </summary>
        static UnityPurchasingAdapter s_PurchasingAdapter;

        /// <summary>
        ///     Indicates whether we are expecting a response from UnityPurchasingAdapter.
        ///     It is possible to get purchase responses without expecting them.
        ///     Expected and unexpected purchase responses need to be handled in slightly different ways.
        /// </summary>
        static bool s_WaitingForPurchaseResponse;
#endif

        /// <summary>
        ///     Returns true after TransactionManager has been successfully initialized with a data layer.
        /// </summary>
        public static bool isInitialized { get; private set; }

        /// <summary>
        ///     Returns true if the optional purchasing adapter has finished initializing.
        /// </summary>
        public static bool purchasingAdapterIsInitialized { get; private set; }

        /// <summary>
        ///     Accessor to GameFoundation's current DAL.
        /// </summary>
        static ITransactionDataLayer dataLayer => GameFoundation.dataLayer;

        static VirtualTransaction s_CurrentVirtualTransaction;

        /// <summary>
        ///     This is a Transaction that is currently in progress, recently initiated by the user.
        /// </summary>
        public static IAPTransaction currentIap
        {
            get { return s_CurrentIap.transaction; }
        }

        static (IAPTransaction transaction, bool isSuccessful, string failureMessage) s_CurrentIap;

        /// <summary>
        /// List of all successfully-purchased IAP product ids.
        /// </summary>
        internal static List<string> s_PurchasedIapProducts;

        /// <summary>
        /// Filename to save iap products.  File will be written to App's persistent data path.
        /// </summary>
        const string k_purchasedIapProductsFilename = "iapProducts";

        /// <summary>
        /// Extension for json files 'json'.
        /// </summary>
        const string k_jsonExtension = "json";

        /// <summary>
        /// Fully qualified path for writing iap products to applications persistent data path.
        /// </summary>
        static string purchasedIapProductsFullPath => 
            $"{Application.persistentDataPath}/{k_purchasedIapProductsFilename}.{k_jsonExtension}";

        /// <summary>
        /// Fully qualified path for backing up iap products to applications persistent data path.
        /// </summary>
        static string purchasedIapProductsBackupPath => 
            $"{Application.persistentDataPath}/{k_purchasedIapProductsFilename}_backup.{k_jsonExtension}";

        internal static void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("TransactionManager is already initialized and cannot be initialized again.");
                return;
            }

            // read the purchased iap products list
            DeserializePurchasedIapProducts();

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            // you should only ever construct one UnityPurchasingAdapter per session
            if (s_PurchasingAdapter == null)
            {
                s_PurchasingAdapter = new UnityPurchasingAdapter();
            }
            s_PurchasingAdapter.Initialize();
#endif

            isInitialized = true;
        }

        internal static void Uninitialize()
        {
            if (!isInitialized)
                return;

            if (s_CurrentIap.transaction != null)
            {
                Debug.LogWarning("An IAP is still pending while uninitializing GameFoundation." +
                    " This might cause unexpected behaviour.");
            }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
            s_PurchasingAdapter?.Uninitialize();

            // don't nullify this; you should only ever construct one UnityPurchasingAdapter per session
            // s_PurchasingAdapter = null;

            purchasingAdapterIsInitialized = false;
#endif

            s_CurrentVirtualTransaction = null;
            s_CurrentIap = default;

            s_PurchasedIapProducts = null;

            isInitialized = false;
        }

        /// <summary>
        ///     Throws an exception if the TransactionManager has not been initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if not initialized.
        /// </exception>
        static void ThrowIfNotInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException(
                    "Error: GameFoundation.Initialize() must be called before the TransactionManager is used.");
            }
        }

        /// <summary>
        ///     The in-app purchasing adapter should call this method after it's successfully initialized.
        /// </summary>
        internal static void PurchasingAdapterInitializationSucceeded()
        {
            purchasingAdapterIsInitialized = true;
            purchasingAdapterInitializeSucceeded?.Invoke();
        }

        /// <summary>
        ///     The in-app purchasing adapter should call this if it fails to initialize.
        /// </summary>
        /// <param name="exception">
        ///     The reason for the failure.
        /// </param>
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

            if (transaction == null)
            {
                throw new ArgumentException("Transaction cannot be null.");
            }

            GameFoundation.GetPromiseHandles<TransactionResult>(out var deferred, out var completer);

            switch (transaction)
            {
                case VirtualTransaction virtualTransaction:
                {
                    // The given transaction is already being processed.
                    if (ReferenceEquals(transaction, s_CurrentVirtualTransaction))
                    {
                        completer.Reject(new ArgumentException(
                            $"This transaction key ({transaction.key}) is already being processed."));

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
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                    // The given transaction is already being processed.
                    if (ReferenceEquals(transaction, s_CurrentIap.transaction))
                    {
                        completer.Reject(new ArgumentException(
                            $"This transaction key ({transaction.key}) is already being processed."));

                        return deferred;
                    }

                    // Another IAP is already being processed.
                    if (s_CurrentIap.transaction != null)
                    {
                        completer.Reject(new ArgumentException(
                            $"Can't start the IAP \"{iapTransaction.productId}\" because"
                            + $" \"{s_CurrentIap.transaction.productId}\" is already being processed."));

                        return deferred;
                    }

                    // Make sure to reset the whole tuple to start on a clean slate.
                    s_CurrentIap = (iapTransaction, false, null);

                    GameFoundation.updater.StartCoroutine(
                        ProcessIAPTransactionCoroutine(completer, iapTransaction));

#else
                    completer.Reject(new NotSupportedException(
                        "Tried to process an IAP transaction, but IAP support is not enabled."));
#endif
                    break;
                }

                default:
                    completer.Reject(new ArgumentException(
                        $"Unknown or unsupported transaction type: {transaction.GetType()}"));
                    break;
            }

            return deferred;
        }

        /// <summary>
        ///     Coroutine which processes a <see cref="VirtualTransaction" /> and updates a <see cref="Completer{TResult}"/>.
        /// </summary>
        /// <param name="transaction">
        ///     The <see cref="VirtualTransaction" /> to process.
        /// </param>
        /// <param name="completer">
        ///     The <see cref="Completer{TResult}" /> to update as the <see cref="VirtualTransaction" /> is processed.
        /// </param>
        /// <param name="costItemIds">
        ///     The specific ids for each <see cref="InventoryItem" /> to consume as the cost for this transaction.
        ///     This parameter is required but it can be empty.
        /// </param>
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

            dataLayer.MakeVirtualTransaction(transaction.key, costItemIds, dalCompleter);

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
                var costs = ProcessCostsInternally(dalDeferred.result.cost);
                var rewards = ProcessRewardsInternally(dalDeferred.result.rewards);

                var result = new TransactionResult(costs, rewards);

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

        /// <summary>
        ///     Removes currency and items from the Game Foundation Wallet and Inventory without also informing the Data Layer.
        /// </summary>
        /// <param name="exchangeCost">
        ///     The costs to deduct from the Wallet and/or Inventory.
        /// </param>
        /// <returns>
        ///     An object representing the costs actually deducted, including ids of any Inventory Items that were removed.
        /// </returns>
        static TransactionCosts ProcessCostsInternally(TransactionExchangeData exchangeCost)
        {
            // WALLET COST

            var currencyCostCount = exchangeCost.currencies.Length;
            var currencyCosts = new CurrencyExchange[currencyCostCount];

            for (var i = 0; i < currencyCostCount; i++)
            {
                // the amount in the currency exchange is negative,
                // but RemoveBalance expects only positive numbers
                var amount = Math.Abs(exchangeCost.currencies[i].amount);
                WalletManager.RemoveBalanceInternal(exchangeCost.currencies[i].currencyKey, amount);

                currencyCosts[i] = new CurrencyExchange
                {
                    amount = exchangeCost.currencies[i].amount,
                    currency = GameFoundation.catalogs.currencyCatalog.FindItem(exchangeCost.currencies[i].currencyKey)
                };
            }

            // INVENTORY COST

            var itemCostCount = exchangeCost.items.Length;
            var itemCosts = new string[itemCostCount];

            for (var i = 0; i < itemCostCount; i++)
            {
                InventoryManager.RemoveItemInternal(exchangeCost.items[i].id);

                itemCosts[i] = exchangeCost.items[i].definitionKey;
            }

            return new TransactionCosts(itemCosts, currencyCosts);
        }

        /// <summary>
        ///     Grants items and currency to the Game Foundation Wallet and Inventory without also informing the Data Layer.
        /// </summary>
        /// <param name="exchange">
        ///     The rewards to grant to the wallet and/or inventory.
        /// </param>
        /// <returns>
        ///     An object representing the rewards actually granted, including any new Inventory Items created.
        /// </returns>
        static TransactionRewards ProcessRewardsInternally(TransactionExchangeData exchange)
        {
            // WALLET

            var currenciesLength = exchange.currencies.Length;
            var currencyRewards = new CurrencyExchange[currenciesLength];
            for (var i = 0; i < currenciesLength; i++)
            {
                WalletManager.AddBalanceInternal(
                    exchange.currencies[i].currencyKey,
                    exchange.currencies[i].amount);

                currencyRewards[i] = new CurrencyExchange
                {
                    currency = GameFoundation.catalogs.currencyCatalog.FindItem(exchange.currencies[i].currencyKey),
                    amount = exchange.currencies[i].amount
                };
            }

            // INVENTORY

            var itemsLength = exchange.items.Length;
            var itemRewards = new InventoryItem[itemsLength];
            for (var i = 0; i < itemsLength; i++)
            {
                var itemData = exchange.items[i];

                itemRewards[i] = InventoryManager.CreateItemInternal(itemData.definitionKey, itemData.id);
            }

            // RESULT

            return new TransactionRewards(itemRewards, currencyRewards);
        }

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

        /// <summary>
        ///     Set a validator instance for the purchasing adapter to use.
        /// </summary>
        /// <param name="validator">
        ///     The validator reference to set.
        /// </param>
        public static void SetIAPValidator(CrossPlatformValidator validator)
        {
            s_PurchasingAdapter.validator = validator;
        }

        /// <summary>
        ///     A list of <see cref="PurchaseEventArgs" /> that were not automatically
        ///     processed because the "Process Background Purchases" option was unchecked.
        ///     It is up to the developer to fully process these.
        /// </summary>
        public static List<PurchaseEventArgs> unprocessedPurchases
            => s_PurchasingAdapter.unprocessedPurchases;

        /// <summary>
        ///     Use this to manually process purchases that succeeded in the background,
        ///     such as when purchases are restored, or automatically processed from a previous session. 
        /// </summary>
        /// <param name="purchaseEventArgs">
        ///     The <see cref="PurchaseEventArgs" /> to process.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="Deferred" /> struct which can be used to track the state of the processing.
        /// </returns>
        public static Deferred<TransactionResult> ProcessPurchaseEventArgs(PurchaseEventArgs purchaseEventArgs)
        {
            // send purchaseEventArgs to UnityPurchasingAdapter for immediate processing

            GameFoundation.GetPromiseHandles<TransactionResult>(out var deferred, out var completer);

            GameFoundation.updater.StartCoroutine(
                s_PurchasingAdapter.ValidateAndProcessSuccessfulPurchase(
                    purchaseEventArgs, completer));

            return deferred;
        }

        /// <summary>
        ///     Coroutine which processes an <see cref="IAPTransaction" /> and updates a <see cref="Completer{TResult}"/>.
        /// </summary>
        /// <param name="completer">
        ///     The <see cref="Completer{TResult}" /> to update as the <see cref="IAPTransaction" /> is processed.
        /// </param>
        /// <param name="transaction">
        ///     An optional <see cref="IAPTransaction"/> object to resolve.
        ///     If one is provided, then that means it is a foreground purchase (purchase currently in progress).
        ///     If none is provided, then this is likely a background purchase (restore purchases, delayed success, etc).
        /// </param>
        static IEnumerator ProcessIAPTransactionCoroutine(
            Completer<TransactionResult> completer, IAPTransaction transaction = null)
        {
            var hasTransaction = transaction != null;

            completer.SetProgression(0, 4);

            if (hasTransaction)
            {
                transactionInitiated?.Invoke(transaction);
                transactionProgressed?.Invoke(transaction, 0, 4);
            }

            if (s_PurchasingAdapter == null)
            {
                var exception = new Exception(
                    "Tried to process an in-app purchase transaction with no platform purchase handler configured.");

                completer.Reject(exception);

                if (hasTransaction)
                {
                    transactionFailed?.Invoke(transaction, exception);
                }

                s_CurrentIap = default;

                yield break;
            }

            if (hasTransaction && string.IsNullOrEmpty(transaction.productId))
            {
                var exception = new Exception(
                    $"Transaction definition with key {transaction.key} doesn't have a product id.");

                completer.Reject(exception);
                transactionFailed?.Invoke(transaction, exception);

                s_CurrentIap = default;

                yield break;
            }

            if (hasTransaction)
            {
                // if the transaction is null, it means this coroutine is being used to process a background purchase
                // that means the purchase was already successful in the past,
                // so we don't need to wait for a response from the IAP SDK (we won't get one)

                s_WaitingForPurchaseResponse = true;

                s_PurchasingAdapter.BeginPurchase(transaction.productId);
            }

            completer.SetProgression(1, 4);

            if (hasTransaction)
            {
                transactionProgressed?.Invoke(transaction, 1, 4);
            }

            // wait for the purchasing adapter

            while (s_WaitingForPurchaseResponse) yield return null;

            // purchasing manager has now responded with success or failure

            if (!string.IsNullOrEmpty(s_CurrentIap.failureMessage))
            {
                var exception = new Exception(s_CurrentIap.failureMessage);

                completer.Reject(exception);

                if (hasTransaction)
                {
                    transactionFailed?.Invoke(transaction, exception);
                }

                s_CurrentIap = default;

                yield break;
            }

            // at this point, we assume the platform purchase was successful

            completer.SetProgression(2, 4);

            if (hasTransaction)
            {
                transactionProgressed?.Invoke(transaction, 2, 4);
            }

            // now send it all to the data layer

            var confirmation = s_PurchasingAdapter.GetCurrentPurchaseData();

            yield return SuccessfulPurchaseToDataLayer(confirmation, completer, transaction);
        }

        /// <summary>
        ///     Send a successful purchase to the data layer for processing there.
        /// </summary>
        /// <param name="confirmation">
        ///     The successful purchase data to send to the data layer for processing.
        /// </param>
        /// <param name="completer">
        ///     A completer for updating progress.
        /// </param>
        /// <param name="transaction">
        ///     If this is null, then the purchase will be treated as a "background purchase".
        ///     This can be the case when restoring multiple purchases, or finishing a
        ///     purchase that was interrupted in a previous session.
        /// </param>
        static IEnumerator SuccessfulPurchaseToDataLayer(
            PurchaseConfirmation confirmation,
            Completer<TransactionResult> completer,
            IAPTransaction transaction = null)
        {
            GameFoundation.GetPromiseHandles<TransactionExchangeData>(out var dalDeferred, out var dalCompleter);

            if (transaction == null)
            {
                s_CurrentIap.transaction = GameFoundation.catalogs.transactionCatalog
                    .FindIAPTransactionByProductId(confirmation.productId);
                if (s_CurrentIap.transaction == null)
                {
                    completer.Reject(new Exception(
                        $"Could not find a transaction using product id '{confirmation.productId}'."));
                    dalDeferred.Release();
                    s_CurrentIap.transaction = null;
                    yield break;
                }
            }

            if (s_PurchasingAdapter.isAppleIOS)
            {
                dataLayer.RedeemAppleIap(
                    key: s_CurrentIap.transaction.key,
                    receipt: confirmation.receiptParts[0],
                    completer: dalCompleter);
            }
            else if (s_PurchasingAdapter.isGooglePlay)
            {
                dataLayer.RedeemGoogleIap(
                    key: s_CurrentIap.transaction.key,
                    purchaseData: confirmation.receiptParts[0],
                    purchaseDataSignature: confirmation.receiptParts[1],
                    completer: dalCompleter);
            }
            else if (s_PurchasingAdapter.isFakeStore)
            {
                // TODO: fake a result based on the transaction asset values
                // TODO: something like s_DataLayer.RedeemTestIap() maybe ?
                // for now, just pretend we're trying with apple
                dataLayer.RedeemAppleIap(
                    key: s_CurrentIap.transaction.key,
                    receipt: "",
                    completer: dalCompleter);
            }
            else
            {
                completer.Reject(new Exception(
                    "Game Foundation currently cannot redeem IAP for platforms other than Apple iOS or Google Play."));
            }

            while (!dalDeferred.isDone)
            {
                yield return null;
            }

            completer.SetProgression(3, 4);

            if (transaction != null)
            {
                transactionProgressed?.Invoke(transaction, 3, 4);
            }

            // now handle the response from the DAL
            // even if the platform purchase succeeded,
            // the data layer could still fail or reject it

            if (dalDeferred.isFulfilled)
            {
                TransactionCosts costs = default; // an IAP transaction should never have virtual costs
                var rewards = ProcessRewardsInternally(dalDeferred.result);
                var result = new TransactionResult(costs, rewards);

                // tell the purchasing adapter that redemption worked and it
                // can stop asking TransactionManager to fulfill the purchase

                s_PurchasingAdapter.CompletePendingPurchase(confirmation.productId);

                // tell the caller that the purchase and redemption are successfully finished

                completer.Resolve(result);

                // check for product definition in purchased product and in transaction

                ProductDefinition productDefinition = null;

                var purchasedProduct = s_PurchasingAdapter.FindProduct(confirmation.productId);
                if (purchasedProduct?.definition != null)
                {
                    productDefinition = purchasedProduct.definition;
                }
                else if (transaction?.product?.definition != null)
                {
                    productDefinition = transaction.product.definition;
                }
                else
                {
                    // TODO: a way to add non-fatal warnings to a successful promise

                    Debug.LogError("Processed a purchase for a product id that cannot " +
                                   "be found in the currently loaded IAP catalog.");
                }

                // if non-consumable, add the purchase to list of successfully purchased IAP products
                if (productDefinition?.type == ProductType.NonConsumable)
                {
                    AddPurchasedIapProduct(confirmation.productId);
                }

                if (transaction != null)
                {
                    transactionProgressed?.Invoke(transaction, 4, 4);
                    transactionSucceeded?.Invoke(transaction, result);
                }

                // TODO: should we invoke a different event for a background purchase?
            }
            else
            {
                completer.Reject(dalDeferred.error);

                if (transaction != null)
                {
                    transactionFailed?.Invoke(transaction, dalDeferred.error);
                }
            }

            s_CurrentIap = (null, false, null);
            dalDeferred.Release();
        }

        /// <summary>
        ///     The purchasing adapter should call this method any time a purchase succeeds.
        /// </summary>
        /// <param name="productId">
        ///     The IAP product ID that was successfully purchased.
        /// </param>
        /// <returns>
        ///     A Deferred which the caller can use to monitor the progress.
        /// </returns>
        internal static Deferred<TransactionResult> FinalizeSuccessfulIAP(string productId)
        {
            if (s_CurrentIap.transaction != null)
            {
                if (s_CurrentIap.transaction.productId == productId)
                {
                    s_WaitingForPurchaseResponse = false;

                    // this is the transaction we have been currently waiting on
                    s_CurrentIap.isSuccessful = true;

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

            // by calling this coroutine without a transaction parameter,
            // the purchase will be treated as a "background purchase"
            GameFoundation.updater.StartCoroutine(ProcessIAPTransactionCoroutine(completer));

            return deferred;
        }

        /// <summary>
        ///     The purchasing adapter should call this method when a purchase fails.
        /// </summary>
        /// <param name="productId">
        ///     The IAP product ID for the purchase that failed.
        /// </param>
        /// <param name="message">
        ///     The reason for the failure.
        /// </param>
        internal static void PlatformPurchaseFailure(string productId, string message)
        {
            // is this the transaction we're currently waiting on?
            if (s_CurrentIap.transaction != null
                && s_CurrentIap.transaction.productId == productId)
            {
                // by setting the error message, the current coroutine will discover there was a failure
                s_CurrentIap.failureMessage = message;

                // we were waiting for this particular response, but not anymore
                s_WaitingForPurchaseResponse = false;

                return;
            }

            // this is some other failure we were not expecting

            transactionFailed?.Invoke(
                s_CurrentIap.transaction,
                new Exception(
                    "TransactionManager received an unexpected platform purchase failure " +
                    $"for product id '{productId}' with message: {message}"));
        }

        /// <summary>
        ///     This uses the purchasing adapter to get localized product info from the platform store.
        /// </summary>
        /// <param name="productId">
        ///     The product ID for which you want localized info.
        /// </param>
        /// <returns>
        ///     A struct containing localized name and price strings.
        /// </returns>
        /// <exception cref="Exception">
        ///     Throws an exception if no purchasing adapter has been initialized.
        /// </exception>
        public static LocalizedProductMetadata GetLocalizedIAPProductInfo(string productId)
        {
            ThrowIfNotInitialized();

            return s_PurchasingAdapter.GetLocalizedProductInfo(productId);
        }

        /// <summary>
        ///     Get the IAP product info from the initialized Unity Purchasing instance.
        /// </summary>
        /// <param name="transaction">
        ///     An <see cref="IAPTransaction"/> with a productId to search for in the catalog.
        /// </param>
        /// <returns>
        ///     A <see cref="Product"/> from the IAP Catalog.
        /// </returns>
        public static Product FindIAPProduct(IAPTransaction transaction)
        {
            return transaction == null ? null : FindIAPProduct(transaction.productId);
        }

        /// <summary>
        ///     Get the IAP product info from the initialized Unity Purchasing instance.
        /// </summary>
        /// <param name="productId">
        ///     The product ID to search for in the catalog.
        /// </param>
        /// <returns>
        ///     A <see cref="Product"/> from the IAP Catalog.
        /// </returns>
        public static Product FindIAPProduct(string productId)
        {
            ThrowIfNotInitialized();
            if (string.IsNullOrEmpty(productId)) return null;

            return s_PurchasingAdapter.FindProduct(productId);
        }

        // TODO: support "Restore Purchases" on platforms other than iOS
#if UNITY_IPHONE
        /// <summary>
        ///     Tells the IAP SDK to begin the purchase restoration process (only works on iOS).
        /// </summary>
        public static void RestorePurchases()
        {
            s_PurchasingAdapter.RestorePurchases();
        }
#endif

#endif

        /// <summary>
        ///     Add IAP Product Id to list of successfully purchased IAP products.
        /// </summary>
        /// <param name="productId">Product Id to add.</param>
        /// <returns>
        ///     true if new IAP product was added, else false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if <see cref="TransactionManager"/> has not been initialized.
        /// </exception>
        static internal bool AddPurchasedIapProduct(string productId)
        {
            if (IsIapProductOwned(productId))
            {
                return false;
            }

            s_PurchasedIapProducts.Add(productId);

            SerializePurchasedIapProducts();

            return true;
        }

        /// <summary>
        ///     Remove IAP Product Id (if it exists).
        /// </summary>
        /// <param name="productId">Product Id to remove.</param>
        /// <returns>
        ///     true if IAP Product Id was found and removed, else false.
        /// </returns>
        static internal bool RemovePurchasedIapProduct(string productId)
        {
            if (s_PurchasedIapProducts.Remove(productId))
            {
                SerializePurchasedIapProducts();

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Determine if specified Product Id is owned by the player.
        /// </summary>
        /// <param name="productId">Product Id for which to search.</param>
        /// <returns>
        ///     true if specifeid Product Id is owned by the player, otherwise false.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if <see cref="TransactionManager"/> has not been initialized.
        /// </exception>
        public static bool IsIapProductOwned(string productId)
        {
            ThrowIfNotInitialized();

            return s_PurchasedIapProducts.Contains(productId);
        }

        /// <summary>
        ///     Remove all Product Ids.
        /// </summary>
        static internal void RemoveAllPurchasedIapProducts()
        {
            if (s_PurchasedIapProducts.Count > 0)
            {
                s_PurchasedIapProducts.Clear();

                SerializePurchasedIapProducts();
            }
        }

        /// <summary>
        ///     Gets full list of all purchased iap products.
        /// </summary>
        static internal List<string> GetPurchasedIapProducts()
        {
            return new List<string>(s_PurchasedIapProducts);
        }

        /// <summary>
        ///     Sets and serializes list of purchased iap products.  
        ///     Used mainly to restore list after unit testing.
        /// </summary>
        /// <param name="purchasedIapProducts">List of iap products.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <see cref="purchasedIapProducts"/> is null.
        /// </exception>
        static internal void SetPurchasedIapProducts(List<string> purchasedIapProducts)
        {
            Tools.ThrowIfArgNull(purchasedIapProducts, nameof(purchasedIapProducts));

            s_PurchasedIapProducts = purchasedIapProducts;
            SerializePurchasedIapProducts();
        }

        /// <summary>
        ///     Simple wrapper for list of IAP Product Id strings.  Needed to serialize to json format.
        /// </summary>
        class IapProductsJsonWrapper
        {
            /// <summary>
            ///     Create a Json wrapper for specified list of IAP Product Ids.
            /// </summary>
            /// <param name="iapProducts">List of IAP Product Id strings.</param>
            public IapProductsJsonWrapper(List<string> iapProducts)
            {
                this.iapProducts = iapProducts;
            }

            /// <summary>
            ///     Construct Json wrapper from a json string.
            /// </summary>
            /// <param name="iapProducts"></param>
            public IapProductsJsonWrapper(string iapProducts)
            {
                this.iapProducts =
                    JsonUtility.FromJson<IapProductsJsonWrapper>(iapProducts).iapProducts;
            }

            /// <summary>
            ///     List of IAP Product Id strings in this json wrapper.
            ///     Note: it is not possible to use property here as JsonUtility 
            ///     needs to access the list as a memeber.
            /// </summary>
            public List<string> iapProducts;
        }

        /// <summary>
        ///     Helper method to write iap products list as json to app's persistent data path.
        /// </summary>
        static void SerializePurchasedIapProducts()
        {
            // write iap products to json file in App persistent data path
            using (var sw = new StreamWriter(purchasedIapProductsFullPath, false, Encoding.Default))
            {
                var ispProducts = new IapProductsJsonWrapper(s_PurchasedIapProducts);
                var jsonString = JsonUtility.ToJson(ispProducts);
                sw.Write(jsonString);
            }

            // make a backup
            File.Copy(purchasedIapProductsFullPath, purchasedIapProductsBackupPath, true);
        }

        /// <summary>
        ///     Helper method to read iap products list as json from app's persistent data path.
        /// </summary>
        static void DeserializePurchasedIapProducts()
        {
            // read from default path/filename
            string path = purchasedIapProductsFullPath;

            // if the main file doesn't exist, check for backup
            if (!File.Exists(path))
            {
                path = purchasedIapProductsBackupPath;
                if (!File.Exists(path))
                {
                    // if neither main nor backup file exist, clear out the iap products list and exit
                    s_PurchasedIapProducts = new List<string>();

                    return;
                }
            }

            // read list of IAP Product Id strings and store for later use.
            var fileInfo = new FileInfo(path);
            using (var sr = new StreamReader(fileInfo.OpenRead(), Encoding.Default))
            {
                var iapProductsWrapper = JsonUtility.FromJson<IapProductsJsonWrapper>(sr.ReadToEnd());
                s_PurchasedIapProducts = iapProductsWrapper.iapProducts;

                // if json was empty or formatted incorrectly recreate empty list to avoid later null 
                // ref errors and permit future purchasing
                if (s_PurchasedIapProducts == null)
                {
                    s_PurchasedIapProducts = new List<string>();
                }
            }
        }
    }
}
