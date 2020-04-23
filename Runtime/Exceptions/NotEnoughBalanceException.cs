using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    /// Thrown during a process manipulating the wallet when the player tries to
    /// spend more than he has.
    /// </summary>
    public class NotEnoughBalanceException : Exception
    {
        /// <summary>
        /// The id of the currency.
        /// </summary>
        public readonly string currencyId;

        /// <summary>
        /// The expected balance.
        /// </summary>
        public readonly long expectedBalance;

        /// <summary>
        /// The actual balance.
        /// </summary>
        public readonly long actualBalance;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="NotEnoughBalanceException"/> class.
        /// </summary>
        /// <param name="currencyId">The identifier of the <see cref="Currency"/></param>
        /// <param name="expectedBalance">The necessary balance.</param>
        /// <param name="actualBalance">The available balance.</param>
        internal NotEnoughBalanceException
            (string currencyId, long expectedBalance, long actualBalance)
        {
            this.currencyId = currencyId;
            this.expectedBalance = expectedBalance;
            this.actualBalance = actualBalance;
        }

        /// <inheritdoc />
        public override string Message
            => $"Not enough balance for {currencyId}. Expected: {expectedBalance}, found: {actualBalance}";
    }
}
