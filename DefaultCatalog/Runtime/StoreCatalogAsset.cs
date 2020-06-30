namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public sealed partial class StoreCatalogAsset : SingleCollectionCatalogAsset<StoreAsset>
    {
        internal static readonly string k_MainStoreDefinitionKey = "main";
        internal static readonly string k_MainStoreDefinitionName = "Main";

        /// <summary>
        /// It listens the <see cref="TransactionCatalogAsset"/> for any
        /// <see cref="BaseTransactionAsset"/> to be removed and cleans the
        /// stores referencing it.
        /// </summary>
        protected override void InitializeSingleCollectionCatalog()
        {
#if UNITY_EDITOR
            Editor_InitializeCatalog();
#endif
        }

        /// <summary>
        /// This will make sure main and wallet exist and are setup, and fix
        /// things if they aren't.
        /// </summary>
        internal void VerifyDefaultCollections()
        {
            var mainStore = FindItem(k_MainStoreDefinitionKey);

            if (mainStore == null)
            {
                mainStore = CreateInstance<StoreAsset>();
                mainStore.m_Key = k_MainStoreDefinitionKey;
                mainStore.m_DisplayName = k_MainStoreDefinitionName;

#if UNITY_EDITOR

                // the Scriptable Object name that appears in the Project window
                mainStore.name = mainStore.Editor_AssetName;
#endif

                m_Items.Insert(0, mainStore);
            }
        }
    }
}
