// This warning is disabled here because we don't need to warn the dev when GameFoundation
// is using something obsolete, we only need to warn them when *they* use something that's
// obsolete. When the obsolete class is removed permanently, we can remove this pragma.
// Current obsolete classes: IconDetailDefinition
#pragma warning disable 618

using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    public static class EditorCatalogProvider
    {
        /// <summary>
        /// Initializes the runtime catalogs with the catalog and definition information
        /// in GameFoundationDatabase which is created in the Game Foundation editor windows.
        /// Runtime catalogs initialized are GameItemCatalog, InventoryCatalog and StatCatalog.
        /// InitializeCatalogs must successfully complete initialization before calling GameFoundation.Initialize();
        /// </summary>
        public static void InitializeCatalogs()
        {
            if (GameFoundationDatabaseSettings.database == null)
            {
                Debug.LogError("GameFoundationSettings database is null when trying to build runtime catalogs.");
                return;
            }

            GameFoundationDatabase editorDatabase = GameFoundationDatabaseSettings.database;

            BuildGameItemCatalog(editorDatabase);
            BuildInventoryCatalog(editorDatabase, CatalogManager.gameItemCatalog?.definitions);
            BuildStatCatalog(editorDatabase);
        }

        private static void BuildGameItemCatalog(GameFoundationDatabase editorDatabase)
        {
            List<UnityEngine.GameFoundation.GameItemDefinition> gameItemDefinitions = null;
            List<UnityEngine.GameFoundation.CategoryDefinition> gameItemCategoryDefinitions = null;

            if (editorDatabase == null)
            {
                Debug.LogError("GameFoundationSettings database is null when trying to build runtime game item catalog.");
            }
            else if (editorDatabase.gameItemCatalog == null)
            {
                Debug.LogError("Editor game item catalog is null when trying to build runtime game item catalog.");
            }
            else
            {
                gameItemCategoryDefinitions = BuildCategoryList(editorDatabase.gameItemCatalog.GetCategories());
                GameItemDefinition[] editorGameItemDefinitions = editorDatabase.gameItemCatalog.GetGameItemDefinitions();
                if (editorGameItemDefinitions != null)
                {
                    gameItemDefinitions = new List<UnityEngine.GameFoundation.GameItemDefinition>(editorGameItemDefinitions.Length);
                    foreach (var editorItemDefinition in editorGameItemDefinitions)
                    {
                        Dictionary<Type, UnityEngine.GameFoundation.BaseDetailDefinition> runtimeDetailDefinitions = BuildRuntimeDetailDictionary(editorItemDefinition);
                        gameItemDefinitions.Add(new UnityEngine.GameFoundation.GameItemDefinition(editorItemDefinition.id, editorItemDefinition.displayName, null, editorItemDefinition.GetCategoryHashes(), runtimeDetailDefinitions));
                    }
                }
            }

            if (gameItemDefinitions == null)
            {
                gameItemDefinitions = new List<UnityEngine.GameFoundation.GameItemDefinition>();
            }

            if (gameItemCategoryDefinitions == null)
            {
                gameItemCategoryDefinitions = new List<UnityEngine.GameFoundation.CategoryDefinition>();
            }

            CatalogManager.gameItemCatalog = new UnityEngine.GameFoundation.GameItemCatalog(gameItemDefinitions, gameItemCategoryDefinitions);
        }

        private static void BuildInventoryCatalog(GameFoundationDatabase editorDatabase, List<UnityEngine.GameFoundation.GameItemDefinition> gameItemDefinitions)
        {
            List<UnityEngine.GameFoundation.InventoryItemDefinition> inventoryItemDefinitions = null;
            List<UnityEngine.GameFoundation.InventoryDefinition> inventoryDefinitions = null;
            List<UnityEngine.GameFoundation.DefaultCollectionDefinition> defaultInventoryDefinitions = null;
            List<UnityEngine.GameFoundation.CategoryDefinition> inventoryItemCategoryDefinitions = null;

            if (editorDatabase == null)
            {
                Debug.LogError("GameFoundationDatabaseSettings database is null when trying to build runtime inventory catalog.");
            }
            else if (editorDatabase.inventoryCatalog == null)
            {
                Debug.LogError("Editor inventory catalog is null when trying to build runtime inventory catalog.");
            }
            else
            {
                // Build Inventory Category List
                inventoryItemCategoryDefinitions = BuildCategoryList(editorDatabase.inventoryCatalog.GetCategories());

                // Build Inventory Item List
                if (gameItemDefinitions == null)
                {
                    Debug.LogWarning("GameItemDefinitions is null when building inventoryCatalog which means that all InventoryItemDefinitions will have null reference definitions.");
                }
                InventoryItemDefinition[] editorInventoryItemDefinitions = editorDatabase.inventoryCatalog.GetItemDefinitions();
                if (editorInventoryItemDefinitions != null)
                {
                    inventoryItemDefinitions = new List<UnityEngine.GameFoundation.InventoryItemDefinition>(editorInventoryItemDefinitions.Length);
                    foreach (var editorInventoryItemDefinition in editorInventoryItemDefinitions)
                    {
                        UnityEngine.GameFoundation.GameItemDefinition referenceDefinition = GetRuntimeReferenceDefinition(gameItemDefinitions, editorInventoryItemDefinition.referenceDefinition);
                        Dictionary<Type, UnityEngine.GameFoundation.BaseDetailDefinition> runtimeDetailDefinitions = BuildRuntimeDetailDictionary(editorInventoryItemDefinition);
                        inventoryItemDefinitions.Add(new UnityEngine.GameFoundation.InventoryItemDefinition(editorInventoryItemDefinition.id,
                            editorInventoryItemDefinition.displayName, referenceDefinition,
                            editorInventoryItemDefinition.GetCategoryHashes(), runtimeDetailDefinitions));
                    }
                }

                // Build Inventory List
                InventoryDefinition[] editorInventoryDefinitions = editorDatabase.inventoryCatalog.GetCollectionDefinitions();
                if (editorInventoryDefinitions != null)
                {
                    inventoryDefinitions = new List<UnityEngine.GameFoundation.InventoryDefinition>(editorInventoryDefinitions.Length);
                    foreach (var editorInventoryDefinition in editorInventoryDefinitions)
                    {
                        UnityEngine.GameFoundation.GameItemDefinition referenceDefinition = GetRuntimeReferenceDefinition(gameItemDefinitions, editorInventoryDefinition.referenceDefinition);
                        Dictionary<Type, UnityEngine.GameFoundation.BaseDetailDefinition> runtimeDetailDefinitions = BuildRuntimeDetailDictionary(editorInventoryDefinition);
                        List<DefaultItemDefinition> runtimeDefaultItems = new List<DefaultItemDefinition>();
                        DefaultItem[] editorDefaultItems = editorInventoryDefinition.GetDefaultItems();
                        if (editorDefaultItems != null)
                        {
                            foreach (var editorDefaultItemDefinition in editorDefaultItems)
                            {
                                runtimeDefaultItems.Add(new DefaultItemDefinition(editorDefaultItemDefinition.definitionId, editorDefaultItemDefinition.quantity));
                            }
                        }

                        inventoryDefinitions.Add(new UnityEngine.GameFoundation.InventoryDefinition(editorInventoryDefinition.id,
                            editorInventoryDefinition.displayName, referenceDefinition,
                            editorInventoryDefinition.GetCategoryHashes(), runtimeDetailDefinitions, runtimeDefaultItems));
                    }
                }

                // Build Default Inventory Definition list
                DefaultCollectionDefinition[] editorDefaultInventoryDefinitions = editorDatabase.inventoryCatalog.GetDefaultCollectionDefinitions();
                if (editorDefaultInventoryDefinitions != null)
                {
                    defaultInventoryDefinitions = new List<UnityEngine.GameFoundation.DefaultCollectionDefinition>(editorDefaultInventoryDefinitions.Length);
                    foreach (var editorDefaultInventoryDefinition in editorDefaultInventoryDefinitions)
                    {
                        defaultInventoryDefinitions.Add(new UnityEngine.GameFoundation.DefaultCollectionDefinition(
                            editorDefaultInventoryDefinition.id,
                            editorDefaultInventoryDefinition.displayName,
                            editorDefaultInventoryDefinition.collectionDefinitionHash));
                    }
                }
            }

            if (inventoryItemDefinitions == null)
            {
                inventoryItemDefinitions = new List<UnityEngine.GameFoundation.InventoryItemDefinition>();
            }

            if (inventoryDefinitions == null)
            {
                inventoryDefinitions = new List<UnityEngine.GameFoundation.InventoryDefinition>();
            }

            if (defaultInventoryDefinitions == null)
            {
                defaultInventoryDefinitions = new List<UnityEngine.GameFoundation.DefaultCollectionDefinition>();
            }

            if (inventoryItemCategoryDefinitions == null)
            {
                inventoryItemCategoryDefinitions = new List<UnityEngine.GameFoundation.CategoryDefinition>();
            }

            CatalogManager.inventoryCatalog = new UnityEngine.GameFoundation.InventoryCatalog(inventoryItemDefinitions, inventoryDefinitions, defaultInventoryDefinitions, inventoryItemCategoryDefinitions);
        }

        private static void BuildStatCatalog(GameFoundationDatabase editorDatabase)
        {
            List<UnityEngine.GameFoundation.StatDefinition> statDefinitions = null;

            if (editorDatabase == null)
            {
                Debug.LogError("GameFoundationSettings database is null when trying to build runtime stat catalog.");
            }
            else if (editorDatabase.statCatalog == null)
            {
                Debug.LogError("Editor stat catalog is null when trying to build runtime stat catalog.");
            }
            else
            {
                List<StatDefinition> editorStatDefinitions = editorDatabase.statCatalog.m_StatDefinitions;
                if (editorStatDefinitions != null)
                {
                    statDefinitions = new List<UnityEngine.GameFoundation.StatDefinition>(editorStatDefinitions.Count);
                    foreach (var editorStatDefinition in editorStatDefinitions)
                    {
                        switch (editorStatDefinition.statValueType)
                        {
                            case StatDefinition.StatValueType.Float:
                                statDefinitions.Add(new UnityEngine.GameFoundation.StatDefinition(editorStatDefinition.id, editorStatDefinition.displayName, UnityEngine.GameFoundation.StatDefinition.StatValueType.Float));
                                break;
                            case StatDefinition.StatValueType.Int:
                                statDefinitions.Add(new UnityEngine.GameFoundation.StatDefinition(editorStatDefinition.id, editorStatDefinition.displayName, UnityEngine.GameFoundation.StatDefinition.StatValueType.Int));
                                break;
                            default:
                                Debug.LogWarning( "Trying to create runtime statDefinition of an unsupported editorStatValueType: " + editorStatDefinition.statValueType);
                                break;
                        }
                    }
                }
            }

            if (statDefinitions == null)
            {
                statDefinitions = new List<UnityEngine.GameFoundation.StatDefinition>();
            }

            CatalogManager.statCatalog = new UnityEngine.GameFoundation.StatCatalog(statDefinitions);
        }

        private static List<UnityEngine.GameFoundation.CategoryDefinition> BuildCategoryList(CategoryDefinition[] categories)
        {
            List<UnityEngine.GameFoundation.CategoryDefinition> runtimeCategories = new List<UnityEngine.GameFoundation.CategoryDefinition>();

            if (categories == null)
            {
                return runtimeCategories;
            }

            foreach (var categoryDefinition in categories)
            {
                runtimeCategories.Add(new UnityEngine.GameFoundation.CategoryDefinition(categoryDefinition.id, categoryDefinition.displayName));
            }

            return runtimeCategories;
        }

        private static Dictionary<Type, UnityEngine.GameFoundation.BaseDetailDefinition> BuildRuntimeDetailDictionary(GameItemDefinition editorItemDefinition)
        {
            if (editorItemDefinition == null)
            {
                return new Dictionary<Type, UnityEngine.GameFoundation.BaseDetailDefinition>();
            }

            BaseDetailDefinition[] editorDetailDefinitions = editorItemDefinition.GetDetailDefinitions();
            if (editorDetailDefinitions == null)
            {
                return new Dictionary<Type, UnityEngine.GameFoundation.BaseDetailDefinition>();
            }
            
            Dictionary<Type, UnityEngine.GameFoundation.BaseDetailDefinition> runtimeDetailDefinitions = new Dictionary<Type, UnityEngine.GameFoundation.BaseDetailDefinition>(editorDetailDefinitions.Length);
            foreach (BaseDetailDefinition editorDetailDefinition in editorDetailDefinitions)
            {
                UnityEngine.GameFoundation.BaseDetailDefinition runtimeDetailDefinition = editorDetailDefinition.CreateRuntimeDefinition();
                runtimeDetailDefinitions.Add(runtimeDetailDefinition.GetType(), runtimeDetailDefinition);
            }

            return runtimeDetailDefinitions;
        }

        private static UnityEngine.GameFoundation.GameItemDefinition GetRuntimeReferenceDefinition(List<UnityEngine.GameFoundation.GameItemDefinition> gameItemDefinitions, GameItemDefinition referenceDefinition)
        {
            UnityEngine.GameFoundation.GameItemDefinition runtimeReferenceDefinition = null;
            if (referenceDefinition == null)
            {
                return null;
            }

            if (gameItemDefinitions == null)
            {
                Debug.LogWarning("Runtime GameItemDefinitions list is null and therefore runtime reference definition will be null but editor inventory item has non null reference definition.");
                return null;
            }

            foreach (var itemDefinition in gameItemDefinitions)
            {
                if (itemDefinition.id == referenceDefinition.id)
                {
                    runtimeReferenceDefinition = itemDefinition;
                    break;
                }
            }
            return runtimeReferenceDefinition;
        }
    }
}
