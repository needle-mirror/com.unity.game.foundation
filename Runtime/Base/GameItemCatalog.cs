using System;
using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// A Catalog for all GameItemDefinitions.
    /// </summary>
    [CreateAssetMenu(fileName = "GameItemCatalog.asset", menuName = "Game Foundation/Catalog/GameItem Catalog")]
    public class GameItemCatalog : ScriptableObject
    {
        /// <summary>
        /// A dictionary of all CategoryDefinitions.
        /// </summary>
        [SerializeField]
        protected internal List<CategoryDefinition> m_Categories = new List<CategoryDefinition>();

        /// <summary>
        /// A dictionary of all GameItemDefinitions.
        /// </summary>
        [SerializeField]
        protected internal List<GameItemDefinition> m_Definitions = new List<GameItemDefinition>();

        /// <summary>
        /// Iterator for accessing the Categories in this GameItemCatalog.
        /// </summary>
        /// <returns>An iterator for accessing the Categories in this GameItemCatalog.</returns>
        public IEnumerable<CategoryDefinition> categories
        {
            get { return m_Categories; }
        }

        /// <summary>
        /// Adds the given Category to this GameItemCatalog.
        /// </summary>
        /// <param name="category">The Category to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if the given category is a duplicate.</exception>
        public bool AddCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot add a Category to a GameItemCatalog while in play mode.");

            if (category == null)
            {
                return false;
            }
            
            if (GetCategory(category.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + category.id + ", hash: " + category.hash + ")");
            }

            m_Categories.Add(category);
            return true;
        }

        /// <summary>
        /// Returns the Category at the specified index.
        /// </summary>
        /// <param name="index">The index to return.</param>
        /// <returns>The Category at requested index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of range.</exception>
        public CategoryDefinition GetCategoryByIndex(int index)
        {
            if (index < 0 || index >= m_Categories.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return m_Categories[index];
        }

        /// <summary>
        /// Returns the index of requested Category, or -1 if this Category is not in this GameItemCatalog.
        /// </summary>
        /// <param name="category">The Category who's index we are looking for.</param>
        /// <returns>The index of requested Category, or -1 if this Category is not in this GameItemCatalog.</returns>
        public int GetIndexOfCategory(CategoryDefinition category)
        {
            if (category == null)
                return -1;
            
            return m_Categories.IndexOf(category);
        }

        /// <summary>
        /// Removes the given Category from this GameItemCatalog.
        /// </summary>
        /// <param name="category">The Category to remove</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot remove a Category from a GameItemCatalog while in play mode.");
            
            return m_Categories.Remove(category);
        }

        /// <summary>
        /// Returns the number of Categories within this GameItemCatalog.
        /// </summary>
        /// <returns>The number of Categories within this GameItemCatalog.</returns>
        public int categoryCount
        {
            get { return m_Categories.Count; }
        }

        /// <summary>
        /// This is an enumerator for iterating through all GameItemDefinitions.
        /// </summary>
        /// <returns>An enumerator for iterating through all GameItemDefinitions.</returns>
        public IEnumerable<GameItemDefinition> allGameItemDefinitions
        {
            get { return m_Definitions; }
        }

        /// <summary>
        /// Adds the given GameItemDefinition to this GameItemCatalog.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition to add.</param>
        /// <returns>Whether or not the GameItemDefinition was successfully added.</returns>
        /// <exception cref="ArgumentException">Thrown if the given game item definition is a duplicate.</exception>
        public bool AddGameItemDefinition(GameItemDefinition gameItemDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot add a GameItemDefinition to a GameItemCatalog while in play mode.");

            if (gameItemDefinition == null)
            {
                return false;
            }

            if (GetDefinition(gameItemDefinition.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + gameItemDefinition.id + ", hash: " + gameItemDefinition.hash + ")");
            }
            
            m_Definitions.Add(gameItemDefinition);
            return true;
        }

        /// <summary>
        /// Returns the GameItemDefinition at the given index.
        /// </summary>
        /// <param name="index">The index of the GameItemDefinition to return.</param>
        /// <returns>The GameItemDefinition at the requested index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of range.</exception>
        public GameItemDefinition GetGameItemDefinitionByIndex(int index)
        {
            if (index < 0 || index >= m_Definitions.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return m_Definitions[index];
        }

        /// <summary>
        /// Returns the index of the given GameItemDefinition in this GameItemCatalog, or -1 if not found.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition to find the index of.</param>
        /// <returns>The index of the given GameItemDefinition.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given game item definition is null.</exception>
        public int GetIndexOfGameItemDefinition(GameItemDefinition gameItemDefinition)
        {
            if (gameItemDefinition == null)
            {
                throw new IndexOutOfRangeException("Cannot get the index of a null game item definition.");
            }
            
            return m_Definitions.IndexOf(gameItemDefinition);
        }

        /// <summary>
        /// Removes the given GameItemDefinition from this GameItemCatalog.
        /// </summary>
        /// <param name="gameItemDefinition">The GameItemDefinition to remove.</param>
        /// <returns>Whether or not the GameItemDefinition was successfully removed.</returns>
        public bool RemoveGameItemDefinition(GameItemDefinition gameItemDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a GameItemDefinition from a GameItemCatalog while in play mode.");

            if (gameItemDefinition == null)
            {
                return false;
            }

            if (!m_Definitions.Contains(gameItemDefinition))
            {
                return false;
            }
            
            gameItemDefinition.OnRemove();

            return m_Definitions.Remove(gameItemDefinition);
        }

        /// <summary>
        /// Returns the number of GameItemDefinitions within this GameItemCatalog.
        /// </summary>
        /// <returns>The count of GameItemDefinitions in this GameItemCatalog.</returns>
        public int gameItemDefinitionCount
        {
            get { return m_Definitions.Count; }
        }

        /// <summary>
        /// Return specified GameItemDefinition by GameItemDefinition id string.
        /// </summary>
        /// <param name="definitionId">The GameItemDefinition id string to find.</param>
        /// <returns>Specified GameItemDefinition in this GameItemCatalog.</returns>
        public GameItemDefinition GetDefinition(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
                return null;
            
            return GetDefinition(Tools.StringToHash(definitionId));
        }

        /// <summary>
        /// Return specified GameItemDefinition by id hash.
        /// </summary>
        /// <param name="definitionHash">The id hash of the GameItemDefinition to find.</param>
        /// <returns>Specified GameItemDefinition.</returns>
        public GameItemDefinition GetDefinition(int definitionHash)
        {
            foreach(var definition in m_Definitions)
            {
                if (definition.hash == definitionHash)
                {
                    return definition;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns specified Category by CategoryDefinition id hash.
        /// </summary>
        /// <param name="categoryId">The id of the id of the CategoryDefinition to find.</param>
        /// <returns>The requested CategoryDefinition, or null if an invalid hash.</returns>
        public CategoryDefinition GetCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                return null;
            
            return GetCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// Returns specified CategoryDefinition by its hash.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition to find.</param>
        /// <returns>The requested CategoryDefinition, or null if not found.</returns>
        public CategoryDefinition GetCategory(int categoryHash)
        {
            foreach (CategoryDefinition definition in m_Categories)
            {
                if (definition.hash == categoryHash)
                {
                    return definition;
                }
            }

            return null;
        }

        private static readonly GameItemDefinition[] emptyDefinitionsEnumerable = new GameItemDefinition[0];

        /// <summary>
        /// This will return an enumerator for iterating through GameItemDefinitions with the designated Category.
        /// </summary>
        /// <param name="categoryId">The id string of the Category we want to iterate.</param>
        /// <returns>An enumerator of GameItemDefinitions that contain the given Category.</returns>
        public IEnumerable<GameItemDefinition> GetDefinitionByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
                return null;
            
            return GetDefinitionByCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// This will return an enumerator for iterating through GameItemDefinitions with the designated Category.
        /// </summary>
        /// <param name="categoryHash">The id hash of the Category we want to iterate.</param>
        /// <returns>An enumerator of GameItemDefinitions that contain the requested Category.</returns>
        public IEnumerable<GameItemDefinition> GetDefinitionByCategory(int categoryHash)
        {
            foreach (var definition in m_Definitions)
            {
                foreach (var category in definition.categories)
                {
                    if (category.hash == categoryHash)
                    {
                        yield return definition;
                    }
                }
            }
        }

        /// <summary>
        /// Check if the given hash is not yet included in this GameItemCatalog's list of GameItemDefinitions and is available for use.
        /// </summary>
        /// <param name="definitionId">The hash to search for in this Catalog's GameItemDefinitions list.</param>
        /// <returns>True/False whether or not hash is available for use.</returns>
        public bool IsDefinitionHashUnique(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
                return false;
            
            return GetDefinition(Tools.StringToHash(definitionId)) == null;
        }

        /// <summary>
        /// Simple factory method for creating an empty GameItemCatalog.
        /// </summary>
        /// <returns>The newly created GameItemCatalog.</returns>
        public static GameItemCatalog Create()
        {
            Tools.ThrowIfPlayMode("Cannot create a GameItem Catalog while in play mode.");
            
            var catalog = ScriptableObject.CreateInstance<GameItemCatalog>();

            return catalog;
        }
    }
}
