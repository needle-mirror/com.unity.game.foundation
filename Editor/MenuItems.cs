using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal static class MenuItems
    {
        const int GF_Inventory = 1000;
        const int GF_Currency = 1001;
        const int GF_Store = 1002;
        const int GF_Stat = 1003;
        const int GF_Transactions = 1004;

        /// <summary>
        /// Creates menu item for stats system and shows the window when clicked.
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
        /// Creates menu item for stats system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Stat", priority = GF_Stat)]
        public static void ShowStatWindow()
        {
            StatEditorWindow.ShowWindow();
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
        /// Selects the GameFoundationSettings asset file.
        /// </summary>
        [MenuItem("Window/Game Foundation/Settings/Runtime Settings", false, 2011)]
        public static void SelectGameFoundationSettingsAssetFile()
        {
            Selection.SetActiveObjectWithContext(GameFoundationSettings.singleton, null);
        }
        
        /// <summary>
        /// Selects the GameFoundationDatabaseSettings asset file.
        /// </summary>
        [MenuItem("Window/Game Foundation/Settings/Database Settings", false, 2012)]
        public static void SelectGameFoundationDatabaseSettingsAssetFile()
        {
            Selection.SetActiveObjectWithContext(GameFoundationDatabaseSettings.singleton, null);
        }

        [MenuItem("Window/Game Foundation/Tools/Debugger", false, 2013)]
        public static void ShowDebugWindow()
        {
            DebugEditorWindow.ShowWindow();
        }
        
        [MenuItem("Window/Game Foundation/Help/Documentation", false, 2016)]
        public static void OpenHelpURL()
        {
            Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.game.foundation@latest/");
        }
        
        [MenuItem("Window/Game Foundation/Help/Support Forum", false, 2017)]
        public static void OpenSupportForumURL()
        {
            Application.OpenURL("https://forum.unity.com/forums/game-foundation.416/");
        }
    }
}
