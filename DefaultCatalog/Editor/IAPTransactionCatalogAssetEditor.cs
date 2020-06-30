using UnityEngine;
using UnityEngine.GameFoundation.DefaultCatalog;
using static UnityEditor.EditorGUILayout;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class IAPTransactionCatalogAssetEditor : BaseTransactionCatalogAssetEditor<IAPTransactionCatalogAssetEditor, IAPTransactionAsset>
    {
        public IAPTransactionCatalogAssetEditor(string name)
            : base(name) { }

        protected override void DrawTypeSpecificBlocks(IAPTransactionAsset transaction)
        {
            using (new VerticalScope())
            {
                DrawProductIdFields(transaction);

                Space();

                DrawRewards(transaction);
            }

            Space();
        }

        protected override void FillCatalogItems()
        {
            var catalog = GameFoundationDatabaseSettings.database.transactionCatalog;
            if (catalog != null)
                catalog.GetItems(m_Items);
        }

        protected override void AddCatalogItem(IAPTransactionAsset catalogItem)
            => GameFoundationDatabaseSettings.database.transactionCatalog.Editor_AddItem(catalogItem);

        static void DrawProductIdFields(IAPTransactionAsset iapTransaction)
        {
            GUILayout.Label("Product Ids", GameFoundationEditorStyles.titleStyle);

            using (new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                iapTransaction.m_AppleId = TextField(new GUIContent("Apple Identifier"), iapTransaction.m_AppleId);
                iapTransaction.m_GoogleId = TextField(new GUIContent("Google Product ID"), iapTransaction.m_GoogleId);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                GUI.changed = false;
            }
        }
    }
}
