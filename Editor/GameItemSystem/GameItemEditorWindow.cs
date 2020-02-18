namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Class creates GameItem system-specific editor window.
    /// </summary>
    internal class GameItemEditorWindow : CollectionEditorWindowBase
    {
        /// <summary>
        /// Opens the GameItem window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<GameItemEditorWindow>(false, "Game Item", true);
        }

        /// <summary>
        /// Adds the editors for the GameItem system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();

            m_Editors.Add(new GameItemDefinitionEditor("Game Items"));
        }
    }
}
