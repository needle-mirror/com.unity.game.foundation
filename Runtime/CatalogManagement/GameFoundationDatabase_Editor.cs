#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public partial class GameFoundationDatabase
    {
        const string kBaseFileName = "GameFoundationDatabase";

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

            var databaseName = ObjectNames.GetUniqueName(names, kBaseFileName);

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

            if (m_StatCatalog != null)
            {
                DestroyImmediate(m_StatCatalog, true);
                m_StatCatalog = null;
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
            else
            {
                AssetDatabase.CreateAsset(this, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                save = true;
            }

            var subAssets = new List<Object>();
            Editor_GetSubAssets(subAssets);

            foreach (var subAsset in subAssets)
            {
                if (!AssetDatabase.IsSubAsset(subAsset))
                {
                    AssetDatabase.AddObjectToAsset(subAsset, this);
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
            target.Add(m_CurrencyCatalog);
            m_CurrencyCatalog.Editor_GetSubAssets(target);

            target.Add(m_InventoryCatalog);
            m_InventoryCatalog.Editor_GetSubAssets(target);

            target.Add(m_StatCatalog);
            m_StatCatalog.Editor_GetSubAssets(target);

            target.Add(m_StoreCatalog);
            m_StoreCatalog.Editor_GetSubAssets(target);

            target.Add(m_TransactionCatalog);
            m_TransactionCatalog.Editor_GetSubAssets(target);
        }
    }
}

#endif
