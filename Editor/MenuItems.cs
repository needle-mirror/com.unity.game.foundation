using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal static class MenuItems
    {
        /// <summary>
        /// Creates menu item for GameItem system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Game Item", false, 2000)]
        public static void ShowGameItemsWindow()
        {
            GameItemEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for inventory system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Inventory", false, 2000)]
        public static void ShowInventoriesWindow()
        {
            InventoryEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for stats system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Stat", false, 2000)]
        public static void ShowStatWindow()
        {
            StatEditorWindow.ShowWindow();
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
