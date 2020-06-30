using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    static class MenuItems
    {
        const int GF_GameParameter = 1001;
        const int GF_Inventory = 1002;
        const int GF_Currency = 1003;
        const int GF_Store = 1005;
        const int GF_Transactions = 1004;
        const int GF_Tags = 1006;

        /// <summary>
        /// Creates menu item for game parameters system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Game Parameters", priority = GF_GameParameter)]
        public static void ShowGameParameterWindow()
        {
            GameParameterEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for currency system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Currency", priority = GF_Currency)]
        public static void ShowCurrencyWindow()
        {
            CurrencyEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for inventory system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Inventory", priority = GF_Inventory)]
        public static void ShowInventoriesWindow()
        {
            InventoryEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for Store system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Store", priority = GF_Store)]
        public static void ShowStoresWindow()
        {
            StoreEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for Store system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Transactions", priority = GF_Transactions)]
        public static void ShowPurchasesWindow()
        {
            TransactionEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for tag system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Tags", priority = GF_Tags)]
        public static void ShowTagWindow()
        {
            TagEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Selects the GameFoundationDatabaseSettings asset file.
        /// </summary>
        [MenuItem("Window/Game Foundation/Settings/Database Settings", false, 2012)]
        public static void SelectGameFoundationDatabaseSettingsAssetFile()
        {
            Selection.SetActiveObjectWithContext(GameFoundationDatabaseSettings.singleton, null);
        }
    }
}
