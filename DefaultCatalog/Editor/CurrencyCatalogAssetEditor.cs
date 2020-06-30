using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;
using UnityEngine.GameFoundation.DefaultCatalog.Details;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    class CurrencyCatalogAssetEditor : BaseCatalogAssetEditor<CurrencyAsset>
    {
        static readonly GUIContent kInitialAllocationText =
            new GUIContent("Initial allocation", "The amount of this currency the player will have when starting playing.");

        static readonly GUIContent kMaximumAllocationText = new GUIContent(
            "Maximum allocation",
            "The maximum of this currency the player can own. " +
            "0 means no limit.");

        static readonly GUIContent kTypeText = new GUIContent(
            "Type",
            "Tells if this currency is Soft or Hard for analytics purpose. " +
            "Has no effect on how it is managed by Game Foundation.");

        protected override BaseCatalogAsset assetCatalog
            => GameFoundationDatabaseSettings.database.currencyCatalog;

        protected override GameFoundationAnalytics.TabName tabName
            => GameFoundationAnalytics.TabName.Currencies;

        public CurrencyCatalogAssetEditor(string name)
            : base(name) { }

        protected override void DrawGeneralFields(CurrencyAsset currency)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (var initialChanged = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName("Initial allocation");
                    var initialAllocation = EditorGUILayout.LongField(kInitialAllocationText, currency.m_InitialBalance);
                    if (initialChanged.changed)
                    {
                        currency.m_InitialBalance = initialAllocation;
                    }
                }

                using (var maxChanged = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName("Maximum allocation");
                    var maximumAllocation = EditorGUILayout.LongField(kMaximumAllocationText, currency.m_MaximumBalance);
                    if (maxChanged.changed)
                    {
                        currency.m_MaximumBalance = maximumAllocation;
                    }
                }

                using (var typeChanged = new EditorGUI.ChangeCheckScope())
                {
                    GUI.SetNextControlName("Type");
                    var type = EditorGUILayout.EnumPopup(kTypeText, currency.m_Type);
                    if (typeChanged.changed)
                    {
                        currency.m_Type = (CurrencyType)type;
                    }
                }
            }
        }

        protected override void FillCatalogItems()
        {
            var catalog = GameFoundationDatabaseSettings.database.currencyCatalog;
            if (catalog != null)
                catalog.GetItems(m_Items);
        }

        protected override void AddCatalogItem(CurrencyAsset catalogItem)
        {
            catalogItem.Editor_AddDetail<AnalyticsDetailAsset>();

            GameFoundationDatabaseSettings.database.currencyCatalog.Editor_AddItem(catalogItem);
        }
    }
}
