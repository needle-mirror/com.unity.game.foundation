using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    internal static class EditorAPIHelper
    {
        internal static readonly string k_MainInventoryDefinitionId = "main";
        internal static readonly string k_WalletInventoryDefinitionId = "wallet";

        // GameItemCatalog helper methods
        internal static List<GameItemDefinition> GetGameItemCatalogGameItemDefinitionsList()
        {
            if (GameFoundationSettings.gameItemCatalog == null || GameFoundationSettings.gameItemCatalog.allGameItemDefinitions == null)
            {
                return null;
            }

            return GameFoundationSettings.gameItemCatalog.allGameItemDefinitions.ToList();
        }

        internal static void AddGameItemDefinitionToGameItemCatalog(GameItemDefinition gameItemItem)
        {
            if (GameFoundationSettings.gameItemCatalog != null)
            {
                GameFoundationSettings.gameItemCatalog.AddGameItemDefinition(gameItemItem);
            }
            else
            {
                Debug.LogError("GameItemDefinition " + gameItemItem.displayName + " could not be added to the GameItem catalog because catalog is null");
            }
        }

        internal static void RemoveGameItemDefinitionFromGameItemCatalog(GameItemDefinition gameItemItem)
        {
            if (GameFoundationSettings.gameItemCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.gameItemCatalog.RemoveGameItemDefinition(gameItemItem);
                if (!successfullyRemoved)
                {
                    Debug.LogError("GameItemDefinition " + gameItemItem.displayName + " was unable to be removed from gameItem catalog list.");
                }
            }
            else
            {
                Debug.LogError("GameItemDefinition " + gameItemItem.displayName + " could not be removed from gameItem catalog because catalog is null");
            }
        }

        // GameItemDefinition helper methods
        internal static IEnumerable<CategoryDefinition> GetGameItemDefinitionCategories(GameItemDefinition itemInstance)
        {
            if (itemInstance == null || itemInstance.categories == null)
            {
                return null;
            }

            return itemInstance.categories;
        }

        // InventoryCatalog helper methods
        internal static List<InventoryItemDefinition> GetInventoryCatalogItemDefinitionsList()
        {
            if (GameFoundationSettings.inventoryCatalog == null || GameFoundationSettings.inventoryCatalog.allItemDefinitions == null)
            {
                return null;
            }

            return GameFoundationSettings.inventoryCatalog.allItemDefinitions.ToList();
        }

        internal static List<CategoryDefinition> GetInventoryCatalogCategoriesList()
        {
            if (GameFoundationSettings.inventoryCatalog == null || GameFoundationSettings.inventoryCatalog.allItemDefinitions == null)
            {
                return null;
            }

            return GameFoundationSettings.inventoryCatalog.categories.ToList();
        }

        internal static List<InventoryDefinition> GetInventoryCatalogCollectionDefinitionsList()
        {
            if (GameFoundationSettings.inventoryCatalog == null || GameFoundationSettings.inventoryCatalog.allCollectionDefinitions == null)
            {
                return null;
            }

            return GameFoundationSettings.inventoryCatalog.allCollectionDefinitions.ToList();
        }

        internal static void AddItemDefinitionToInventoryCatalog(InventoryItemDefinition item)
        {
            if (GameFoundationSettings.inventoryCatalog != null)
            {
                GameFoundationSettings.inventoryCatalog.AddItemDefinition(item);
            }
            else
            {
                Debug.LogError("Inventory Item " + item.displayName + " could not be added to the inventory catalog because catalog is null");
            }
        }

        internal static void RemoveItemDefinitionFromInventoryCatalog(InventoryItemDefinition item)
        {
            if (GameFoundationSettings.inventoryCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.inventoryCatalog.RemoveItemDefinition(item);
                if (!successfullyRemoved)
                {
                    Debug.LogError("Inventory Item " + item.displayName + " was unable to be removed from inventory catalog list.");
                }
            }
            else
            {
                Debug.LogError("Inventory Item " + item.displayName + " could not be removed from inventory catalog because catalog is null");
            }
        }

        internal static void AddCategoryDefinitionToInventoryCatalog(CategoryDefinition category)
        {
            if (GameFoundationSettings.inventoryCatalog != null)
            {
                GameFoundationSettings.inventoryCatalog.AddCategory(category);
            }
            else
            {
                Debug.LogError("Category " + category.displayName + " could not be added to the inventory catalog because catalog is null");
            }
        }

        internal static void RemoveCategoryDefinitionFromInventoryCatalog(CategoryDefinition category)
        {
            if (GameFoundationSettings.inventoryCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.inventoryCatalog.RemoveCategory(category);
                if (!successfullyRemoved)
                {
                    Debug.LogError("Category " + category.displayName + " was unable to be removed from inventory catalog list.");
                }
            }
            else
            {
                Debug.LogError("Category " + category.displayName + " could not be removed from inventory catalog because catalog is null");
            }
        }

        internal static void AddInventoryDefinitionToInventoryCatalog(InventoryDefinition inventory)
        {
            if (GameFoundationSettings.inventoryCatalog != null)
            {
                GameFoundationSettings.inventoryCatalog.AddCollectionDefinition(inventory);
            }
            else
            {
                Debug.LogError("Inventory " + inventory.displayName + " could not be added to the inventory catalog because catalog is null");
            }
        }

        internal static void RemoveInventoryDefinitionFromInventoryCatalog(InventoryDefinition inventory)
        {
            if (GameFoundationSettings.inventoryCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.inventoryCatalog.RemoveCollectionDefinition(inventory);
                if (!successfullyRemoved)
                {
                    Debug.LogError("Inventory " + inventory.displayName + " was unable to be removed from inventory catalog list.");
                }
            }
            else
            {
                Debug.LogError("Inventory " + inventory.displayName + " could not be removed from inventory catalog because catalog is null");
            }
        }

        // Stat catalog helper methods
        internal static void AddStatDefinitionToStatCatalog(StatDefinition item)
        {
            if (GameFoundationSettings.statCatalog != null)
            {
                GameFoundationSettings.statCatalog.AddStatDefinition(item);
            }
            else
            {
                Debug.LogError("Stat definition " + item.displayName + " could not be added to the stat catalog because catalog is null");
            }
        }

        internal static void RemoveStatDefinitionFromStatCatalog(StatDefinition item)
        {
            if (GameFoundationSettings.statCatalog != null)
            {
                bool successfullyRemoved = GameFoundationSettings.statCatalog.RemoveStatDefinition(item);
                if (!successfullyRemoved)
                {
                    Debug.LogError("Stat definition " + item.displayName + " was unable to be removed from stat catalog list.");
                }
            }
            else
            {
                Debug.LogError("Stat definition " + item.displayName + " could not be removed from stat catalog because catalog is null");
            }
        }

        internal static List<StatDefinition> GetStatCatalogDefinitionsList()
        {
            if (GameFoundationSettings.statCatalog == null || GameFoundationSettings.statCatalog.allStatDefinitions == null)
            {
                return null;
            }

            return GameFoundationSettings.statCatalog.allStatDefinitions.ToList();
        }

        // InventoryItemDefinition helper methods
        internal static InventoryItemDefinition CreateInventoryItemDefinition(string id, string displayName)
        {
            return InventoryItemDefinition.Create(id, displayName);
        }

        // CategoryDefinition helper methods
        internal static CategoryDefinition CreateCategoryDefinition(string categoryId, string displayName)
        {
            return new CategoryDefinition(categoryId, displayName);
        }

        // InventoryDefinition helper methods
        internal static InventoryDefinition CreateInventoryDefinition(string id, string displayName)
        {
            return InventoryDefinition.Create(id, displayName);
        }

        internal static IEnumerable<DefaultItem> GetInventoryDefinitionDefaultItems(InventoryDefinition inventoryInstance)
        {
            if (inventoryInstance == null || inventoryInstance.defaultItems == null)
            {
                return null;
            }

            return inventoryInstance.defaultItems;
        }

        // StatDefinition helper methods
        internal static StatDefinition CreateStatDefinition(string id, string displayName, StatDefinition.StatValueType statValueType)
        {
            return new StatDefinition(id, displayName, statValueType);
        }
    }
}
