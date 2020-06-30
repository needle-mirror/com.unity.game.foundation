namespace UnityEditor.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Class creates Transaction system-specific editor window.
    /// </summary>
    class TransactionEditorWindow : CollectionEditorWindowBase
    {
        /// <summary>
        /// Opens the Transaction window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            GetWindow<TransactionEditorWindow>(false, "Transactions", true);
        }

        /// <summary>
        /// Adds the editors for the transaction system as tabs in the window.
        /// </summary>
        protected override void CreateEditors()
        {
            m_Editors.Clear();
            m_Editors.Add(new VirtualTransactionCatalogAssetEditor("Virtual Transaction"));
            m_Editors.Add(new IAPTransactionCatalogAssetEditor("IAP"));
        }
    }
}
