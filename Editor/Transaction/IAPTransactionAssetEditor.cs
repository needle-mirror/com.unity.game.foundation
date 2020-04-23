using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;
using static UnityEditor.EditorGUILayout;

namespace UnityEditor.GameFoundation
{
    internal class IAPTransactionAssetEditor : BaseTransactionAssetEditor<IAPTransactionAssetEditor, IAPTransactionAsset>
    {
        public IAPTransactionAssetEditor(string name) : base(name)
        {}

        protected override void AddCatalogItem(IAPTransactionAsset iapTransaction)
            => GameFoundationDatabaseSettings.database.transactionCatalog.Editor_AddItem(iapTransaction);

        protected override IAPTransactionAsset[] GetCatalogItems()
            => GameFoundationDatabaseSettings.database.transactionCatalog.GetItems<IAPTransactionAsset>();

        protected override void GetCatalogItems(ICollection<IAPTransactionAsset> target)
            => GameFoundationDatabaseSettings.database.transactionCatalog.GetItems<IAPTransactionAsset>(target);

        protected void DrawProductIdFields(IAPTransactionAsset iapTransaction)
        {
            GUILayout.Label("Product Ids", GameFoundationEditorStyles.titleStyle);

            using(new GUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
            {
                iapTransaction.m_AppleId = TextField(new GUIContent("Apple Identifier"), iapTransaction.m_AppleId);
                iapTransaction.m_GoogleId = TextField(new GUIContent("Google Product ID"), iapTransaction.m_GoogleId);
            }

            if(GUI.changed)
            {
                EditorUtility.SetDirty(GameFoundationDatabaseSettings.database.transactionCatalog);
                GUI.changed = false;
            }
        }

        protected override void DrawExchanges(IAPTransactionAsset transaction)
        {
            using (new VerticalScope())
            {
                DrawProductIdFields(transaction);

                EditorGUILayout.Space();

                DrawRewards(transaction);
            }
        }
    }
}
