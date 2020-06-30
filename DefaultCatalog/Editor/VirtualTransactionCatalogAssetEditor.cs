using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class VirtualTransactionCatalogAssetEditor : BaseTransactionCatalogAssetEditor<VirtualTransactionCatalogAssetEditor, VirtualTransactionAsset>
    {
        public VirtualTransactionCatalogAssetEditor(string name)
            : base(name) { }

        protected override void DrawTypeSpecificBlocks(VirtualTransactionAsset transaction)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    GUI.SetNextControlName("Costs");
                    DrawExchangeDefinition(transaction.m_Costs, "Costs");
                }

                DrawRewards(transaction);
            }

            EditorGUILayout.Space();
        }

        protected override void FillCatalogItems()
        {
            var catalog = GameFoundationDatabaseSettings.database.transactionCatalog;
            if (catalog != null)
                catalog.GetItems(m_Items);
        }

        protected override void AddCatalogItem(VirtualTransactionAsset catalogItem)
            => GameFoundationDatabaseSettings.database.transactionCatalog.Editor_AddItem(catalogItem);
    }
}
