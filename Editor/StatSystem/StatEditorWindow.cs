namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Class creates Stat system-specific editor window.
    /// </summary>
    internal class StatEditorWindow : CollectionEditorWindowBase
    {
        /// <summary>
        /// Opens the Stat window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<StatEditorWindow>(false, "Stat", true);
        }

        /// <summary>
        /// Adds the editors for the Stat system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();

            m_Editors.Add(new StatDefinitionEditor("Stats"));
        }
    }
}
