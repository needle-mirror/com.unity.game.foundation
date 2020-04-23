namespace UnityEngine.GameFoundation.PurchasingAdapters
{
    /// <summary>
    /// Contains portable information about a successful IAP purchase.
    /// </summary>
    public struct SuccessfulPurchaseData
    {
        public string productId;
        public string[] receiptParts;
    }

    /// <summary>
    /// Contains portable localized information about an IAP product.
    /// </summary>
    public struct LocalizedProductMetadata
    {
        public string name;
        public string price;
    }

    /// <summary>
    ///     Implement this interface when you want to adapt a
    ///     real money IAP library with Game Foundation's Transaction system.
    ///     When the Game Foundation Transaction Manager is initialized with one of these adapters,
    ///     the Transaction Manager will use this adapter to process all IAPTransactionDefinitions.
    /// </summary>
    public interface IPurchasingAdapter
    {
        /// <summary>
        ///     Are we currently running on the Apple iOS platform?
        /// </summary>
        bool isAppleIOS { get; }

        /// <summary>
        ///     Are we currently running on the Google Play platform?
        /// </summary>
        bool isGooglePlay { get; }

        /// <summary>
        ///     Are we currently running on a fake platform just for testing?
        /// </summary>
        bool isFakeStore { get; }

        /// <summary>
        ///     Called to set up anything the platform SDK needs in order to process real money purchase requests.
        /// </summary>
        /// <param name="onInitializeSucceededCallback">A method to call when initialization fails.</param>
        /// <param name="onInitializeFailedCallback">A method to call when initialization is successful.</param>
        void Initialize(
            System.Action onInitializeSucceededCallback,
            System.Action<System.Exception> onInitializeFailedCallback);

        /// <summary>
        /// The TransactionManager will call this method when it is uninitialized, when Game Foundation is uninitialized.
        /// </summary>
        void Uninitialize();

        /// <summary>
        ///     Called when the player initiates a real money in-app purchase.
        /// </summary>
        /// <param name="productId">
        ///     The platform-specific product ID to send to the purchasing platform.
        /// </param>
        /// <param name="customPayload">
        ///     An optional arbitrary string to send to the purchasing platform,
        ///     with the expectation of getting the same string back
        ///     eventually (if the purchasing platform supports that).
        /// </param>
        void BeginPurchase(string productId, string customPayload = "");

        /// <summary>
        ///     Return a struct that contains just the data about a successful purchase
        ///     which the TransactionManager can use to fulfill and/or finalize a purchase.
        /// </summary>
        /// <returns>
        ///     A SuccessfulPurchaseData populated with a product id and as many receipt strings as that platform requires for receipt validation.
        /// </returns>
        SuccessfulPurchaseData GetCurrentPurchaseData();

        /// <summary>
        ///     Called after a purchase has been verified and fulfilled by some other authority, such as an online backend.
        /// </summary>
        void CompletePendingPurchase(string productId);

        /// <summary>
        ///     Called when the player wants to restore purchases (for platforms that don't restore them automatically).
        /// </summary>
        void RestorePurchases();

        /// <summary>
        ///     Use the purchasing platform to get localized product name, price, and other metadata.
        /// </summary>
        /// <param name="productId">The IAP product ID for which you want localized info.</param>
        /// <returns>A struct containing localized name and price strings.</returns>
        LocalizedProductMetadata GetLocalizedProductInfo(string productId);
    }
}
