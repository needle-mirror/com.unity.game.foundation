#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Promise;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace UnityEngine.GameFoundation
{
    /// <summary> 
    ///     This class adapts the Unity Purchasing SDK (or IAP SDK) to Game Foundation. 
    ///     To use it, pass an instance of this class in as an arg when initializing the TransactionManager. 
    ///     The TransactionManager will then call methods on this class instance automatically 
    ///     to do things like initiate a real money purchase on the adapted platform. 
    ///     This class instance will also make calls back to the TransactionManager 
    ///     to do things like update the status of a purchase in progress. 
    /// </summary> 
    internal sealed class UnityPurchasingAdapter : IStoreListener
    {
        /// <summary>
        ///     Are we currently running on the Apple iOS platform?
        /// </summary>
        public bool isAppleIOS { get; private set; }

        /// <summary>
        ///     Are we currently running on the Google Play platform?
        /// </summary>
        public bool isGooglePlay { get; private set; }

        /// <summary>
        ///     Are we currently running on a fake platform just for testing?
        /// </summary>
        public bool isFakeStore { get; private set; }

        bool m_IsInitialized;

        IStoreController m_Controller;
        Action m_OnInitializeSucceededCallback;
        Action<Exception> m_OnInitializeFailedCallback;

        IAppleExtensions m_AppleExtensions;
        IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
        ITransactionHistoryExtensions m_TransactionHistoryExtensions;

        /// <summary> 
        ///     This class, specific to the Unity Purchasing SDK, will enable automatic local validation of receipts. 
        ///     Creating a validator requires you to first use the menu item 
        ///     Window > Unity IAP > Receipt Validation Obfuscator 
        /// </summary> 
        public CrossPlatformValidator validator { get; set; }

        /// <summary>
        ///     The purchase response from the IAP SDK which is currently being processed.
        /// </summary>
        public PurchaseEventArgs currentPurchaseEventArgs { get; private set; }

        Queue<PurchaseEventArgs> m_SuccessfulPurchaseQueue;
        /// <summary>
        ///     A queue (First In First Out) of <see cref="PurchaseEventArgs" />
        ///     that have been returned by the platform store (via the IAP SDK) as successful.
        ///     This can include restored purchases.
        /// </summary>
        public Queue<PurchaseEventArgs> successfulPurchaseQueue => m_SuccessfulPurchaseQueue;

        public List<PurchaseEventArgs> m_UnprocessedPurchases;
        /// <summary>
        ///     A list of purchases that is populated by background purchases if the
        ///     "Process Background Purchases" option is unchecked.
        ///     It is then up to the developer to process the purchases in this list.
        /// </summary>
        public List<PurchaseEventArgs> unprocessedPurchases => m_UnprocessedPurchases;

        Coroutine m_SuccessfulPurchaseQueueCoroutine;

        /// <summary>
        ///     Return a struct that contains just the data about a successful purchase
        ///     which the TransactionManager can use to fulfill and/or finalize a purchase.
        /// </summary>
        /// <returns>
        ///     A PurchaseConfirmation populated with a product id and as many receipt
        ///     strings as that platform requires for receipt validation.
        /// </returns>
        public PurchaseConfirmation GetCurrentPurchaseData()
        {
            ThrowIfNotInitialized();

            if (currentPurchaseEventArgs == null) return default;

            var confirmation = new PurchaseConfirmation
            {
                productId = currentPurchaseEventArgs.purchasedProduct.definition.storeSpecificId
            };

            if (Debug.isDebugBuild)
            {
                Debug.Log($"receipt: {currentPurchaseEventArgs.purchasedProduct.receipt}");
            }

            if (!(Purchasing.MiniJson.JsonDecode(
                currentPurchaseEventArgs.purchasedProduct.receipt) is Dictionary<string, object> receiptDict))
            {
                // this will be malformed and shouldn't successfully validate 
                return confirmation;
            }

            if (isAppleIOS)
            {
                confirmation.receiptParts = new[]
                {
                    receiptDict.TryGetValue("Payload", out var payloadObj) ? payloadObj.ToString() : ""
                };
            }
            else if (isGooglePlay)
            {
                confirmation.receiptParts = new string[2];

                if (!receiptDict.TryGetValue("Payload", out var payloadObj))
                {
                    return confirmation;
                }

                var payloadString = payloadObj as string;
                if (string.IsNullOrEmpty(payloadString))
                {
                    return confirmation;
                }

                if (!(Purchasing.MiniJson.JsonDecode(payloadString) is Dictionary<string, object> payloadDict))
                {
                    return confirmation;
                }

                if (payloadDict.TryGetValue("json", out var jsonObj)
                    && jsonObj is string jsonString)
                {
                    confirmation.receiptParts[0] = jsonString;
                }

                if (payloadDict.TryGetValue("signature", out var signatureObj)
                    && signatureObj is string signatureString)
                {
                    confirmation.receiptParts[1] = signatureString;
                }
            }

            return confirmation;
        }

        /// <summary>
        ///     Called to set up anything the platform SDK needs in order to process real money purchase requests.
        /// </summary>
        /// <param name="onInitializeSucceededCallback">
        ///     A method to call when initialization fails.
        /// </param>
        /// <param name="onInitializeFailedCallback">
        ///     A method to call when initialization is successful.
        /// </param>
        public void Initialize(
            Action onInitializeSucceededCallback = null,
            Action<Exception> onInitializeFailedCallback = null)
        {
            Initialize(FakeStoreUIMode.StandardUser, onInitializeSucceededCallback, onInitializeFailedCallback);
        }

        /// <summary> 
        ///     Configures the Unity Purchasing SDK with data from the IAP Catalog. 
        /// </summary> 
        /// <param name="onInitializeSucceededCallback">
        ///     A method to call when initialization fails.
        /// </param> 
        /// <param name="onInitializeFailedCallback">
        ///     A method to call when initialization is successful.
        /// </param> 
        /// <param name="uiMode">
        ///     Use in conjunction with StandardPurchasingModule.useFakeStoreAlways to test different scenarios.
        /// </param> 
        void Initialize(
            FakeStoreUIMode uiMode,
            Action onInitializeSucceededCallback = null,
            Action<Exception> onInitializeFailedCallback = null)
        {
            if (m_IsInitialized)
            {
                // already been initialized once, so don't reinitialize the purchasing controller 

                TransactionManager.PurchasingAdapterInitializationSucceeded();

                m_SuccessfulPurchaseQueueCoroutine =
                    GameFoundation.updater.StartCoroutine(ProcessSuccessfulPurchaseQueue());

                return;
            }

            m_OnInitializeSucceededCallback = onInitializeSucceededCallback;
            m_OnInitializeFailedCallback = onInitializeFailedCallback;
            m_SuccessfulPurchaseQueue = new Queue<PurchaseEventArgs>();
            m_UnprocessedPurchases = new List<PurchaseEventArgs>();

            var module = StandardPurchasingModule.Instance();

            module.useFakeStoreUIMode = uiMode;

#if UNITY_EDITOR
            module.useFakeStoreAlways = true;
            isFakeStore = true;
#endif

            var builder = ConfigurationBuilder.Instance(module);

            isGooglePlay = Application.platform == RuntimePlatform.Android
                && module.appStore == AppStore.GooglePlay;

            isAppleIOS = Application.platform == RuntimePlatform.IPhonePlayer;

            var catalog = ProductCatalog.LoadDefaultCatalog();

            foreach (var product in catalog.allValidProducts)
            {
                if (product.allStoreIDs.Count > 0)
                {
                    var ids = new IDs();

                    foreach (var storeID in product.allStoreIDs)
                    {
                        ids.Add(storeID.id, storeID.store);

#if UNITY_EDITOR
#if UNITY_ANDROID
                        if (storeID.store == "GooglePlay") 
                            ids.Add(storeID.id, "fake");
#elif UNITY_IOS
                        if (storeID.store == "AppleAppStore")  
                            ids.Add(storeID.id, "fake");
#endif
#endif
                    }

                    builder.AddProduct(product.id, product.type, ids);
                }
                else
                {
                    builder.AddProduct(product.id, product.type);
                }
            }

            UnityPurchasing.Initialize(this, builder);
        }

        /// <summary> 
        ///     The TransactionManager will call this method when it is uninitialized, when Game Foundation is uninitialized.
        ///     Stops the loop that continue to process new successful purchase responses from the platform. 
        /// </summary> 
        public void Uninitialize()
        {
            if (m_SuccessfulPurchaseQueueCoroutine != null)
            {
                GameFoundation.updater.StopCoroutine(m_SuccessfulPurchaseQueueCoroutine);
            }

            m_SuccessfulPurchaseQueueCoroutine = null;
        }

        void ThrowIfNotInitialized()
        {
            if (!m_IsInitialized)
            {
                throw new InvalidOperationException(
                    "Error: GameFoundation.Initialize() must be called before the TransactionManager is used.");
            }
        }

        /// <inheritdoc cref="IStoreListener.OnInitialized" /> 
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_Controller = controller;
            m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
            m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
            m_TransactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();

            m_IsInitialized = true;

            TransactionManager.PurchasingAdapterInitializationSucceeded();

            m_OnInitializeSucceededCallback?.Invoke();
            m_OnInitializeSucceededCallback = null;
            m_OnInitializeFailedCallback = null;

            m_SuccessfulPurchaseQueueCoroutine =
                GameFoundation.updater.StartCoroutine(ProcessSuccessfulPurchaseQueue());
        }

        /// <inheritdoc cref="IStoreListener.OnInitializeFailed" /> 
        public void OnInitializeFailed(InitializationFailureReason initializationFailureReason)
        {
            var exception = new Exception(
                "Unity Purchasing failed to initialize, for the following reason: " +
                "An unrecognized problem occurred!");

            switch (initializationFailureReason)
            {
                case InitializationFailureReason.AppNotKnown:

                    exception = new Exception(
                        "Unity Purchasing failed to initialize, for the following reason: " +
                        "Unknown app. Make sure your app was uploaded to the respective platform store.");

                    break;

                case InitializationFailureReason.PurchasingUnavailable:

                    exception = new Exception(
                        "Unity Purchasing failed to initialize, for the following reason: " +
                        "Purchasing is not enabled on this platform.");

                    break;

                case InitializationFailureReason.NoProductsAvailable:

                    exception = new Exception(
                        "Unity Purchasing failed to initialize, for the following reason: " +
                        "No products are available for purchase.");

                    break;
            }

            TransactionManager.PurchasingAdapterInitializationFailed(exception);

            m_OnInitializeFailedCallback?.Invoke(exception);
            m_OnInitializeSucceededCallback = null;
            m_OnInitializeFailedCallback = null;
        }

        /// <inheritdoc cref="IStoreListener.ProcessPurchase" /> 
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEventArgs)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log("received purchaseEventArgs from IAP SDK");
            }

            // TODO: Contains() might not work right if Unity Purchasing creates new duplicate PurchaseEventArgs instances 
            // TODO: maybe it's fine to queue up the same eventargs multiple times? edge case? 
            if (!m_SuccessfulPurchaseQueue.Contains(purchaseEventArgs))
            {
                m_SuccessfulPurchaseQueue.Enqueue(purchaseEventArgs);
            }

            // the result should always be pending, 
            // and TransactionManager will complete it (maybe immediately, maybe async) 
            return PurchaseProcessingResult.Pending;
        }

        IEnumerator ProcessSuccessfulPurchaseQueue()
        {
            while (true)
            {
                yield return null;

                if (m_Controller == null) continue;
                if (!TransactionManager.isInitialized) continue;
                if (currentPurchaseEventArgs != null) continue;
                if (m_SuccessfulPurchaseQueue.Count <= 0) continue;

                var nextProductId = m_SuccessfulPurchaseQueue.Peek()?.purchasedProduct.definition.storeSpecificId;

                if (string.IsNullOrEmpty(nextProductId)) continue; // TODO: throw?

                // check if this is a background purchase

                if (TransactionManager.currentIap == null
                    || TransactionManager.currentIap.productId != nextProductId)
                {
                    // since this purchase is not the current purchase, it is a background purchase

                    if (!GameFoundationSettings.ProcessBackgroundPurchases)
                    {
                        // don't process it
                        // move it to the background list instead
                        m_UnprocessedPurchases.Add(m_SuccessfulPurchaseQueue.Dequeue());
                        continue;
                    }
                }

                // now we know it's ok to start processing this purchase

                GameFoundation.GetPromiseHandles<TransactionResult>(out var deferred, out var completer);

                // first wait for this coroutine to finish
                yield return ValidateAndProcessSuccessfulPurchase(m_SuccessfulPurchaseQueue.Dequeue(), completer);

                // inside the above coroutine, it could start another async process, so now wait for that too
                while (!deferred.isDone)
                {
                    yield return null;
                }

                // now it's ok to continue processing the queue

                // TODO: PurchaseEventArgs that are in the queue for too long (minutes?, hours?, days?) should be moved to unprocessed list
            }
        }

        /// <summary>
        ///     Process a <see cref="PurchaseEventArgs"/>.
        ///     If validation is enabled, this will locally validate the purchase's receipt data. 
        /// </summary>
        /// <param name="purchaseEventArgs">
        ///     The <see cref="PurchaseEventArgs" /> to process.
        /// </param>
        /// <param name="completer">
        ///     Passed along to communicate the state of a bigger process.
        /// </param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if UnityPurchasingAdapter either hasn't been initialized, or is already processing a PurchaseEventArgs.
        /// </exception>
        public IEnumerator ValidateAndProcessSuccessfulPurchase(
            PurchaseEventArgs purchaseEventArgs,
            Completer<TransactionResult> completer)
        {
            if (!m_IsInitialized)
            {
                completer.Reject(new InvalidOperationException(
                    "Error: GameFoundation.Initialize() must be called before the TransactionManager is used."));
                yield break;
            }

            if (currentPurchaseEventArgs != null)
            {
                completer.Reject(new InvalidOperationException(
                    "UnityPurchasingAdapter is already currently processing a purchaseEventArgs."));
                yield break;
            }

            if (purchaseEventArgs == null)
            {
                completer.Reject(new ArgumentNullException(nameof(purchaseEventArgs)));
                yield break;
            }

            currentPurchaseEventArgs = purchaseEventArgs;

            // LOCAL RECEIPT VALIDATION

            var isValid = true;

            if (validator != null)
            {
                if (isGooglePlay || isAppleIOS)
                {
                    try
                    {
                        var result =
                            validator.Validate(currentPurchaseEventArgs.purchasedProduct.receipt);

                        if (Debug.isDebugBuild)
                        {
                            var sb = new StringBuilder("");
                            sb.AppendLine("Receipt is valid. Contents:");

                            foreach (var purchaseReceipt in result)
                            {
                                sb.AppendLine($"product id: {purchaseReceipt.productID}");
                                sb.AppendLine($"purchase date: {purchaseReceipt.purchaseDate}");
                                sb.AppendLine($"transaction id: {purchaseReceipt.transactionID}");

                                switch (purchaseReceipt)
                                {
                                    case GooglePlayReceipt google:
                                        sb.AppendLine($"purchase state: {google.purchaseState}");
                                        sb.AppendLine($"purchase token: {google.purchaseToken}");
                                        break;

                                    case AppleInAppPurchaseReceipt apple:
                                        sb.AppendLine($"original transaction identifier: {apple.originalTransactionIdentifier}");
                                        sb.AppendLine($"subscription expiration date: {apple.subscriptionExpirationDate}");
                                        sb.AppendLine($"cancellation date: {apple.cancellationDate}");
                                        sb.AppendLine($"quantity: {apple.quantity}");
                                        break;
                                }
                            }

                            Debug.Log(sb);

                            // TODO: improved local receipt validation 
                            // For improved security, consider comparing the signed 
                            // IPurchaseReceipt.productId, IPurchaseReceipt.transactionID, and other data 
                            // embedded in the signed receipt objects to the data which the game is using 
                            // to make this purchase. 
                        }
                    }
                    catch (IAPSecurityException ex)
                    {
                        isValid = false;

                        CompletePendingPurchase(currentPurchaseEventArgs.purchasedProduct.definition.storeSpecificId);

                        TransactionManager.PlatformPurchaseFailure(
                            currentPurchaseEventArgs.purchasedProduct.definition.storeSpecificId, ex.Message);
                    }
                }
            }

            if (isValid)
            {
                var deferred = TransactionManager.FinalizeSuccessfulIAP(
                    currentPurchaseEventArgs.purchasedProduct.definition.storeSpecificId);

                while (!deferred.isDone || TransactionManager.currentIap != null)
                {
                    yield return null;
                }

                if (!deferred.isFulfilled)
                {
                    if (Debug.isDebugBuild)
                    {
                        Debug.LogError(deferred.error);
                    }

                    // pass the async error on to the caller

                    completer.Reject(deferred.error);
                    deferred.Release();
                    currentPurchaseEventArgs = null;
                    yield break;
                }

                // pass the async result on to the caller

                completer.Resolve(deferred.result);

                deferred.Release();
            }

            currentPurchaseEventArgs = null;
        }

        /// <inheritdoc cref="IStoreListener.OnPurchaseFailed" /> 
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // TODO: we could have purchasing adapter error codes 
            // for now, we'll just compile a big long string 

            string message = $"Platform purchase for product id '{product.definition.storeSpecificId}' ...\n" +
                $"failed with reason code: {failureReason}\n" +
                $"The platform returned code: {m_TransactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode()}";

            if (m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
            {
                message += "\nPurchase failure description message: " +
                    $"{m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription().message}";
            }

            TransactionManager.PlatformPurchaseFailure(product.definition.storeSpecificId, message);
        }

        /// <summary>
        ///     Called when the player initiates a real money in-app purchase.
        /// </summary>
        /// <param name="productId">
        ///     The platform-specific product ID to send to the purchasing platform.
        /// </param>
        public void BeginPurchase(string productId)
        {
            ThrowIfNotInitialized();

            m_Controller.InitiatePurchase(m_Controller.products.WithStoreSpecificID(productId));
        }

        /// <summary>
        ///     Tells the store platform that the purchase was successful.
        ///     Usually called after a purchase has been verified and fulfilled
        ///     by some other authority, such as an online backend.
        /// </summary>
        public void CompletePendingPurchase(string productId)
        {
            ThrowIfNotInitialized();

            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogError("Tried to confirm a purchase with an empty product id.");
                return;
            }

            var product = m_Controller.products.WithStoreSpecificID(productId);

            if (product == null)
            {
                Debug.LogError($"Tried to confirm a purchase for product id '{productId}', " +
                    "but that product was not found in the platform product set.");
                return;
            }

            m_Controller.ConfirmPendingPurchase(product);
        }

        /// <summary>
        ///     Called when the player wants to restore purchases
        ///     (for platforms that don't restore them automatically).
        /// </summary>
        public void RestorePurchases()
        {
            Debug.Log("Attempting to restoring purchases...");

            if (isAppleIOS)
            {
                m_AppleExtensions.RestoreTransactions(OnTransactionsRestored);
            }

            // TODO: support "Restore Purchases" on platforms other than iOS
            // else
            // {
            //     m_GooglePlayStoreExtensions.RestoreTransactions(OnTransactionsRestored);
            // }
        }

        void OnTransactionsRestored(bool success)
        {
            if (Debug.isDebugBuild) Debug.Log("Transactions restored: " + success);
        }

        /// <summary> 
        ///     Use the Unity IAP SDK to get the localized price from the Apple or Google stores for the user's current locale. 
        /// </summary> 
        /// <param name="productId">
        ///     The product ID to look for (must exist in the IAP Catalog and in the platform store).
        /// </param> 
        /// <returns>
        ///     A struct containing localized product name and price strings.
        /// </returns> 
        /// <exception cref="System.ArgumentException">
        ///     Throws an exception if the product id passed in is invalid.
        /// </exception> 
        /// <exception cref="System.Exception">
        ///     Throws an exception if the controller is null.
        /// </exception> 
        public LocalizedProductMetadata GetLocalizedProductInfo(string productId)
        {
            ThrowIfNotInitialized();

            var product = m_Controller.products.WithStoreSpecificID(productId);

            if (product == null)
            {
                throw new ArgumentException($"Could not find product with id {productId}.");
            }

            if (product.metadata == null)
            {
                throw new ArgumentException("Product object is missing metadata.");
            }

            return new LocalizedProductMetadata
            {
                name = product.metadata.localizedTitle,
                price = product.metadata.localizedPriceString
            };
        }

        /// <summary>
        ///     Get the IAP product info from the initialized Unity Purchasing instance.
        /// </summary>
        /// <param name="productId">
        ///     The product ID to search for.
        /// </param>
        /// <returns>
        ///     A <see cref="Product"/> from the initialized Unity Purchasing instance.
        /// </returns>
        public Product FindProduct(string productId)
        {
            ThrowIfNotInitialized();

            return string.IsNullOrEmpty(productId) ? null : m_Controller.products.WithStoreSpecificID(productId);
        }
    }
}
#endif
