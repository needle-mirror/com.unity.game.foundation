using UnityEngine.GameFoundation.DataAccessLayers;

namespace UnityEngine.GameFoundation
{
    public struct TransactionResult
    {
        /// <summary>
        /// The items or currency which the transaction took from the player.
        /// </summary>
        public readonly TransactionCosts costs;

        /// <summary>
        /// The items or currency which the transaction granted to the player.
        /// </summary>
        public readonly TransactionRewards rewards;

        internal TransactionResult(TransactionCosts costs, TransactionRewards rewards)
        {
            this.costs = costs;
            this.rewards = rewards;
        }
    }
}
