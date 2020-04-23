using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    internal class VirtualTransactionAssetEditor : BaseTransactionAssetEditor<VirtualTransactionAssetEditor, VirtualTransactionAsset>
    {
        public VirtualTransactionAssetEditor(string name) : base(name)
        { }

        protected override void AddCatalogItem(VirtualTransactionAsset transaction)
            => GameFoundationDatabaseSettings.database.transactionCatalog.Editor_AddItem(transaction);

        protected override VirtualTransactionAsset[] GetCatalogItems()
            => GameFoundationDatabaseSettings.database.transactionCatalog.GetItems<VirtualTransactionAsset>();

        protected override void GetCatalogItems(ICollection<VirtualTransactionAsset> target)
            => GameFoundationDatabaseSettings.database.transactionCatalog.GetItems(target);

        protected override void DrawExchanges(VirtualTransactionAsset transaction)
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
        }
    }
}
