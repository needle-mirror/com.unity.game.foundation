#if UNITY_EDITOR

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class CurrencyAsset
    {
        /// <inheritdoc/>
        internal override string Editor_AssetPrefix => "Currency";

        /// <summary>
        /// Removes the deleted currency from the catalog.
        /// </summary>
        protected override void OnItemDestroy()
        {
            if (catalog is null) return;

            //Debug.Log($"Removing {displayName} ({GetType().Name}) from {catalog}");
            (catalog as CurrencyCatalogAsset).Editor_RemoveItem(this);
        }

        /// <summary>
        /// Sets the <see cref="initialBalance"/> of this <see cref="CurrencyAsset"/> instance.
        /// </summary>
        /// <param name="balance">The initial balance.</param>
        internal void Editor_SetInitialBalance(long balance)
        {
            m_InitialBalance = balance;
        }

        /// <summary>
        /// Sets the <see cref="maximumBalance"/> of this <see cref="CurrencyAsset"/> instance.
        /// </summary>
        /// <param name="balance">The maximum balance.</param>
        internal void Editor_SetMaximumBalance(long balance)
        {
            m_MaximumBalance = balance;
        }

        /// <summary>
        /// Sets the <see cref="type"/> of this <see cref="CurrencyAsset"/> instance.
        /// </summary>
        /// <param name="type">The type of this <see cref="CurrencyAsset"/> instance.</param>
        internal void Editor_SetType(CurrencyType type)
        {
            m_Type = type;
        }
    }
}

#endif
