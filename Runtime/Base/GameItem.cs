using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Common Fields found in BaseItem and BaseCollection. BaseItem and BaseCollection both inherit from this class.
    /// </summary>
    public class GameItem
    {
        /// <summary>
        /// Constructor for a GameItem.
        /// </summary>
        /// <param name="definition">The GameItemDefinition this GameItem should use.</param>
        /// <param name="id">The id this GameItem will use.</param>
        public GameItem(GameItemDefinition definition, string id = null)
        {
            // assign this gameItem a unique gameItem id and register it with the GameItem instance lookup class
            m_GameItemId = GameItemLookup.GetNextIdForInstance();
            GameItemLookup.RegisterInstance(m_GameItemId, this);

            // determine id and hash
            if (string.IsNullOrEmpty(id))
            {
                m_Id = Guid.NewGuid().ToString();
                m_Hash = Tools.StringToHash(m_Id);
            }
            else
            {
                if (!Tools.IsValidId(id))
                {
                    throw new System.ArgumentException("GameItem can only be alphanumeric with optional dashes or underscores.");
                }
                
                m_Id = id;
                m_Hash = Tools.StringToHash(m_Id);
            }

            if (definition == null)
            {
                m_Definition = null;
                m_Categories = new CategoryDefinition[] { };
            }
            else
            { 
                m_Definition = definition;
                if (definition.categories == null || definition.categoryCount == 0)
                {
                    m_Categories = new CategoryDefinition[] { };
                }
                else
                {
                    m_Categories = new CategoryDefinition[definition.categoryCount];
                    int counter = 0;
                    foreach (CategoryDefinition category in definition.categories)
                    {
                        m_Categories[counter] = category;
                        counter++;
                    }
                }

                m_DisplayName = definition.displayName;
                if (definition.detailsDefinitions != null)
                {
                    foreach (var detailsDefinition in definition.detailsDefinitions)
                    {
                        AddDetails(detailsDefinition);
                    }
                }
                if (definition.referenceDefinition != null && definition.referenceDefinition.detailsDefinitions != null)
                {
                    foreach (var detailsDefinition in definition.detailsDefinitions)
                    {
                        if (!m_Details.ContainsKey(detailsDefinition.GetType()))
                        {
                            AddDetails(detailsDefinition);
                        }
                    }
                }
            }

            // set all stats for stats details definitions in gameItem definition and all reference gameItem definitions
            SetDefaultStats();
        }

        private void SetDefaultStats()
        {
            // set all stats for stats details definitions in gameItem definition and all reference gameItem definitions
            var definitionOn = m_Definition;
            while (definitionOn != null)
            {
                if (definitionOn.detailsDefinitions != null)
                {
                    foreach (var detailsDefinition in definitionOn.detailsDefinitions)
                    {
                        var statDetailsDefinition = detailsDefinition as StatDetailsDefinition;
                        if (statDetailsDefinition != null)
                        {
                            if (statDetailsDefinition.statDefaultIntValues != null)
                            {
                                foreach (var kv in statDetailsDefinition.statDefaultIntValues)
                                {
                                    if (!StatManager.TryGetIntValue(this, kv.Key, out int dummy))
                                    {
                                        StatManager.SetIntValue(this, kv.Key, kv.Value);
                                    }
                                }
                            }
                            if (statDetailsDefinition.statDefaultFloatValues != null)
                            {
                                foreach (var kv in statDetailsDefinition.statDefaultFloatValues)
                                {
                                    if (!StatManager.TryGetFloatValue(this, kv.Key, out float dummy))
                                    {
                                        StatManager.SetFloatValue(this, kv.Key, kv.Value);
                                    }
                                }
                            }
                        }
                    }
                }

                definitionOn = definitionOn.referenceDefinition;
            }
        }

        // in finalizer, remove gameItem from gameItem instance lookup
        //TODO: this approach may need further consideration as finalizers are not called until gc so GameItems may remain in table indefinitely
        ~GameItem()
        {
            GameItemLookup.UnregisterInstance(m_GameItemId);
        }

        [SerializeField]
        protected int m_GameItemId;

        /// <summary>
        /// The GameItem id (unique thoughout game) for this GameItem.
        /// </summary>
        /// <returns>The GameItem id (unique thoughout game) for this GameItem.</returns>
        public int gameItemId
        {
            get { return m_GameItemId; }
        }

        [SerializeField] 
        protected string m_DisplayName;

        /// <summary>
        /// The name of this GameItem for the user to display.
        /// </summary>
        /// <returns>The name of this GameItem for the user to display.</returns>
        public string displayName
        {
            get { return m_DisplayName; }
        }

        [SerializeField]
        protected string m_Id;

        /// <summary>
        /// The string id of this GameItem.
        /// </summary>
        /// <returns>The id string for this GameItem.</returns>
        public string id
        {
            get { return m_Id; }
        }

        [SerializeField] 
        protected int m_Hash;

        /// <summary>
        /// The hash of this GameItem's id.
        /// </summary>
        /// <returns>The hash of this GameItem's id.</returns>
        public int hash
        {
            get { return m_Hash; }
        }

        [SerializeField]
        protected GameItemDefinition m_Definition;

        /// <summary>
        /// The GameItemDefinition for this GameItem.
        /// </summary>
        /// <returns>The GameItemDefinition for this GameItem.</returns>
        public GameItemDefinition definition
        {
            get { return m_Definition; }
        }

        [SerializeField]
        protected CategoryDefinition[] m_Categories;

        /// <summary>
        /// An array of all CategoryDefinitions assigned to this GameItem.
        /// </summary>
        /// <returns>An array of all CategoryDefinitions assigned to this GameItem.</returns>
        public CategoryDefinition[] categories
        {
            get { return m_Categories; }
        }

        [SerializeField] 
        protected Dictionary<Type,BaseDetails> m_Details = new Dictionary<Type, BaseDetails>();

        /// <summary>
        /// The Details attached to this GameItem.
        /// </summary>
        /// <returns>An enumerator for the Details attached to this GameItem.</returns>
        protected IEnumerable<BaseDetails> details
        {
            get { return m_Details.Values; }
        }

        /// <summary>
        /// This will add a Details instance to this GameItem based on the specified DetailsDefinition, if needed.
        /// </summary>
        /// <param name="detailsDefinition">The DetailsDefinition to create a Details from.</param>
        /// <returns>A reference to the Details instance that was added or null if no runtime Details is needed for the specified DetailsDefinition.</returns>
        protected BaseDetails AddDetails(BaseDetailsDefinition detailsDefinition)
        {
            if (detailsDefinition == null)
            {
                Debug.LogWarning("Null details definition given, this will not be added.");
                return null;
            }

            var createdDetails = detailsDefinition.CreateDetails(this);

            // above method only creates runtime details when they're needed--null signals runtime details not required
            if (createdDetails != null)
            {
                AddDetails(createdDetails);
            }
            return createdDetails;
        }

        /// <summary>
        /// This will add the given Details to the Details list for this GameItem.
        /// </summary>
        /// <param name="details">The Details to add to this GameItem.</param>
        /// <returns>A reference to the Details that was added.</returns>
        /// <exception cref="ArgumentException">Thrown if the given details is a duplicate.</exception>
        protected BaseDetails AddDetails(BaseDetails details)
        {
            if (details == null)
            {
                Debug.LogWarning("Null details given, this will not be added.");
                return null;
            }
            
            var type = details.GetType();

            if (m_Details.ContainsKey(type))
            {
                throw new ArgumentException("Cannot add a duplicate details.");
            }
            m_Details.Add(type, details);

            return details;
        }

        /// <summary>
        /// This will add a Details of the specified type to this GameItem.
        /// </summary>
        /// <typeparam name="T">Type of details to add.</typeparam>
        /// <returns>A reference to the newly-created Details that was added.</returns>
        protected T AddDetails<T>() where T : BaseDetails, new()
        {
            var newDetails = new T();
            newDetails.owner = this;
            return AddDetails(newDetails) as T;
        }

        /// <summary> 
        /// This will return a reference to the requested Details by type.
        /// </summary>
        /// <typeparam name="T">The type of Details to return.</typeparam>
        /// <returns>A reference to the Details or null if not found.</returns>
        protected T GetDetails<T>() where T : BaseDetails
        {
            var type = typeof(T);
            BaseDetails details;
            if (m_Details.TryGetValue(type, out details))
            {
                return (T)details;
            }

            return null;
        }

        /// <summary> 
        /// This will remove the requested Details (by Details type) from this GameItem.
        /// </summary>
        /// <typeparam name="T">The type of Details to remove.</typeparam>
        /// <returns>True if Details was successfully removed, else false.</returns>
        protected bool RemoveDetails<T>() where T : BaseDetails
        {
            var type = typeof(T);
            
            return m_Details.Remove(type);
        }
    }
}
