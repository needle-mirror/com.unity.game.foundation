#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Purchasing;
#endif

namespace UnityEngine.GameFoundation.PurchasingAdapters
{

#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION

    /// <summary>
    /// This class adapts the Unity Purchasing SDK (or IAP SDK) to Game Foundation.
    /// To use it, pass an instance of this class in as an arg when initializing the TransactionManager.
    /// The TransactionManager will then call methods on this class instance automatically
    /// to do things like initiate a real money purchase on the adapted platform.
    /// This class instance will also make calls back to the TransactionManager
    /// to do things like update the status of a purchase in progress.
    /// </summary>
    public sealed class UnityPurchasingAdapter : IStoreListener, IPurchasingAdapter
    {
        /// <inheritdoc cref="IPurchasingAdapter.isAppleIOS" />
        public bool isAppleIOS { get; private set; }

        /// <inheritdoc cref="IPurchasingAdapter.isGooglePlay" />
        public bool isGooglePlay { get; private set; }

        /// <inheritdoc cref="IPurchasingAdapter.isFakeStore" />
        public bool isFakeStore { get; private set; }

        /// <summary>
        /// Returns true if this object is ready to use.
        /// </summary>
        public bool isInitialized { get { return m_IsInitialized; } }
        private bool m_IsInitialized;

        IStoreController m_Controller;
        System.Action m_OnInitializeSucceededCallback;
        System.Action<System.Exception> m_OnInitializeFailedCallback;

        IAppleExtensions m_AppleExtensions;
        IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
        ITransactionHistoryExtensions m_TransactionHistoryExtensions;

        PurchaseEventArgs m_CurrentPurchaseEventArgs;
        Queue<PurchaseEventArgs> m_SuccessfulPurchaseQueue;
        Coroutine m_SuccessfulPurchaseQueueCoroutine;

        public SuccessfulPurchaseData GetCurrentPurchaseData()
        {
            ThrowIfNotInitialized();

            if (m_CurrentPurchaseEventArgs == null) return default;

            var successfulPurchaseData = new SuccessfulPurchaseData
            {
                productId = m_CurrentPurchaseEventArgs.purchasedProduct.definition.id
            };

            Debug.Log($"receipt: {m_CurrentPurchaseEventArgs.purchasedProduct.receipt}");

            if (!(UnityEngine.Purchasing.MiniJson.JsonDecode(
                m_CurrentPurchaseEventArgs.purchasedProduct.receipt) is Dictionary<string, object> receiptDict))
            {
                // this will be malformed and shouldn't successfully validate
                return successfulPurchaseData;
            }

            if (isAppleIOS)
            {
                successfulPurchaseData.receiptParts = new[]
                {
                    receiptDict.TryGetValue("payload", out var payloadObj) ? payloadObj.ToString() : ""
                };
            }
            else if (isGooglePlay)
            {
                if (UnityEngine.Purchasing.MiniJson.JsonDecode(
                    receiptDict["Payload"].ToString()) is Dictionary<string, object> payloadDict)
                {
                    successfulPurchaseData.receiptParts = new[]
                    {
                        receiptDict.TryGetValue("json", out var jsonObj) ? jsonObj.ToString() : "",
                        receiptDict.TryGetValue("signature", out var sigObj) ? sigObj.ToString() : ""
                    };
                }
            }

            return successfulPurchaseData;
        }

        /// <inheritdoc cref="IPurchasingAdapter.Initialize"/>
        public void Initialize(
            System.Action onInitializeSucceededCallback = null,
            System.Action<System.Exception> onInitializeFailedCallback = null)
        {
            Initialize(FakeStoreUIMode.StandardUser, onInitializeSucceededCallback, onInitializeFailedCallback);
        }

        /// <summary>
        /// Configures the Unity Purchasing SDK with data from the IAP Catalog.
        /// </summary>
        /// <param name="onInitializeSucceededCallback">A method to call when initialization fails.</param>
        /// <param name="onInitializeFailedCallback">A method to call when initialization is successful.</param>
        /// <param name="uiMode">Use in conjunction with StandardPurchasingModule.useFakeStoreAlways to test different scenarios.</param>
        private void Initialize(
            FakeStoreUIMode uiMode,
            System.Action onInitializeSucceededCallback = null,
            System.Action<System.Exception> onInitializeFailedCallback = null)
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

            var module = StandardPurchasingModule.Instance();

            module.useFakeStoreUIMode =  uiMode;

#if UNITY_EDITOR
            module.useFakeStoreAlways = true;
            isFakeStore = true;
#endif

            var builder = ConfigurationBuilder.Instance(module);

            isGooglePlay = Application.platform == RuntimePlatform.Android
                           && module.appStore == AppStore.GooglePlay;

            isAppleIOS = Application.platform == RuntimePlatform.IPhonePlayer
                         && module.appStore == AppStore.AppleAppStore;

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
        /// Stops the loop that continue to process new successful purchase responses from the platform.
        /// </summary>
        public void Uninitialize()
        {
            if (m_SuccessfulPurchaseQueueCoroutine != null)
            {
                GameFoundation.updater.StopCoroutine(m_SuccessfulPurchaseQueueCoroutine);
            }

            m_SuccessfulPurchaseQueueCoroutine = null;
        }

        private void ThrowIfNotInitialized()
        {
            if (!m_IsInitialized)
            {
                throw new System.InvalidOperationException(
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
            var exception = new System.Exception(
                "Unity Purchasing failed to initialize, for the following reason: " +
                "An unrecognized problem occurred!");

            switch (initializationFailureReason)
            {
                case InitializationFailureReason.AppNotKnown:

                    exception = new System.Exception(
                        "Unity Purchasing failed to initialize, for the following reason: " +
                        "Unknown app. Make sure your app was uploaded to the respective platform store.");

                    break;

                case InitializationFailureReason.PurchasingUnavailable:

                    exception = new System.Exception(
                        "Unity Purchasing failed to initialize, for the following reason: " +
                        "Purchasing is not enabled on this platform.");

                    break;

                case InitializationFailureReason.NoProductsAvailable:

                    exception = new System.Exception(
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
            Debug.Log("received purchaseEventArgs from IAP SDK");

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
                if (m_CurrentPurchaseEventArgs != null) continue;
                if (m_SuccessfulPurchaseQueue.Count <= 0) continue;

                var nextProductId = m_SuccessfulPurchaseQueue.Peek().purchasedProduct.definition.storeSpecificId;

                // if the transaction manager is busy,
                // but it's not with the next product in the queue
                // then wait for the transaction manager to finish
                while (TransactionManager.pendingIapTransaction != null
                    && TransactionManager.pendingIapTransaction.productId != nextProductId)
                {
                    yield return null;
                }

                m_CurrentPurchaseEventArgs = m_SuccessfulPurchaseQueue.Dequeue();

                var deferred = TransactionManager.FinalizeSuccessfulIAP(
                    m_CurrentPurchaseEventArgs.purchasedProduct.definition.storeSpecificId);

                // wait for the deferred to finish before moving on to the next in the queue

                while (!deferred.isDone || TransactionManager.pendingIapTransaction != null)
                {
                    yield return null;
                }

                if (!deferred.isFulfilled)
                {
                    Debug.LogError(deferred.error);
                }

                // let's be silent about it here, whether it succeeds or fails.
                // if TransactionManager was waiting for it, then it will notify the game

                m_CurrentPurchaseEventArgs = null;

                // TODO: PurchaseEventArgs that are in the queue for too long (minutes?, hours?, days?) should just be [deleted? confirmed?]
            }
        }

        /// <inheritdoc cref="IStoreListener.OnPurchaseFailed" />
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // TODO: we could have purchasing adapter error codes
            // for now, we'll just compile a big long string

            string message = $"Platform purchase for product id '{product.definition.id}' ...\n" +
                             $"failed with reason code: {failureReason}\n" +
                             $"The platform returned code: {m_TransactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode()}";

            if (m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
            {
                message += "\nPurchase failure description message: " +
                           $"{m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription().message}";
            }

            TransactionManager.PlatformPurchaseFailure(product.definition.storeSpecificId, message);
        }

        /// <inheritdoc cref="IPurchasingAdapter.BeginPurchase"/>
        public void BeginPurchase(string productId, string customPayload = "")
        {
            ThrowIfNotInitialized();

            m_Controller.InitiatePurchase(m_Controller.products.WithStoreSpecificID(productId), customPayload);
        }

        /// <inheritdoc cref="IPurchasingAdapter.CompletePendingPurchase"/>
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
                               $"but that product was not found in the platform product set.");
                return;
            }

            m_Controller.ConfirmPendingPurchase(product);
        }

        /// <inheritdoc cref="IPurchasingAdapter.RestorePurchases"/>
        public void RestorePurchases()
        {
            if (isGooglePlay)
            {
                m_GooglePlayStoreExtensions.RestoreTransactions(OnTransactionsRestored);
            }
            else
            {
                m_AppleExtensions.RestoreTransactions(OnTransactionsRestored);
            }
        }

        private void OnTransactionsRestored(bool success)
        {
            // TODO: something better?
            Debug.Log("Transactions restored: " + success);

            // TODO: tell TransactionManager ?
        }

        /// <summary>
        /// Use the Unity IAP SDK to get the localized price from the Apple or Google stores for the user's current locale.
        /// </summary>
        /// <param name="productId">The product ID to look for (must exist in the IAP Catalog and in the platform store).</param>
        /// <returns>A struct containing localized product name and price strings.</returns>
        /// <exception cref="System.ArgumentException">Throws an exception if the product id passed in is invalid.</exception>
        /// <exception cref="System.Exception">Throws an exception if the controller is null.</exception>
        public LocalizedProductMetadata GetLocalizedProductInfo(string productId)
        {
            ThrowIfNotInitialized();

            var product = m_Controller.products.WithStoreSpecificID(productId);

            if (product == null)
            {
                throw new System.ArgumentException($"Could not find product with id {productId}.");
            }

            if (product.metadata == null)
            {
                throw new System.ArgumentException($"Product object is missing metadata.");
            }

            return new LocalizedProductMetadata()
            {
                name = product.metadata.localizedTitle,
                price = product.metadata.localizedPriceString
            };
        }
    }

#else

    public class UnityPurchasingAdapter : IPurchasingAdapter
    {
        const string k_purchasingNotEnabledMessage =
            "In-App Purchasing is not enabled. " +
            "Make sure that In-App Purchasing is enabled in the Services window, " +
            "that the Unity Purchasing SDK has been imported into the project, " +
            "and that the Purchasing option is enabled in Game Foundation Runtime Settings.";

        public bool isAppleIOS => throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        public bool isGooglePlay => throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        public bool isFakeStore => throw new System.NotImplementedException(k_purchasingNotEnabledMessage);

        public void BeginPurchase(string productId, string customPayload = "")
        {
            throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        }

        public SuccessfulPurchaseData GetCurrentPurchaseData()
        {
            throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        }

        public void CompletePendingPurchase(string productId)
        {
            throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        }

        public void RestorePurchases()
        {
            throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        }

        public void Initialize(System.Action onInitializeSucceededCallback = null,
            System.Action<System.Exception> onInitializeFailedCallback = null)
        {
            throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        }

        public void Uninitialize()
        {
            throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        }

        public LocalizedProductMetadata GetLocalizedProductInfo(string productId)
        {
            throw new System.NotImplementedException(k_purchasingNotEnabledMessage);
        }
    }

#endif

}
