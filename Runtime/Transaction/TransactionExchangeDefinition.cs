using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes the changes of a transaction definition.
    /// As a reward, those changes are added.
    /// As a cost, those changes are removed.
    /// </summary>
    public sealed class TransactionExchangeDefinition
    {
        /// <summary>
        /// The list of item definitions with their quantity.
        /// </summary>
        internal ItemExchangeDefinition[] m_Items;

        /// <summary>
        /// The list of currency with their amount.
        /// </summary>
        internal CurrencyExchangeDefinition[] m_Currencies;

        /// <summary>
        /// Creates a new transaction exchange definition.
        /// </summary>
        internal TransactionExchangeDefinition() {}

        /// <summary>
        /// 
        /// </summary>
        public int ItemExchangeCount => m_Items.Length;

        public int CurrencyExchangeCount => m_Currencies.Length;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int GetItemExchanges(ICollection<ItemExchangeDefinition> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));
            return Tools.Copy(m_Items, target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ItemExchangeDefinition GetItemExchange(int index)
        {
            Tools.ThrowIfOutOfRange(index, 0, m_Items.Length - 1, nameof(index));
            return m_Items[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int GetCurrencyExchanges
            (ICollection<CurrencyExchangeDefinition> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));
            return Tools.Copy(m_Currencies, target);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CurrencyExchangeDefinition GetCurrencyExchange(int index)
        {
            Tools.ThrowIfOutOfRange(index, 0, m_Currencies.Length - 1, nameof(index));
            return m_Currencies[index];
        }
    }
}
