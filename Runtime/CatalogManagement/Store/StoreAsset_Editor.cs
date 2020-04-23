#if UNITY_EDITOR

using System;
using UnityEditor;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class StoreAsset
    {
        /// <inheritdoc/>
        internal override string Editor_AssetPrefix => "Store";

        /// <summary>
        /// Adds a <paramref name="transaction"/> to the store.
        /// </summary>
        /// <param name="transaction">The <see cref="BaseTransactionAsset"/> to
        /// add.</param>
        internal void Editor_AddItem(BaseTransactionAsset transaction)
        {
            GFTools.ThrowIfArgNull(transaction, nameof(transaction));

            if (Contains(transaction))
            {
                throw new Exception
                    ("This transaction already exists in this store");
            }

            var storeItem = new StoreItemObject();
            storeItem.store = this;
            storeItem.m_Transaction = transaction;
            storeItem.m_Enabled = true;

            m_StoreItems.Add(storeItem);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Removes a <paramref name="storeItem"/> from the store.
        /// </summary>
        /// <param name="storeItem">the <see cref="StoreItemObject"/> to
        /// remove.</param>
        /// <returns><c>true</c> if removed, <c>false</c> otherwise.</returns>
        internal bool Editor_RemoveItem(StoreItemObject storeItem)
        {
            GFTools.ThrowIfArgNull(storeItem, nameof(storeItem));
            var removed = m_StoreItems.Remove(storeItem);

            if (removed)
            {
                EditorUtility.SetDirty(this);
            }

            return removed;
        }

        /// <summary>
        /// Swaps <paramref name="storeItem1"/> with <paramref name="storeItem2"/>
        /// in the store.
        /// </summary>
        /// <param name="storeItem1">The <see cref="StoreItemObject"/> to swap.</param>
        /// <param name="storeItem2">The <see cref="StoreItemObject"/> to swap
        /// the first with.</param>
        internal void Editor_SwapItemsListOrder
            (StoreItemObject storeItem1, StoreItemObject storeItem2)
        {
            GFTools.ThrowIfArgNull(storeItem1, nameof(storeItem1));
            GFTools.ThrowIfArgNull(storeItem2, nameof(storeItem2));

            var index1 = m_StoreItems.IndexOf(storeItem1);
            var index2 = m_StoreItems.IndexOf(storeItem2);

            m_StoreItems[index1] = storeItem2;
            m_StoreItems[index2] = storeItem1;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Clean the store by removing the <see cref="StoreItemObject"/> if
        /// they contain the reference to the removed
        /// <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction"></param>
        internal void Editor_HandleTransactionRemoved
            (BaseTransactionAsset transaction)
        {
            for (var i = 0; i < m_StoreItems.Count;)
            {
                var storeItem = m_StoreItems[i];
                if (storeItem.transaction == transaction)
                {
                    //Debug.Log($"{displayName} ({GetType().Name}) has a link to {transaction.displayName} ({transaction.GetType().Name}). Updating…");
                    m_StoreItems.RemoveAt(i);
                    EditorUtility.SetDirty(this);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnItemDestroy()
        {
            if (catalog is null) return;

            Debug.Log($"Removing {displayName} ({GetType().Name}) from {catalog}");
            (catalog as StoreCatalogAsset).Editor_RemoveItem(this);
        }
    }
}

#endif
