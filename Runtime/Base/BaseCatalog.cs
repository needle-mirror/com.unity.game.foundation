using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This is a class for storing catalogs of Definitions for a system.
    /// Derived classes will specify each generic to specify which classes are used by their Catalog.
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this Catalog uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this Catalog uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this Catalog uses.</typeparam>
    /// <typeparam name="T4">The type of Items this Catalog uses.</typeparam>
    public abstract class BaseCatalog<T1, T2, T3, T4>
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T1, T2, T3, T4>
        where T4 : BaseItem<T1, T2, T3, T4>
    {
        /// <summary>
        /// List of CategoryDefinitions inside this Catalog
        /// </summary>
        protected List<CategoryDefinition> categories { get; }

        /// <summary>
        /// A list of all CollectionDefinition this Catalog can use.
        /// </summary>
        protected List<T1> collectionDefinitions { get; }

        /// <summary>
        /// A list of each type of ItemDefinition this Catalog can use.
        /// </summary>
        protected List<T3> itemDefinitions { get; }

        /// <summary>
        /// A list of DefaultCollectionDefinitions in this Catalog.
        /// </summary>
        internal readonly List<DefaultCollectionDefinition> m_DefaultCollectionDefinitions;

        /// <summary>
        /// Constructor to build a BaseCatalog object.
        /// </summary>
        /// <param name="itemDefinitions">The list of item definitions of type BaseItemDefinition that will be available in this catalog for runtime instance instantiation. If null value is passed in an empty list will be created.</param>
        /// <param name="collectionDefinitions">The list of collection definitions of type BaseCollectionDefinition that will be available in this catalog for runtime instance instantiation. If null value is passed in an empty list will be created.</param>
        /// <param name="defaultCollectionDefinitions">The list of DefaultCollectionDefinitions that are the collections in this catalog that get automatically instantiated. If null value is passed in an empty list will be created.</param>
        /// <param name="categories">The list of CategoryDefinitions that are the possible categories which could be applied to items in this catalog. If null value is passed in an empty list will be created.</param>
        internal BaseCatalog(List<T3> itemDefinitions,  List<T1> collectionDefinitions, List<DefaultCollectionDefinition> defaultCollectionDefinitions, List<CategoryDefinition> categories)
        {
            this.itemDefinitions = itemDefinitions ?? new List<T3>();
            this.collectionDefinitions = collectionDefinitions ?? new List<T1>();
            m_DefaultCollectionDefinitions = defaultCollectionDefinitions ?? new List<DefaultCollectionDefinition>();
            this.categories = categories ?? new List<CategoryDefinition>();
        }

        /// <summary>
        /// Returns specified CategoryDefinition by its Hash.
        /// </summary>
        /// <param name="categoryId">The Id of the CategoryDefinition we're looking for.</param>
        /// <returns>The requested CategoryDefinition, or null if an invalid Hash.</returns>
        public CategoryDefinition GetCategory(string categoryId)
        {
            return GetCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// Returns specified CategoryDefinition by its Hash.
        /// </summary>
        /// <param name="categoryHash">The Hash of the CategoryDefinition we're looking for.</param>
        /// <returns>The requested CategoryDefinition or null if the Hash is not found.</returns>
        public CategoryDefinition GetCategory(int categoryHash)
        {
            foreach (CategoryDefinition definition in categories)
            {
                if (definition.hash == categoryHash)
                {
                    return definition;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Returns an array of all categories in this catalog.
        /// </summary>
        /// <returns>An array of all categories.</returns>
        public CategoryDefinition[] GetCategories()
        {
            return categories?.ToArray();
        }

        /// <summary>
        /// Fills in the given list with all categories in this catalog.
        /// Note: this returns the current state of all categories in the catalog.  To ensure
        /// that there are no invalid or duplicate entries, the 'categories' list 
        /// will always be cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="categories">The list to clear and write all categories to.</param>
        public void GetCategories(List<CategoryDefinition> categories)
        {
            if (categories == null)
            {
                return;
            }

            categories.Clear();

            if (this.categories == null)
            {
                return;
            }
            
            categories.AddRange(this.categories);
        }
        
        /// <summary>
        /// Find CollectionDefinition by Definition Id.
        /// </summary>
        /// <param name="collectionDefinitionId">The Id of the Definition we want.</param>
        /// <returns>Reference to the CollectionDefinition requested.</returns>
        public T1 GetCollectionDefinition(string collectionDefinitionId)
        {
            if (string.IsNullOrEmpty(collectionDefinitionId))
            {
                return null;
            }
            
            return GetCollectionDefinition(Tools.StringToHash(collectionDefinitionId));
        }

        /// <summary>
        /// Find CollectionDefinition by Hash.
        /// </summary>
        /// <param name="collectionDefinitionHash">The Hash of the Definition we want.</param>
        /// <returns>Reference to the CollectionDefinition requested.</returns>
        public T1 GetCollectionDefinition(int collectionDefinitionHash)
        {
            foreach (var collectionDefinition in collectionDefinitions)
            {
                if (collectionDefinition.hash.Equals(collectionDefinitionHash))
                {
                    return collectionDefinition;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Returns an array of all collection definitions in the catalog.
        /// </summary>
        /// <returns>An array of all collection definitions in the catalog.</returns>
        public T1[] GetCollectionDefinitions()
        {
            return collectionDefinitions?.ToArray();
        }

        /// <summary>
        /// Adds all collection definitions into the given list.
        /// Note: this returns the current state of all collection definitions in the catalog.  To ensure
        /// that there are no invalid or duplicate entries, the 'collectionDefinitions' list 
        /// will always be cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="collectionDefinitions">The list to clear and write all collection definitions to.</param>
        public void GetCollectionDefinitions(List<T1> collectionDefinitions)
        {
            if (collectionDefinitions == null)
            {
                return;
            }

            collectionDefinitions.Clear();

            if (this.collectionDefinitions == null)
            {
                return;
            }
            
            collectionDefinitions.AddRange(this.collectionDefinitions);
        }
        
                /// <summary>
        /// This is a getter for getting ItemDefinitions by their Id.
        /// </summary>
        /// <param name="itemDefinitionId">The Id of the Definition we want.</param>
        /// <returns>Reference to the ItemDefinition requested.</returns>
        public T3 GetItemDefinition(string itemDefinitionId)
        {
            if (string.IsNullOrEmpty(itemDefinitionId))
            {
                return null;
            }
            
            return GetItemDefinition(Tools.StringToHash(itemDefinitionId));
        }

        /// <summary>
        /// This is a getter for getting ItemDefinitions by their Hash.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash of the Definition we want.</param>
        /// <returns>Reference to the ItemDefinition requested.</returns>
        public T3 GetItemDefinition(int itemDefinitionHash)
        {
            foreach (var itemDefinition in itemDefinitions)
            {
                if (itemDefinition.hash.Equals(itemDefinitionHash))
                {
                    return itemDefinition;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an array of all item definitions.
        /// </summary>
        /// <returns>An array of all item definitions.</returns>
        public T3[] GetItemDefinitions()
        {
            return itemDefinitions?.ToArray();
        }

        /// <summary>
        /// Fills in the given list with all item definitions in this catalog.
        /// Note: this returns the current state of all item definitions in the catalog.  To ensure
        /// that there are no invalid or duplicate entries, the 'itemDefinitions' list 
        /// will always be cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="itemDefinitions">The list to clear and write all item definitions to.</param>
        public void GetItemDefinitions(List<T3> itemDefinitions)
        {
            if (itemDefinitions == null)
            {
                return;
            }

            itemDefinitions.Clear();

            if (this.itemDefinitions == null)
            {
                return;
            }
            
            itemDefinitions.AddRange(this.itemDefinitions);
        }
        
        /// <summary>
        /// This will return an array of ItemDefinitions with the designated Category by CategoryDefinition id.
        /// </summary>
        /// <param name="categoryId">The id of the CategoryDefinition we want.</param>
        /// <returns>An array of ItemDefinitions that contain the given Category.</returns>
        public T3[] GetItemDefinitionsByCategory(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return null;
            }
            
            return GetItemDefinitionsByCategory(Tools.StringToHash(categoryId));
        }

        /// <summary>
        /// Fills in the given list with items matching the given category.
        /// Note: this returns the current state of items in the catalog.  To ensure
        /// that there are no invalid or duplicate entries, the 'items' list will always
        /// be cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="categoryId">The id of the CategoryDefinition we want.</param>
        /// <param name="items">The list to clear and write matching items into.</param>
        public void GetItemDefinitionsByCategory(string categoryId, List<T3> items)
        {
            if (items == null)
            {
                return;
            }

            items.Clear();

            if (string.IsNullOrEmpty(categoryId))
            {
                return;
            }
            
            items.AddRange(GetItemDefinitionsByCategory(categoryId));
        }

        /// <summary>
        /// This will return an array of ItemDefinitions with the designated Category by CategoryDefinition id hash.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition we want to check for.</param>
        /// <returns>An array of ItemDefinitions that contain the given Category.</returns>
        public T3[] GetItemDefinitionsByCategory(int categoryHash)
        {
            List<T3> items = new List<T3>();
            foreach (var definition in itemDefinitions)
            {
                if (definition != null && definition.GetCategories() != null)
                {
                    foreach (var category in definition.GetCategories())
                    {
                        if (category.hash == categoryHash)
                        {
                            items.Add(definition);
                        }
                    }
                }
            }

            return items.ToArray();
        }

        /// <summary>
        /// Fills in the given list with items matching the given category.
        /// Note: this returns the current state of matching items in the catalog.  To ensure
        /// that there are no invalid or duplicate entries, the 'items' list will always 
        /// be cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="categoryHash">The id hash of the CategoryDefinition we want to check for.</param>
        /// <param name="items">The list to clear and write matching items into.</param>
        public void GetItemDefinitionsByCategory(int categoryHash, List<T3> items)
        {
            if (items == null)
            {
                return;
            }

            items.Clear();

            items.AddRange(GetItemDefinitionsByCategory(categoryHash));
        }

        /// <summary>
        /// This will return an array of ItemDefinitions with the designated Category by CategoryDefinition CategoryDefinition.
        /// </summary>
        /// <param name="category">The Category we want to check for.</param>
        /// <returns>An array of ItemDefinitions that contain the given Category.</returns>
        public T3[] GetItemDefinitionsByCategory(CategoryDefinition category)
        {
            if (category == null)
            {
                return null;
            }

            return GetItemDefinitionsByCategory(category.hash);
        }

        /// <summary>
        /// Fills in the given list with items matching the given category.
        /// Note: this returns the current state of matching items in the catalog.  To ensure
        /// that there are no invalid or duplicate entries, the 'items' list will always 
        /// be cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="category">The Category we want to check for.</param>
        /// <param name="items">The list to clear and write matching items into.</param>
        public void GetItemDefinitionsByCategory(CategoryDefinition category, List<T3> items)
        {
            if (items == null)
            {
                return;
            }

            items.Clear();

            if (category == null)
            {
                return;
            }

            items.AddRange(GetItemDefinitionsByCategory(category));
        }
        
        /// <summary>
        /// This gets the DefaultCollectionDefinitions by Id string.
        /// </summary>
        /// <param name="defaultDefinitionId">The Id of the DefaultCollectionDefinition we want.</param>
        /// <returns>Reference to the DefaultCollectionDefinition requested.</returns>
        public DefaultCollectionDefinition GetDefaultCollectionDefinition(string defaultDefinitionId)
        {
            if (string.IsNullOrEmpty(defaultDefinitionId))
            {
                return null;
            }
            
            return GetDefaultCollectionDefinition(Tools.StringToHash(defaultDefinitionId));
        }

        /// <summary>
        /// This gets the DefaultCollectionDefinition by Hash.
        /// </summary>
        /// <param name="defaultDefinitionHash">The Hash of the DefaultCollectionDefinition we want.</param>
        /// <returns>Reference to the DefaultCollectionDefinition requested.</returns>
        public DefaultCollectionDefinition GetDefaultCollectionDefinition(int defaultDefinitionHash)
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
        /// Returns an array of all default collection definitions.
        /// </summary>
        /// <returns>An array of all default collection definitions.</returns>
        public DefaultCollectionDefinition[] GetDefaultCollectionDefinitions()
        {
            return m_DefaultCollectionDefinitions?.ToArray();
        }

        /// <summary>
        /// Fills the given list with all default collection definitions in this catalog.
        /// Note: this returns the current state of all default collection definitions in the catalog.  To
        /// ensure that there are no invalid or duplicate entries, the 'defaultCollectionDefinitions' list 
        /// will always be cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="defaultCollectionDefinitions">The list to clear and write all default collection definitions to.</param>
        public void GetDefaultCollectionDefinitions(List<DefaultCollectionDefinition> defaultCollectionDefinitions)
        {
            if (defaultCollectionDefinitions == null)
            {
                return;
            }

            defaultCollectionDefinitions.Clear();

            if (m_DefaultCollectionDefinitions == null)
            {
                return;
            }

            defaultCollectionDefinitions.AddRange(m_DefaultCollectionDefinitions);
        }

        /// <summary>
        /// Check if the given Hash is available to be added to CollectionDefinitions.
        /// </summary>
        /// <param name="collectionDefinitionHash">The Hash we are checking for.</param>
        /// <returns>True/False whether or not Hash is available for use.</returns>
        public bool IsCollectionDefinitionHashUnique(int collectionDefinitionHash)
        {
            return GetCollectionDefinition(collectionDefinitionHash) == null;
        }

        /// <summary>
        /// Check if the given Hash is not yet within ItemDefinitions and is available for use.
        /// </summary>
        /// <param name="itemDefinitionHash">The Hash we are checking for.</param>
        /// <returns>True/False whether or not Hash is available for use.</returns>
        public bool IsItemDefinitionHashUnique(int itemDefinitionHash)
        {
            return GetItemDefinition(itemDefinitionHash) == null;
        }
    }
}
