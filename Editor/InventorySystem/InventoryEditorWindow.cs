using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Class creates Inventory system-specific editor window.
    /// </summary>
    internal class InventoryEditorWindow : CollectionEditorWindowBase
    {

        private InventoryCatalog m_SelectedAssetToLoad;
        private InventoryCatalog m_NewSelectedAssetToLoad;

        private static InventoryEditorWindow s_Window;

        private static List<ICollectionEditor> m_InventoryEditors = new List<ICollectionEditor>();

        protected override List<ICollectionEditor> m_Editors
        {
            get { return m_InventoryEditors; }
        }

        /// <summary>
        /// Basic constructor, sets the collection type name to inventory.
        /// </summary>
        public InventoryEditorWindow()
        {
            m_CollectionTypeName = "Inventory";
        }

        /// <summary>
        /// Opens the Inventories window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            s_Window = GetWindow<InventoryEditorWindow>(false, "Inventory", true);
        }

        /// <summary>
        /// Adds the editors for the inventory system as tabs in the window.
        /// </summary>
        public override void CreateEditors()
        {
            m_InventoryEditors.Clear();

            m_InventoryEditors.Add(new InventoryItemDefinitionEditor("Inventory Items", this));
            m_InventoryEditors.Add(new InventoryDefinitionEditor("Inventories", this));
            m_InventoryEditors.Add(new CategoryDefinitionEditor("Categories", this));
        }

        protected bool IsCatalogSet()
        {
            return GameFoundationSettings.inventoryCatalog != null;
        }

        protected override void SetUpNecessaryCatalogs()
        {
            base.SetUpNecessaryCatalogs();

            if (!IsCatalogSet())
            {
                CreateInventoryCatalog();
            }
        }

        private void CreateInventoryCatalog()
        {
            string createDatabaseFileName = "InventoryCatalog";
            string inventoryCatalogAssetPath = string.Format("Assets/{0}/Resources/{1}.asset", GameFoundationSettings.kAssetsFolder, createDatabaseFileName);

            SetUpResourcesFolder();

            if (File.Exists(Path.Combine(Application.dataPath, inventoryCatalogAssetPath.Substring(7))))
            {
                inventoryCatalogAssetPath = CollectionEditorTools.CreateUniqueCatalogPath(inventoryCatalogAssetPath);
            }

            InventoryCatalog inventoryCatalog = InventoryCatalog.Create();
            AssetDatabase.CreateAsset(inventoryCatalog, inventoryCatalogAssetPath);
            CollectionEditorTools.AssetDatabaseUpdate();

            GameFoundationSettings.inventoryCatalog = inventoryCatalog;
            Debug.LogWarningFormat("Creating new asset file at path {0} and connecting it to inventoryCatalog in GameFoundationSettings asset.", inventoryCatalogAssetPath);
            ResetEditors();
        }
    }
}
