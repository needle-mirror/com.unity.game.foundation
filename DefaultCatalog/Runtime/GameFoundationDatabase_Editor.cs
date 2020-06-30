#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class GameFoundationDatabase
    {
        const string k_BaseFileName = "GameFoundationDatabase";

        [MenuItem("Assets/Create/Game Foundation/Database")]
        static void Editor_Create()
        {
            string path = null;

            var selection = Selection.activeObject;
            if (EditorUtility.IsPersistent(selection))
            {
                path = AssetDatabase.GetAssetPath(selection);

                if (!AssetDatabase.IsValidFolder(path))
                {
                    var selectionName = Path.GetFileName(path);
                    path = path.Substring(0, path.Length - selectionName.Length);
                }
                else
                {
                    path += "/";
                }
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                path = "Assets/";
            }

            var names = new DirectoryInfo(path).GetFiles().Select(f => f.Name.Substring(0, f.Name.Length - f.Extension.Length)).ToArray();

            var databaseName = ObjectNames.GetUniqueName(names, k_BaseFileName);

            path += databaseName + ".asset";

            var database = CreateInstance<GameFoundationDatabase>();
            database.name = databaseName;

            database.Editor_Save(path);

            EditorUtility.FocusProjectWindow();
        }

        /// <summary>
        /// Destroys the catalogs.
        /// Note: needed since the database is not normally ...
        ///       ... destroyed but unit testing requires a cleanup test.
        /// </summary>
        internal void Editor_DestroyCatalogsImmediate()
        {
            if (m_InventoryCatalog != null)
            {
                DestroyImmediate(m_InventoryCatalog, true);
                m_InventoryCatalog = null;
            }

            if (m_StoreCatalog != null)
            {
                DestroyImmediate(m_StoreCatalog, true);
                m_StoreCatalog = null;
            }

            if (m_CurrencyCatalog != null)
            {
                DestroyImmediate(m_CurrencyCatalog, true);
                m_CurrencyCatalog = null;
            }

            if (m_TransactionCatalog != null)
            {
                DestroyImmediate(m_TransactionCatalog, true);
                m_TransactionCatalog = null;
            }
        }

        /// <summary>
        /// Saves the database and all its subassets in the same asset file.
        /// </summary>
        /// <param name="path">The path of the output file.</param>
        internal void Editor_Save(string path = null)
        {
            var oldPath = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(oldPath))
            {
                path = oldPath;
            }

            bool save;

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException
                    (nameof(path), $"{nameof(path)} cannot be null");
            }

            AssetDatabase.CreateAsset(this, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            save = true;

            var subAssets = new List<Object>();
            Editor_GetSubAssets(subAssets);

            foreach (var subAsset in subAssets)
            {
                if (!AssetDatabase.IsSubAsset(subAsset))
                {
                    AssetDatabase.AddObjectToAsset(subAsset, this);
                    if (string.IsNullOrWhiteSpace(subAsset.name))
                    {
                        if (subAsset is CatalogItemAsset catalogItem)
                        {
                            catalogItem.name = catalogItem.Editor_AssetName;
                        }
                        else if (subAsset is BaseDetailAsset detail)
                        {
                            detail.name = detail.Editor_AssetName;
                        }
                        else if (subAsset is TagAsset tag)
                        {
                            tag.name = tag.Editor_AssetName;
                        }
                    }
                    save = true;
                }
            }

            if (save)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Gets all the subassets of the database.
        /// </summary>
        /// <param name="target">The target collection where the subassets are
        /// added.</param>
        internal void Editor_GetSubAssets(ICollection<Object> target)
        {
            target.Add(m_TagCatalog);
            m_TagCatalog.Editor_GetSubAssets(target);

            target.Add(m_CurrencyCatalog);
            m_CurrencyCatalog.Editor_GetSubAssets(target);

            target.Add(m_InventoryCatalog);
            m_InventoryCatalog.Editor_GetSubAssets(target);

            target.Add(m_StoreCatalog);
            m_StoreCatalog.Editor_GetSubAssets(target);

            target.Add(m_TransactionCatalog);
            m_TransactionCatalog.Editor_GetSubAssets(target);

            target.Add(m_GameParameterCatalog);
            m_GameParameterCatalog.Editor_GetSubAssets(target);
        }

        /// <summary>
        /// Creates a new <see cref="InventoryItemDefinitionAsset"/> instance and adds it to the database.
        /// </summary>
        /// <param name="key">The key of the new <see cref="InventoryItemDefinitionAsset"/> instance.</param>
        /// <returns>The new <see cref="InventoryItemDefinitionAsset"/> instance.</returns>
        internal InventoryItemDefinitionAsset Editor_CreateInventoryItem(string key)
        {
            var inventoryItem = CreateInstance<InventoryItemDefinitionAsset>();
            inventoryItem.m_Catalog = m_InventoryCatalog;
            inventoryItem.Editor_SetKey(key);

            m_InventoryCatalog.m_Items.Add(inventoryItem);
            return inventoryItem;
        }

        /// <summary>
        /// Creates a new <see cref="TagAsset"/> instance and adds it to the database.
        /// </summary>
        /// <param name="id">The key of the new <see cref="TagAsset"/> instance.</param>
        /// <returns>The new <see cref="TagAsset"/> instance.</returns>
        internal TagAsset Editor_CreateTag(string id)
        {
            var tag = CreateInstance<TagAsset>();
            tag.Editor_SetId(id);
            m_TagCatalog.Editor_AddTag(tag);
            return tag;
        }

        /// <summary>
        /// Creates a new <see cref="CurrencyAsset"/> instance and adds it to the database.
        /// </summary>
        /// <param name="key">The key of the new <see cref="CurrencyAsset"/> instance.</param>
        /// <returns>The new <see cref="CurrencyAsset"/> instance.</returns>
        internal CurrencyAsset Editor_CreateCurrency(string key)
        {
            var currency = CreateInstance<CurrencyAsset>();
            currency.m_Catalog = m_CurrencyCatalog;
            currency.Editor_SetKey(key);

            m_CurrencyCatalog.Editor_AddItem(currency);
            return currency;
        }

        /// <summary>
        /// Creates a new <see cref="StoreAsset"/> instance and adds it to the database.
        /// </summary>
        /// <param name="key">The key of the new <see cref="StoreAsset"/> instance.</param>
        /// <returns>The new <see cref="StoreAsset"/> instance.</returns>
        internal StoreAsset Editor_CreateStore(string key)
        {
            var store = CreateInstance<StoreAsset>();
            store.m_Catalog = m_StoreCatalog;
            store.Editor_SetKey(key);

            m_StoreCatalog.Editor_AddItem(store);
            return store;
        }

        /// <summary>
        /// Creates a new <see cref="VirtualTransactionAsset"/> instance and adds it to the database.
        /// </summary>
        /// <param name="key">The key of the new <see cref="VirtualTransactionAsset"/> instance.</param>
        /// <returns>The new <see cref="VirtualTransactionAsset"/> instance.</returns>
        internal VirtualTransactionAsset Editor_CreateVirtualTransaction(string key)
        {
            var transaction = CreateInstance<VirtualTransactionAsset>();
            transaction.m_Costs = new TransactionExchangeDefinitionObject();
            transaction.m_Rewards = new TransactionExchangeDefinitionObject();
            transaction.m_Catalog = m_TransactionCatalog;
            transaction.Editor_SetKey(key);

            m_TransactionCatalog.Editor_AddItem(transaction);
            return transaction;
        }

        /// <summary>
        /// Creates a new <see cref="IAPTransactionAsset"/> instance and adds it to the database.
        /// </summary>
        /// <param name="key">The key of the new <see cref="IAPTransactionAsset"/> instance.</param>
        /// <returns>The new <see cref="IAPTransactionAsset"/> instance.</returns>
        internal IAPTransactionAsset Editor_CreateIapTransaction(string key)
        {
            var iap = CreateInstance<IAPTransactionAsset>();
            iap.m_Rewards = new TransactionExchangeDefinitionObject();
            iap.m_Catalog = m_TransactionCatalog;
            iap.Editor_SetKey(key);

            m_TransactionCatalog.Editor_AddItem(iap);
            return iap;
        }

        internal void Editor_OnTagRemoved(TagAsset tag)
        {
            m_InventoryCatalog?.Editor_OnTagRemoved(tag);
            m_CurrencyCatalog?.Editor_OnTagRemoved(tag);
            m_TransactionCatalog?.Editor_OnTagRemoved(tag);
            m_StoreCatalog?.Editor_OnTagRemoved(tag);
        }
    }
}

#endif
