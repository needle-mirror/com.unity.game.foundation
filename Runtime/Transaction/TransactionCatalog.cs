using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contains all the available transactions.
    /// </summary>
    public sealed class TransactionCatalog : SingleCollectionCatalog<BaseTransaction>
    {
        /// <summary>
        /// Get a <see cref="BaseTransaction"/> from its id.
        /// </summary>
        /// <param name="transactionKey">The key of the transaction
        /// definition to get.</param>
        /// <returns>If found, returns the <see cref="BaseTransaction"/>
        /// object, otherwise null</returns>
        public TTransaction FindTransaction<TTransaction>(string transactionKey)
            where TTransaction : BaseTransaction
        {
            Tools.ThrowIfArgNull(transactionKey, nameof(transactionKey));
            return (TTransaction)FindItem(transactionKey);
        }

        /// <summary>
        /// Get a <see cref="BaseTransaction"/> from its id.
        /// </summary>
        /// <param name="productId">The product id of the IAP transaction definition to find.</param>
        /// <returns>If found, returns the <see cref="IAPTransaction"/> object, otherwise null</returns>
        public IAPTransaction FindIAPTransactionByProductId(string productId)
        {
            Tools.ThrowIfArgNull(productId, nameof(productId));

            List<IAPTransaction> allIAPTransactions = new List<IAPTransaction>();

            GetTransactions(allIAPTransactions);

            foreach (var iapTransaction in allIAPTransactions)
            {
                if (iapTransaction.productId == productId)
                {
                    return iapTransaction;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the transactions of type <typeparamref name="TTransaction"/>.
        /// </summary>
        /// <typeparam name="TTransaction">The transaction type to use as a
        /// filter.</typeparam>
        /// <param name="target">The list contains the transaction found in the
        /// catalog.</param>
        /// <returns>The number of items copied</returns>
        public int GetTransactions<TTransaction>
            (ICollection<TTransaction> target = null)
            where TTransaction : BaseTransaction
        {
            if (target != null) target.Clear();

            var count = 0;

            foreach (var transaction in m_Items.Values)
            {
                if (transaction is TTransaction typedTransaction)
                {
                    count++;
                    if (target != null) target.Add(typedTransaction);
                }
            }

            return count;
        }
    }
}
