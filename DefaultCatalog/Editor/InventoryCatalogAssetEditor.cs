using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class InventoryCatalogAssetEditor : BaseCatalogAssetEditor<InventoryItemDefinitionAsset>
    {
        static readonly GUIContent k_PropertiesLabel = new GUIContent(
            "Properties",
            "Store variable data inside your items you can manipulate at runtime.");

        /// <summary>
        /// Label for Initial Allocation entry for this <see cref="InventoryItemDefinitionAsset"/>.
        /// </summary>
        static readonly GUIContent k_InitialAllocationLabel = new GUIContent(
            "Initial allocation", 
            "The quantity of this item to autmatically add to player's inventory at startup.");

        /// <summary>
        /// Quantity at which warning should be added to the Editor regarding slow startup times.
        /// </summary>
        const int k_InitialAllocationWarningSize = 1000;

        /// <summary>
        /// Quantity-large warning.  Added when Initial Allocation is set larger than 
        /// <see cref="k_InitialAllocationWarningSize"/>.
        /// </summary>
        static readonly string k_InitialAllocationLargeLabel = 
            "Large initial allocations may adversely effect performance and memory consumption.\n" + 
            "Current implementation of Game Foundation requires that each item be added iteratively so startup time will " +
            "be extended due to large Initial Allocation quantities.  For now, please consider using Currency instead, " +
            "if possible or redesign your game economy to require less Inventory Items at startup.";

        MutablePropertiesEditor m_MutablePropertiesEditor = new MutablePropertiesEditor();

        protected override BaseCatalogAsset assetCatalog
            => GameFoundationDatabaseSettings.database.inventoryCatalog;

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.InventoryItems;

        public InventoryCatalogAssetEditor(string name)
            : base(name) { }

        protected override void SelectItem(InventoryItemDefinitionAsset item)
        {
            base.SelectItem(item);

            m_MutablePropertiesEditor.SelectItem(item);
        }

        protected override void DrawTypeSpecificBlocks(InventoryItemDefinitionAsset catalogItem)
        {
            //Draw properties.
            EditorGUILayout.LabelField(MutablePropertiesEditor.mutablePropertiesLabel, GameFoundationEditorStyles.titleStyle);
            using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                m_MutablePropertiesEditor.Draw();
            }

            EditorGUILayout.Space();
        }

        protected override void FillCatalogItems()
        {
            var catalog = GameFoundationDatabaseSettings.database.inventoryCatalog;
            if (catalog != null)
                catalog.GetItems(m_Items);
        }

        protected override void AddCatalogItem(InventoryItemDefinitionAsset catalogItem)
            => GameFoundationDatabaseSettings.database.inventoryCatalog.Editor_AddItem(catalogItem);

       protected override void DrawGeneralDetail(InventoryItemDefinitionAsset inventoryItemDefinition)
        {
            if (GameFoundationDatabaseSettings.database == null
                || GameFoundationDatabaseSettings.database.inventoryCatalog == null)
            {
                return;
            }

            EditorGUILayout.LabelField("General", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                var displayName = inventoryItemDefinition.displayName;
                m_ReadableNameKeyEditor.DrawReadableNameKeyFields(ref m_CurrentItemKey, ref displayName);

                if (inventoryItemDefinition.displayName != displayName)
                {
                    inventoryItemDefinition.Editor_SetDisplayName(displayName);
                    EditorUtility.SetDirty(inventoryItemDefinition);
                }

                // allow dev to set initial allocation
                EditorGUILayout.Space();
                GUI.SetNextControlName("Initial allocation");
                var initialAllocation = EditorGUILayout.IntField(k_InitialAllocationLabel, 
                    inventoryItemDefinition.initialAllocation);
                if (GUI.changed)
                {
                    // negative allocation not permitted.
                    inventoryItemDefinition.initialAllocation = initialAllocation >= 0 
                        ? initialAllocation : 0;
                }

                // add warning
                // TODO: this should be removed once non-iterative, stacked inventory items can be added
                if (inventoryItemDefinition.initialAllocation >= k_InitialAllocationWarningSize)
                {
                    EditorGUILayout.HelpBox(k_InitialAllocationLargeLabel, MessageType.Warning);
                }
            }
        }
    }
}
