using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Base class for both BaseItemDefinition and BaseCollectionDefinition. 
    /// Holds Id, display name, DetailDefinitions etc.
    /// </summary>
    public class GameItemDefinition
    {
        /// <summary>
        /// The readable name of this GameItemDefinition for displaying to users.
        /// </summary>
        /// <returns>The name of this GameItemDefinition for displaying to users.</returns>
        public string displayName { get; }

        /// <summary>
        /// The string Id of this GameItemDefinition.
        /// </summary>
        /// <returns>The string Id of this GameItemDefinition.</returns>
        public string id { get; }

        /// <summary>
        /// The Hash of this GameItemDefinition's Id.
        /// </summary>
        /// <returns>The Hash of this GameItemDefinition's Id.</returns>
        public int hash { get; }

        /// <summary>
        /// The reference GameItemDefinition for this GameItemDefinition.
        /// </summary>
        /// <returns>The reference GameItemDefinition for this GameItemDefinition.</returns>
        public GameItemDefinition referenceDefinition { get; }

        private readonly List<int> m_Categories;
        private readonly Dictionary<Type, BaseDetailDefinition> m_DetailDefinitions;

        /// <summary>
        /// Constructor to build a GameItemDefinition object.
        /// </summary>
        /// <param name="id">The string id value for this GameItemDefinition. Throws error if null, empty or invalid.</param>
        /// <param name="displayName">The readable string display name value for this GameItemDefinition. Throws error if null or empty.</param>
        /// <param name="referenceDefinition">The reference GameItemDefinition for this GameItemDefinition. Null is an allowed value.</param>
        /// <param name="categories">The list of CategoryDefinition hashes that are the categories applied to this GameItemDefinition. If null value is passed in an empty list will be created.</param>
        /// <param name="detailDefinitions">The dictionary of Type, BaseDetailDefinition pairs that are the detail definitions applied to this GameItemDefinition. If null value is passed in an empty dictionary will be created.</param>
        /// <exception cref="System.ArgumentException">Throws if id or displayName are null or empty or if the id is not valid. Valid ids are alphanumeric with optional dashes or underscores.</exception>
        internal GameItemDefinition(string id, string displayName, GameItemDefinition referenceDefinition = null, List<int> categories = null, Dictionary<Type, BaseDetailDefinition> detailDefinitions = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("GameItemDefinition cannot have null or empty id.");
            }

            if (!Tools.IsValidId(id))
            {
                throw new ArgumentException("GameItemDefinition must be alphanumeric. Dashes (-) and underscores (_) allowed.");
            }

            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentException("GameItemDefinition cannot have null or empty displayName.");
            }

            this.displayName = displayName;
            this.id = id;
            hash = Tools.StringToHash(id);
            this.referenceDefinition = referenceDefinition;
            m_Categories = categories ?? new List<int>();
            m_DetailDefinitions = detailDefinitions ?? new Dictionary<Type, BaseDetailDefinition>();
            foreach (BaseDetailDefinition detailDefinition in m_DetailDefinitions.Values)
            {
                detailDefinition.owner = this;
            }
        }

        /// <summary>
        /// Returns an array of all categories on this game item definition.
        /// </summary>
        /// <returns>An array of all categories on this game item definition.</returns>
        public CategoryDefinition[] GetCategories()
        {
            if (m_Categories == null)
                return null;

            List<CategoryDefinition> actualCategories = new List<CategoryDefinition>();
            foreach (int categoryHash in m_Categories)
            {
                CategoryDefinition category = GetCategoryDefinition(categoryHash);

                if (category != null)
                {
                    actualCategories.Add(category);
                }
            }

            return actualCategories.ToArray();
        }

        /// <summary>
        /// Fills the given list with all categories on this game item definition.
        /// Will clear any existing values from the list before adding new ones.
        /// </summary>
        /// <param name="categories">The list to fill up.</param>
        public void GetCategories(List<CategoryDefinition> categories)
        {
            if (categories == null)
            {
                return;
            }

            categories.Clear();

            if (m_Categories == null)
            {
                return;
            }

            foreach (int categoryHash in m_Categories)
            {
                CategoryDefinition category = GetCategoryDefinition(categoryHash);

                if (category != null)
                {
                    categories.Add(category);
                }
            }
        }

        /// <summary>
        /// Checks whether or not the given CategoryDefinition is within this GameItemDefinition.
        /// </summary>
        /// <param name="category">The Category to search for.</param>
        /// <returns>Whether or not this GameItemDefinition has the specified CategoryDefinition included.</returns>
        public bool HasCategoryDefinition(CategoryDefinition category)
        {
            if (category == null)
            {
                return false;
            }

            foreach (int currentCategoryHash in m_Categories)
            {
                if (currentCategoryHash == category.hash)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a CategoryDefinition from this GameItemDefinition categories with the following Hash 
        /// </summary>
        /// <param name="categoryHash">CategoryDefinition Hash of CategoryDefinition to get</param>
        /// <returns>Requested Category Definition</returns>
        protected virtual CategoryDefinition GetCategoryDefinition(int categoryHash)
        {
            return CatalogManager.gameItemCatalog.GetCategory(categoryHash);
        }

        /// <summary>
        /// Returns an array of all detail definitions on this game item definition.
        /// </summary>
        /// <returns>An array of all detail definitions on this game item definition.</returns>
        public BaseDetailDefinition[] GetDetailDefinitions()
        {
            if (m_DetailDefinitions == null)
            {
                return null;
            }

            // count how many entries are actually of the correct type (and NOT polymorphic entries)
            int count = 0;
            foreach(var kv in m_DetailDefinitions)
            {
                if (kv.Key == kv.Value.GetType())
                {
                    ++ count;
                }
            }

            // setup return array
            var baseDetailDefinitions = new BaseDetailDefinition[count];

            // fill the return array with the detail definitions of the exact type of key
            // note: this skips any 'polymorphic' entries which were added to allow base class types to find derived class entries
            count = 0;
            foreach (var kv in m_DetailDefinitions)
            {
                if (kv.Key == kv.Value.GetType())
                {
                    baseDetailDefinitions[count] = kv.Value;
                    ++ count;
                }
            }

            return baseDetailDefinitions;
        }

        /// <summary>
        /// Fills in the given list with all detail definitions on this game item definition.
        /// Note: this returns the current state of detail definitions.  To ensure that there
        /// are no invalid or duplicate entries, the 'detailDefinitions' list will always be 
        /// cleared and 'recycled' (i.e. updated) with current data from the catalog.
        /// </summary>
        /// <param name="detailDefinitions">The list to clear and fill with detail definitions.</param>
        public void GetDetailDefinitions(List<BaseDetailDefinition> detailDefinitions)
        {
            if (detailDefinitions == null)
            {
                return;
            }

            detailDefinitions.Clear();

            if (m_DetailDefinitions == null)
            {
                return;
            }

            // fill results list with only detail definitions the exactly match the type of their dictionary key
            // note: this skips all the 'polymorphic' entries which all base class types to find objects of derived classes
            foreach (var kv in m_DetailDefinitions)
            {
                if (kv.Key == kv.Value.GetType())
                {
                    detailDefinitions.Add(kv.Value);
                }
            }
        }

        /// <summary> 
        /// This will return a reference to the requested DetailDefinition by type.
        /// </summary>
        /// <param name="lookInReferenceDefinition">Whether or not to also check the reference definition for the requested detail. Defaults to true.</param>
        /// <typeparam name="T">The type of DetailDefinition requested.</typeparam>
        /// <returns>A reference to the DetailDefinition, or null if this GameItemDefinition does not have one.</returns>
        public T GetDetailDefinition<T>(bool lookInReferenceDefinition = true)
            where T : BaseDetailDefinition
        {
            if (m_DetailDefinitions != null && m_DetailDefinitions.ContainsKey(typeof(T)))
            {
                return m_DetailDefinitions[typeof(T)] as T;
            }

            if (lookInReferenceDefinition && !ReferenceEquals(referenceDefinition, null))
            {
                return referenceDefinition.GetDetailDefinition<T>();
            }

            return null;
        }
    }
}
