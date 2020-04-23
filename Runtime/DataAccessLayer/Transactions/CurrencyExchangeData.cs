namespace UnityEngine.GameFoundation.DataAccessLayers
{
    /// <summary>
    /// Description of a currency update.
    /// </summary>
    public struct CurrencyExchangeData
    {
        /// <summary>
        /// The identifier of the updated currency.
        /// </summary>
        public string currencyId;

        /// <summary>
        /// The amount.
        /// </summary>
        public long amount;
    }
}
