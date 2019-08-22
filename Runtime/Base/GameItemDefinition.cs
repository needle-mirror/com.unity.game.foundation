using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Base class for both BaseItemDefinition and BaseCollectionDefinition. 
    /// Holds id, dsplay name, etc., and allows DetailsDefinitions to be attached as needed.
    /// </summary>
    public class GameItemDefinition : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] 
        protected string m_DisplayName;

        /// <summary>
        /// The name of this GameItemDefinition for the user to display.
        /// </summary>
        /// <returns>The name of this GameItemDefinition for the user to display.</returns>
        public string displayName
        {
            get { return m_DisplayName; }
            set { SetDisplayName(value); }
        }

        private void SetDisplayName(string name)
        {
            Tools.ThrowIfPlayMode("Cannot set the display name of a GameItemDefinition while in play mode.");

            m_DisplayName = name;
        }
        
        [SerializeField]
        protected string m_Id;

        /// <summary>
        /// The string id of this GameItemDefinition.
        /// </summary>
        /// <returns>The string id of this GameItemDefinition.</returns>
        public string id
        {
            get { return m_Id; }
        }

        [SerializeField] 
        protected int m_Hash;

        /// <summary>
        /// The hash of this GameItemDefinition's id.
        /// </summary>
        /// <returns>The hash of this GameItemDefinition's id.</returns>
        public int hash
        {
            get { return m_Hash; }
        }

        [SerializeField]
        protected GameItemDefinition m_ReferenceDefinition;

        /// <summary>
        /// The reference GameItemDefinition for this GameItemDefinition.
        /// </summary>
        /// <returns>The reference GameItemDefinition for this GameItemDefinition.</returns>
        public GameItemDefinition referenceDefinition
        {
            get { return m_ReferenceDefinition; }
            set { SetReferenceDefinition(value); }
        }

        protected void SetReferenceDefinition(GameItemDefinition referenceDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot set the reference GameItemDefinition of a GameItemDefinition while in play mode.");
            
            if (m_ReferenceDefinition != referenceDefinition)
            {
                if (referenceDefinition == this)
                {
                    throw new ArgumentException("GameItemDefinition cannot point to itself.");
                }
                m_ReferenceDefinition = referenceDefinition;
            }
        }

        [SerializeField]
        internal List<int> m_Categories = new List<int>();

        [SerializeField]
        protected List<BaseDetailsDefinition> m_DetailsDefinitionValues = new List<BaseDetailsDefinition>();
        
        protected Dictionary<Type, BaseDetailsDefinition> m_DetailsDefinitions = new Dictionary<Type, BaseDetailsDefinition>();

        /// <summary>
        /// Iterator for iterating through the Categories on this GameItemDefinition.
        /// </summary>
        /// <returns>Iterator for iterating through the Categories on this GameItemDefinition.</returns>
        public IEnumerable<CategoryDefinition> categories
        {
            get { return QueryCategories(); }
        }

        private List<CategoryDefinition> QueryCategories()
        {
            List<CategoryDefinition> actualCategories = new List<CategoryDefinition>();
            foreach (int categoryHash in m_Categories)
            {
                CategoryDefinition category = GetCategoryDefinition(categoryHash);

                if (category == null)
                {
                    continue;
                }
                    
                actualCategories.Add(category);
            }
            return actualCategories;
        }

        protected virtual CategoryDefinition GetCategoryDefinition(int hash)
        {
            return GameFoundationSettings.gameItemCatalog.GetCategory(hash);
        }

        /// <summary>
        /// Adds the given Category to this GameItemDefinition.
        /// </summary>
        /// <param name="definition">The CategoryDefinition to add.</param>
        /// <returns>Whether or not adding the Category was successful.</returns>
        /// <exception cref="ArgumentException">Thrown if the given category is already on this definition.</exception>
        public bool AddCategory(CategoryDefinition definition)
        {
            Tools.ThrowIfPlayMode("Cannot add a CategoryDefinition to a GameItemDefinition while in play mode.");

            if (definition == null)
            {
                return false;
            }

            if (m_Categories.Contains(definition.hash))
            {
                throw new ArgumentException("Cannot add a duplicate category definition.");
            }
            
            m_Categories.Add(definition.hash);
            return true;
        }

        /// <summary>
        /// Adds the given Categories to this GameItemDefinition by list.
        /// </summary>
        /// <param name="categories">The list of CategoryDefinitions to add.</param>
        public bool AddCategories(List<CategoryDefinition> categories)
        {
            Tools.ThrowIfPlayMode("Cannot add CategoryDefinitions to a GameItemDefinition while in play mode.");
            
            if (categories == null)
            {
                return false;
            }
            
            foreach (CategoryDefinition category in categories)
            {
                AddCategory(category);
            }

            return true;
        }

        /// <summary>
        /// Returns the Category at the given index.
        /// </summary>
        /// <param name="index">The index to look at.</param>
        /// <returns>The CategoryDefinition id hash at specified index</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of range.</exception>
        public int GetCategoryByIndex(int index)
        {
            if (index < 0 || index >= m_Categories.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return m_Categories[index];
        }

        /// <summary>
        /// Returns the index of requested Category by CategoryDefinition id hash, or -1 if the CategoryDefinition is not found.
        /// </summary>
        /// <param name="category">The CategoryDefinition's index to return.</param>
        /// <returns>The index of requested Category by CategoryDefinition id hash, or -1 if not found.</returns>
        public int GetIndexOfCategory(int category)
        {
            return m_Categories.IndexOf(category);
        }

        /// <summary>
        /// Removes the given Category from this GameItemDefinition.
        /// </summary>
        /// <param name="definition">The CategoryDefinition to remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        public bool RemoveCategory(CategoryDefinition definition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a Category from a GameItemDefinition while in play mode.");

            if (definition == null)
            {
                return false;
            }

            return m_Categories.Remove(definition.hash);
        }

        /// <summary>
        /// Returns the number of Categories on this GameItemDefinition.
        /// </summary>
        /// <returns>The number of Categories on this GameItemDefinition.</returns>
        public int categoryCount
        {
            get { return m_Categories.Count; }
        }

        /// <summary>
        /// Iterator for iterating through the DetailsDefinitions attached to this GameItemDefinition.
        /// </summary>
        /// <returns>An iterator for iterating through the DetailsDefinitions attached to this GameItemDefinition.</returns>
        public IEnumerable<BaseDetailsDefinition> detailsDefinitions
        {
            get
            {
                if (m_DetailsDefinitions == null)
                {
                    return null;
                }
                
                return m_DetailsDefinitions.Values;
            }
        }

        /// <summary>
        /// This will add the given DetailsDefinition to this GameItemDefinition.
        /// </summary>
        /// <param name="detailsDefinition">The DetailsDefinition to add.</param>
        /// <returns>A reference to the DetailsDefinition that was just added.</returns>
        /// <exception cref="ArgumentException">Thrown if the given details definition is already on this game item.</exception>
        public BaseDetailsDefinition AddDetailsDefinition(BaseDetailsDefinition detailsDefinition) 
        {
            Tools.ThrowIfPlayMode("Cannot add a DetailsDefinition to a GameItemDefinition during play mode.");

            if (detailsDefinition == null)
            {
                Debug.LogWarning("Null details definition given, this will not be added to the definition.");
                return null;
            }

            if (m_DetailsDefinitions == null)
            {
                m_DetailsDefinitions = new Dictionary<Type, BaseDetailsDefinition>();
            }

            // if the Details already exists then throw
            var detailsDefinitionType = detailsDefinition.GetType();
            if (m_DetailsDefinitions.ContainsKey(detailsDefinitionType))
            {
                throw new ArgumentException(string.Format("The definition \"{0}\" already has a {1} details.", m_Id, detailsDefinitionType.Name));
            }

            detailsDefinition.owner = this;

            m_DetailsDefinitions.Add(detailsDefinitionType, detailsDefinition);

            // naming convention for details objects: "{ gameItem id }_{ details type name }"
            detailsDefinition.name = string.Format("{0}_{1}", this.m_Id, detailsDefinitionType.Name);

#if UNITY_EDITOR
            if (EditorUtility.IsPersistent(this))
            {
                AssetDatabase.AddObjectToAsset(detailsDefinition, this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif

            return detailsDefinition;
        }

        /// <summary>
        /// This will add a DetailsDefinition of specified type to this GameItemDefinition.
        /// </summary>
        /// <typeparam name="T">The type of the new DetailsDefinition to add.</typeparam>
        /// <returns>A reference to the DetailsDefinition that was just added.</returns>
        public T AddDetailsDefinition<T>() where T : BaseDetailsDefinition 
        {
            var newDetailsDefinition = CreateInstance<T>();

            AddDetailsDefinition(newDetailsDefinition);

            return newDetailsDefinition;
        }

        /// <summary>
        /// Returns the DetailsDefinition at the requested index.
        /// </summary>
        /// <param name="index">The index of the DetailsDefinition to return.</param>
        /// <returns>The DetailsDefinition at the requested index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the given index is out of range.</exception>
        public BaseDetailsDefinition GetDetailsDefinitionByIndex(int index)
        {
            if (index < 0 || index >= m_DetailsDefinitions.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            int counter = 0;
            foreach (BaseDetailsDefinition baseDetailsDefinition in m_DetailsDefinitions.Values)
            {
                if (counter >= index)
                {
                    return baseDetailsDefinition;
                }

                counter++;
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary> 
        /// This will return a reference to the requested DetailsDefinition by type.
        /// </summary>
        /// <typeparam name="T">The type of DetailsDefinition requested.</typeparam>
        /// <returns>A reference to the DetailsDefinition, or null if this GameItemDefinition does not have one.</returns>
        public T GetDetailsDefinition<T>(bool lookInReferenceDefinition = true)
            where T : BaseDetailsDefinition
        {
            if (m_DetailsDefinitions != null && m_DetailsDefinitions.ContainsKey(typeof(T)))
            {
                return m_DetailsDefinitions[typeof(T)] as T;
            }

            if (lookInReferenceDefinition && referenceDefinition != null)
            {
                return referenceDefinition.GetDetailsDefinition<T>();
            }

            return null;
        }

        /// <summary>
        /// Returns the index of the given DetailsDefinition, or -1 if not found.
        /// </summary>
        /// <param name="detailsDefinition">The DetailsDefinition to find.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if detailsDefinition was null</exception>
        /// <returns>The index of the requested DetailsDefinition, or -1 if not found.</returns>
        public int GetIndexOfDetailsDefinition(BaseDetailsDefinition detailsDefinition)
        {
            if (detailsDefinition == null)
            {
                throw new IndexOutOfRangeException("Input argument " + nameof(detailsDefinition) + " was null.");
            }
            
            int counter = 0;
            foreach (BaseDetailsDefinition baseDetailsDefinition in m_DetailsDefinitions.Values)
            {
                if (baseDetailsDefinition == detailsDefinition)
                {
                    return counter;
                }

                counter++;
            }

            return -1;
        }

        /// <summary> 
        /// Remove the requested DetailsDefinition by type from this GameItemDefinition.
        /// </summary>
        /// <typeparam name="T">The type of DetailsDefinition we want to remove.</typeparam>
        public bool RemoveDetailsDefinition<T>()
            where T : BaseDetailsDefinition
        {
            Tools.ThrowIfPlayMode("Cannot remove a DetailsDefinition from a GameItemDefinition during play mode.");

            var detailsDefinitionToRemove = GetDetailsDefinition<T>(false);

            if (detailsDefinitionToRemove == null)
            {
                return false;
            }

            return RemoveDetailsDefinition(detailsDefinitionToRemove);
        }

        /// <summary>
        /// Removes the specified DetailsDefinition from this GameItemDefinition.
        /// </summary>
        /// <param name="detailsDefinition">DetailsDefinition to remove from this GameItemDefinition.</param>
        /// <returns>Whether or not the given details was successfully removed.</returns>
        public bool RemoveDetailsDefinition(BaseDetailsDefinition detailsDefinition)
        {
            Tools.ThrowIfPlayMode("Cannot remove a DetailsDefinition from a GameItemDefinition during play mode.");

            if (detailsDefinition == null)
            {
                return false;
            }

            var detailsType = detailsDefinition.GetType();
            if (!m_DetailsDefinitions.ContainsKey(detailsType))
            {
                return false;
            }

            m_DetailsDefinitions.Remove(detailsType);

#if UNITY_EDITOR
            if (EditorUtility.IsPersistent(this))
            {
                AssetDatabase.RemoveObjectFromAsset(detailsDefinition);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
#endif
            return true;
        }

        private int RemoveAllDetailsDefinitions()
        {
            if (Application.isPlaying)
            {
                throw new System.Exception("Cannot remove DetailsDefinitions from a GameItemDefinition during play mode.");
            }

            int count = m_DetailsDefinitions.Count;

            // if any DetailsDefinitions are actually attached
            if (count > 0)
            {
#if UNITY_EDITOR
                // remove them from the asset database
                foreach(var detailsDefinitionToRemove in m_DetailsDefinitions)
                {
                    AssetDatabase.RemoveObjectFromAsset(detailsDefinitionToRemove.Value);
                }
#endif

                // clear the list
                m_DetailsDefinitions.Clear();

                // save updated asset database
#if UNITY_EDITOR
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#endif
            }

            return count;
        }

        internal void OnRemove()
        {
            if (Application.isPlaying)
            {
                throw new System.Exception("GameItemDefinitions cannot be removed during play mode.");
            }

            RemoveAllDetailsDefinitions();
        }

        /// <summary>
        /// Returns the number of DetailsDefinitions attached to this GameItemDefinition.
        /// </summary>
        /// <returns>The number of DetailsDefinitions attached to this GameItemDefinition.</returns>
        public int detailsDefinitionCount
        {
            get { return m_DetailsDefinitions.Count; }
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
        /// Called before serialization, this will copy over all keys and values from 
        /// the DetailsDefinitions dictionary into their serializable lists.
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_DetailsDefinitionValues.Clear();
            
            foreach (var kv_Details in m_DetailsDefinitions)
            {
                m_DetailsDefinitionValues.Add(kv_Details.Value);
            }
        }

        /// <summary>
        /// Called after serialization, this will pull out the DetailsDefinition keys and values from the lists and store them into the main dictionary.
        /// </summary>
        public void OnAfterDeserialize()
        {
            m_DetailsDefinitions = new Dictionary<Type, BaseDetailsDefinition>();

            for (int i = 0; i < m_DetailsDefinitionValues.Count; i++)
            {
                m_DetailsDefinitions.Add(m_DetailsDefinitionValues[i].GetType(), m_DetailsDefinitionValues[i]);
            }
        }

        /// <summary>
        /// Creates a new GameItemDefinition by id and displayName.
        /// </summary>
        /// <param name="id">The id this GameItemDefinition will use.</param>
        /// <param name="displayName">The display name this GameItemDefinition will use.</param>
        /// <returns>The newly created GameItemDefinition.</returns>
        public static GameItemDefinition Create(string id, string displayName)
        {
            Tools.ThrowIfPlayMode("Cannot create a GameItemDefinition while in play mode.");
            
            GameItemDefinition gameItem = CreateInstance<GameItemDefinition>();
            gameItem.Initialize(id, displayName);

            return gameItem;
        }
        
        protected virtual void Initialize(string id, string displayName)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentException("GameItemDefinition cannot have null or empty id.");
            }

            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("GameItemDefinition can only be alphanumeric with optional dashes or underscores.");
            }
            
            if (string.IsNullOrEmpty(displayName))
            {
                throw new System.ArgumentException("GameItemDefinition cannot have null or empty displayName.");
            }
            
            m_Id = id;
            m_DisplayName = displayName;
            m_Hash = Tools.StringToHash(id);
            name = displayName;
        }
    }
}
