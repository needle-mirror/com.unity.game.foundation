using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This is a class for storing Definitions for a system that the user setup in the editor.
    /// Derived classes will specify each generic to specify which classes are used by their Catalog.
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this Catalog uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this Catalog uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this Catalog uses.</typeparam>
    /// <typeparam name="T4">The type of Items this Catalog uses.</typeparam>
    public abstract class BaseCatalog<T1, T2, T3, T4> : ScriptableObject
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T3, T4>
        where T4 : BaseItem<T3, T4>
    {
        [SerializeField]
        protected List<CategoryDefinition> m_Categories = new List<CategoryDefinition>();

        /// <summary>
        /// Returns specified CategoryDefinition by its hash.
        /// </summary>
        /// <param name="categoryId">The id of the CategoryDefinition we're looking for.</param>
        /// <returns>The requested CategoryDefinition, or null if an invalid hash.</returns>
        public CategoryDefinition GetCategory(string categoryId)
        {
            return GetCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// Returns specified CategoryDefinition by its hash.
        /// </summary>
        /// <param name="categoryHash">The hash of the CategoryDefinition we're looking for.</param>
        /// <returns>The requested CategoryDefinition or null if the hash is not found.</returns>
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

        /// <summary>
        /// A list of all CollectionDefinition this Catalog can use.
        /// </summary>
        [SerializeField]
        protected List<T1> m_CollectionDefinitions = new List<T1>();

        /// <summary>
        /// A list of each type of ItemDefinition this Catalog can use.
        /// </summary>
        [SerializeField]
        protected List<T3> m_ItemDefinitions = new List<T3>();

        /// <summary>
        /// A list of DefaultCollectionDefinitions in this Catalog.
        /// </summary>
        [SerializeField]
        internal List<DefaultCollectionDefinition<T1,T2,T3,T4>> m_DefaultCollectionDefinitions = new List<DefaultCollectionDefinition<T1, T2, T3, T4>>();

        /// <summary>
        /// Iterator for accessing the CategoryDefinitions in this Catalog.
        /// </summary>
        /// <returns>An iterator for accessing the CategoryDefinitions in this Catalog.</returns>
        public IEnumerable<CategoryDefinition> categories
        {
            get { return m_Categories; }
        }

        /// <summary>
        /// Adds the given CategoryDefinition to this Catalog.
        /// </summary>
        /// <param name="category">The CategoryDefinition to add.</param>
        /// <returns>Whether or not the CategoryDefinition was added successfully.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate entry is given.</exception>
        public bool AddCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot add a CategoryDefinition to a Catalog while in play mode.");

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
        /// Returns the CategoryDefinition at the given index.
        /// </summary>
        /// <param name="index">The index to return.</param>
        /// <returns>The CategoryDefinition at the specified index</returns>
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
        /// Returns the index of requested CategoryDefinition, or -1 if this CategoryDefinition is not found in this Catalog.
        /// </summary>
        /// <param name="category">The CategoryDefinition who's index we are looking for.</param>
        /// <returns>The index of the requested CategoryDefinition in this Catalog, or -1 if not found.</returns>
        public int GetIndexOfCategory(CategoryDefinition category)
        {
            return m_Categories.IndexOf(category);
        }

        /// <summary>
        /// Removes the given CategoryDefinition from this Catalog.
        /// </summary>
        /// <param name="category">The CategoryDefinition to remove.</param>
        /// <returns>Whether or not the CategoryDefinition was successfully removed.</returns>
        public bool RemoveCategory(CategoryDefinition category)
        {
            Tools.ThrowIfPlayMode("Cannot remove a category from a Catalog while in play mode.");
            
            return m_Categories.Remove(category);
        }

        /// <summary>
        /// Returns the number of CategoryDefinitions in this Catalog.
        /// </summary>
        /// <returns>The number of CategoryDefinitions in this Catalog.</returns>
        public int categoryCount
        {
            get { return m_Categories.Count; }
        }

        /// <summary>
        /// This is an enumerator for iterating through all CollectionDefinitions.
        /// </summary>
        /// <returns>An enumerator for iterating through all CollectionDefinitions.</returns>
        public IEnumerable<T1> allCollectionDefinitions
        {
            get { return m_CollectionDefinitions; }
        }

        /// <summary>
        /// Adds the given CollectionDefinition to this Catalog.
        /// </summary>
        /// <param name="collectionDefinition">The CollectionDefinition to add.</param>
        /// <returns>Whether or not the CollectionDefinition was added successfully.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate entry is given.</exception>
        public bool AddCollectionDefinition(T1 collectionDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot add a CollectionDefinition to a Catalog while in play mode.");

            if (collectionDefinition == null)
            {
                return false;
            }

            if (GetCollectionDefinition(collectionDefinition.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + collectionDefinition.id + ", hash: " + collectionDefinition.hash + ")");
            }

            m_CollectionDefinitions.Add(collectionDefinition);
            return true;
        }

        /// <summary>
        /// Returns the CollectionDefinition at the requested index.
        /// </summary>
        /// <param name="index">The index of the CollectionDefinition to retrieve.</param>
        /// <returns>The CollectionDefinition at the requested index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of range.</exception>
        public T1 GetCollectionDefinitionByIndex(int index)
        {
            if (index < 0 || index >= m_CollectionDefinitions.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return m_CollectionDefinitions[index];
        }

        /// <summary>
        /// Returns the index of the requested CollectionDefinition or -1 if it's not in this Catalog.
        /// </summary>
        /// <param name="collectionDefinition">The CollectionDefinition we are looking for.</param>
        /// <returns>The index of the requested CollectionDefinition or -1 if it's not in this Catalog.</returns>
        public int GetIndexOfCollectionDefinition(T1 collectionDefinition)
        {
            return m_CollectionDefinitions.IndexOf(collectionDefinition);
        }

        /// <summary>
        /// Removes the given CollectionDefinition from this Catalog.
        /// </summary>
        /// <param name="collectionDefinition">The CollectionDefinition to remove.</param>
        /// <returns>Whether or not the CollectionDefinition was successfully removed.</returns>
        public bool RemoveCollectionDefinition(T1 collectionDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a CollectionDefinition from a Catalog while in play mode.");

            if (collectionDefinition == null || !m_CollectionDefinitions.Contains(collectionDefinition))
            {
                return false;
            }
            
            collectionDefinition.OnRemove();

            return m_CollectionDefinitions.Remove(collectionDefinition);
        }

        /// <summary>
        /// Returns the number of CollectionDefinitions in this Catalog.
        /// </summary>
        /// <returns>The number of CollectionDefinitions in this Catalog.</returns>
        public int collectionDefinitionCount
        {
            get { return m_CollectionDefinitions.Count; }
        }

        /// <summary>
        /// This is an enumerator for iterating through ItemDefinitions.
        /// </summary>
        /// <returns>An enumerator for iterating through ItemDefinitions.</returns>
        public IEnumerable<T3> allItemDefinitions
        {
            get { return m_ItemDefinitions; }
        }

        /// <summary>
        /// Adds the given ItemDefinition to this Catalog.
        /// </summary>
        /// <param name="definition">The ItemDefinition to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate definition is given.</exception>
        public bool AddItemDefinition(T3 definition)
        {
            Tools.ThrowIfPlayMode("Cannot add an ItemDefinition to a Catalog while in play mode.");

            if (definition == null)
            {
                return false;
            }

            if (GetItemDefinition(definition.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + definition.id + ", hash: " + definition.hash + ")");
            }
            
            m_ItemDefinitions.Add(definition);
            return true;
        }

        /// <summary>
        /// Returns the ItemDefinition at the requested index.
        /// </summary>
        /// <param name="index">The index we are checking.</param>
        /// <returns>The ItemDefinition at the requested index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of range.</exception>
        public T3 GetItemDefinitionByIndex(int index)
        {
            if (index < 0 || index >= m_ItemDefinitions.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return m_ItemDefinitions[index];
        }

        /// <summary>
        /// Returns the index of the given ItemDefinition.
        /// </summary>
        /// <param name="itemDefinition">The ItemDefinition we are checking.</param>
        /// <returns>The index of this ItemDefinition.</returns>
        public int GetIndexOfItemDefinition(T3 itemDefinition)
        {
            return m_ItemDefinitions.IndexOf(itemDefinition);
        }

        /// <summary>
        /// Removes the given ItemDefinition from this Catalog.
        /// </summary>
        /// <param name="definition">The ItemDefinition to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveItemDefinition(T3 definition)
        {
            Tools.ThrowIfPlayMode("Cannot remove an ItemDefinition from a Catalog while in play mode.");

            if (definition == null || !m_ItemDefinitions.Contains(definition))
            {
                return false;
            }

            definition.OnRemove();
            
            return m_ItemDefinitions.Remove(definition);
        }

        /// <summary>
        /// Returns the number of ItemDefinitions within this Catalog.
        /// </summary>
        /// <returns>The number of ItemDefinitions within this Catalog.</returns>
        public int itemDefinitionCount
        {
            get { return m_ItemDefinitions.Count; }
        }

        /// <summary>
        /// Enumerator for iterating through DefaultCollectionDefinitions.
        /// </summary>
        /// <returns>An enumerator for iterating through DefaultCollectionDefinitions.</returns>
        public IEnumerable<DefaultCollectionDefinition<T1, T2, T3, T4>> defaultCollectionDefinitions
        {
            get { return m_DefaultCollectionDefinitions; }
        }

        /// <summary>
        /// Adds the given DefaultCollectionDefinition to this Catalog.
        /// </summary>
        /// <param name="defaultCollectionDefinition">The DefaultCollectionDefinition to add.</param>
        /// <returns>Whether or not the adding was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if a duplicate default collection definition is given.</exception>
        public bool AddDefaultCollectionDefinition(DefaultCollectionDefinition<T1, T2, T3, T4> defaultCollectionDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot add a DefaultCollectionDefinition to a Catalog while in play mode.");

            if (defaultCollectionDefinition == null)
            {
                return false;
            }
            
            if (GetDefaultCollectionDefinition(defaultCollectionDefinition.hash) != null)
            {
                throw new ArgumentException("The object is already registered within this Catalog. (id: " + defaultCollectionDefinition.id + ", hash: " + defaultCollectionDefinition.hash + ")");
            }
            
            m_DefaultCollectionDefinitions.Add(defaultCollectionDefinition);
            return true;
        }

        /// <summary>
        /// Returns the DefaultCollectionDefinition at the requested index.
        /// </summary>
        /// <param name="index">The index requested.</param>
        /// <returns>The DefaultCollectionDefinition at specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of range.</exception>
        public DefaultCollectionDefinition<T1, T2, T3, T4> GetDefaultCollectionDefinitionByIndex(int index)
        {
            if (index < 0 || index >= m_DefaultCollectionDefinitions.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return m_DefaultCollectionDefinitions[index];
        }

        /// <summary>
        /// Returns the index of the given DefaultCollectionDefinition, or -1 if it's not found in this Catalog.
        /// </summary>
        /// <param name="defaultCollectionDefinition">The DefaultCollectionDefinition we are checking.</param>
        /// <returns>The index of the given DefaultCollectionDefinition in this Catalog.</returns>
        public int GetIndexOfDefaultCollectionDefinition(DefaultCollectionDefinition<T1, T2, T3, T4> defaultCollectionDefinition)
        {
            return m_DefaultCollectionDefinitions.IndexOf(defaultCollectionDefinition);
        }

        /// <summary>
        /// Removes the given DefaultCollectionDefinition from this Catalog.
        /// </summary>
        /// <param name="defaultCollectionDefinition">The DefaultCollectionDefinition to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveCollectionDefinition(DefaultCollectionDefinition<T1, T2, T3, T4> defaultCollectionDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a DefaultCollectionDefinition from a Catalog while in play mode.");
                
            return m_DefaultCollectionDefinitions.Remove(defaultCollectionDefinition);
        }

        /// <summary>
        /// Returns the number of DefaultCollectionDefinitions in this Catalog.
        /// </summary>
        /// <returns>The number of DefaultCollectionDefinitions in this Catalog.</returns>
        public int defaultCollectionDefinitionCount
        {
            get { return m_DefaultCollectionDefinitions.Count; }
        }

        /// <summary>
        /// Find CollectionDefinition by Definition Id.
        /// </summary>
        /// <param name="definitionId">The id of the Definition we want.</param>
        /// <returns>Reference to the CollectionDefinition requested.</returns>
        public T1 GetCollectionDefinition(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
                return null;
            
            return GetCollectionDefinition(Tools.StringToHash(definitionId));
        }

        /// <summary>
        /// Find CollectionDefinition by hash.
        /// </summary>
        /// <param name="definitionHash">The hash of the Definition we want.</param>
        /// <returns>Reference to the CollectionDefinition requested.</returns>
        public T1 GetCollectionDefinition(int definitionHash)
        {
            foreach (var collectionDefinition in m_CollectionDefinitions)
            {
                if (collectionDefinition.hash.Equals(definitionHash))
                {
                    return collectionDefinition;
                }
            }
            
            return null;
        }

        /// <summary>
        /// This is a getter for getting ItemDefinitions by their id.
        /// </summary>
        /// <param name="definitionId">The id of the Definition we want.</param>
        /// <returns>Reference to the ItemDefinition requested.</returns>
        public T3 GetItemDefinition(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId))
                return null;
            
            return GetItemDefinition(Tools.StringToHash(definitionId));
        }

        /// <summary>
        /// This is a getter for getting ItemDefinitions by their id hash.
        /// </summary>
        /// <param name="definitionHash">The hash of the Definition we want.</param>
        /// <returns>Reference to the ItemDefinition requested.</returns>
        public T3 GetItemDefinition(int definitionHash)
        {
            foreach (var itemDefinition in m_ItemDefinitions)
            {
                if (itemDefinition.hash.Equals(definitionHash))
                {
                    return itemDefinition;
                }
            }

            return null;
        }

        /// <summary>
        /// This will return an enumerator for iterating through ItemDefinitions with the designated Category by CategoryDefinition id.
        /// </summary>
        /// <param name="categoryId">The id of the CategoryDefinition to iterate through.</param>
        /// <returns>An enumerator of ItemDefinitions that contain the given Category.</returns>
        public IEnumerable<T3> GetItemsByCategory(string categoryId)
        {
            if (categoryId == null)
                return null;
            
            return GetItemsByCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// This will return an enumerator for iterating through ItemDefinitions with the designated Category by CategoryDefinition id hash
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition we want to check for.</param>
        /// <returns>An enumerator of ItemDefinitions that contain the given Category.</returns>
        public IEnumerable<T3> GetItemsByCategory(int categoryHash)
        {
            foreach (var definition in allItemDefinitions)
            {
                if (definition != null && definition.categories != null)
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
        }

        /// <summary>
        /// This will return an enumerator for iterating through ItemDefinitions with the designated CategoryDefinition
        /// </summary>
        /// <param name="category">The Category we want to check for.</param>
        /// <returns>An enumrator of ItemDefinitions that contain the given CategoryDefinition.</returns>
        public IEnumerable<T3> GetItemsByCategory(CategoryDefinition category)
        {
            if (category == null)
                return null;
            
            return GetItemsByCategory(category.hash);
        }

        /// <summary>
        /// This gets the DefaultCollectionDefinitions by id string.
        /// </summary>
        /// <param name="defaultDefinitionId">The id of the DefaultCollectionDefinition we want.</param>
        /// <returns>Reference to the DefaultCollectionDefinition requested.</returns>
        public DefaultCollectionDefinition<T1, T2, T3, T4> GetDefaultCollectionDefinition(string defaultDefinitionId)
        {
            if (string.IsNullOrEmpty(defaultDefinitionId))
                return null;
            
            return GetDefaultCollectionDefinition(Tools.StringToHash(defaultDefinitionId));
        }

        /// <summary>
        /// This gets the DefaultCollectionDefinition by id hash.
        /// </summary>
        /// <param name="defaultDefinitionHash">The hash of the DefaultCollectionDefinition we want.</param>
        /// <returns>Reference to the DefaultCollectionDefinition requested.</returns>
        public DefaultCollectionDefinition<T1, T2, T3, T4> GetDefaultCollectionDefinition(int defaultDefinitionHash)
        {
            foreach (var defaultCollectionDefinition in m_DefaultCollectionDefinitions)
            {
                if (defaultCollectionDefinition.hash.Equals(defaultDefinitionHash))
                {
                    return defaultCollectionDefinition;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if the given hash is available to be added to CollectionDefinitions.
        /// </summary>
        /// <param name="hash">The hash we are checking for.</param>
        /// <returns>True/False whether or not hash is available for use.</returns>
        public bool IsCollectionDefinitionHashUnique(int hash)
        {
            return GetCollectionDefinition(hash) == null;
        }

        /// <summary>
        /// Check if the given hash is not yet within ItemDefinitions and is available for use.
        /// </summary>
        /// <param name="hash">The hash we are checking for.</param>
        /// <returns>True/False whether or not hash is available for use.</returns>
        public bool IsItemDefinitionHashUnique(int hash)
        {
            return GetItemDefinition(hash) == null;
        }

        /// <summary>
        /// Check if the given CategoryDefinition is found in this Catalog's list of CategoryDefinitions.
        /// </summary>
        /// <param name="category">The CategoryDefinition we are checking for.</param>
        /// <returns>True if the CategoryDefinition is found, else False.</returns>
        public bool HasCategoryDefinition(CategoryDefinition category)
        {
            if (category == null)
            {
                return false;
            }

            foreach (CategoryDefinition currentCategory in m_Categories)
            {
                if (currentCategory.hash == category.hash)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the given CategoryDefinition display name is found in this Catalog's list of CategoryDefinitions.
        /// </summary>
        /// <param name="categoryName">The display name of the Category we are checking for.</param>
        /// <returns>True if the CategoryDefinition is found.</returns>
        public bool HasCategoryDefinition(string categoryName)
        {
            foreach (CategoryDefinition category in m_Categories)
            {
                if (category.displayName == categoryName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
