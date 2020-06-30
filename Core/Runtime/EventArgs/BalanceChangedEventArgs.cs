namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Data sent when the balance of a currency changed.
    /// </summary>
    public struct BalanceChangedEventArgs
    {
        /// <summary>
        /// The currency that changed balance.
        /// </summary>
        public Currency currency { get; }

        /// <summary>
        /// The currency's balance before the change.
        /// </summary>
        public long oldBalance { get; }

        /// <summary>
        /// The currency's balance after the change.
        /// </summary>
        public long newBalance { get; }

        /// <param name="currency">
        /// The currency that changed balance.
        /// </param>
        /// <param name="oldBalance">
        /// The currency's balance before the change.
        /// </param>
        /// <param name="newBalance">
        /// The currency's balance after the change.
        /// </param>
        public BalanceChangedEventArgs(Currency currency, long oldBalance, long newBalance)
        {
            this.currency = currency;
            this.oldBalance = oldBalance;
            this.newBalance = newBalance;
        }
    }
}
