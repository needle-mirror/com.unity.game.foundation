namespace UnityEditor.GameFoundation
{
    internal static class MenuItems
    {
        /// <summary>
        /// Creates menu item for GameItem system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Game Item", false, 0)]
        public static void ShowGameItemsWindow()
        {
            GameItemEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for inventory system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Inventory", false, 0)]
        public static void ShowInventoriesWindow()
        {
            InventoryEditorWindow.ShowWindow();
        }

        /// <summary>
        /// Creates menu item for stats system and shows the window when clicked.
        /// </summary>
        [MenuItem("Window/Game Foundation/Stat", false, 0)]
        public static void ShowStatWindow()
        {
            StatEditorWindow.ShowWindow();
        }
    }
}
