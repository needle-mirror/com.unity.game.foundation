namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes the IAP transaction info, and outcome.
    /// </summary>
    public sealed class IAPTransaction : BaseTransaction
    {
        /// <summary>
        /// The product identifier defined in the platform store.
        /// </summary>
        public string productId { get; internal set; }
    }
}
