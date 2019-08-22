using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// DefaultCollectionDefinitions contain preset values and rules for a 
    /// Collection by using a CollectionDefinition. During runtime, it may 
    /// be useful to refer back to the DefaultCollectionDefinition for the 
    /// presets and rules, but the values cannot be changed at runtime (your 
    /// system may, for example, bypass the presets, or calculate new values 
    /// on the fly with modifiers).
    /// </summary>
    /// <typeparam name="T1">The type of CollectionDefinitions this DefaultCollectionDefinition uses.</typeparam>
    /// <typeparam name="T2">The type of Collections this DefaultCollectionDefinition uses.</typeparam>
    /// <typeparam name="T3">The type of ItemDefinitions this DefaultCollectionDefinition uses.</typeparam>
    /// <typeparam name="T4">The type of Items this DefaultCollectionDefinition uses.</typeparam>
    public abstract class DefaultCollectionDefinition<T1, T2, T3, T4>
        where T1 : BaseCollectionDefinition<T1, T2, T3, T4>
        where T2 : BaseCollection<T1, T2, T3, T4>
        where T3 : BaseItemDefinition<T3, T4>
        where T4 : BaseItem<T3, T4>
    {
        protected DefaultCollectionDefinition(string id, string displayName, T1 baseCollectionDefinition)
        {
            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("DefaultCollectionDefinition can only be alphanumeric with optional dashes or underscores.");
            }
            
            m_DisplayName = displayName;
            m_Id = id;
            m_Hash = Tools.StringToHash(m_Id);
            m_CollectionDefinition = baseCollectionDefinition;
        }

        [SerializeField]
        protected string m_Id;

        /// <summary>x
        /// The string id of this DefaultCollectionDefinition.
        /// </summary>
        /// <returns>The string id of this DefaultCollectionDefinition.</returns>
        public string id
        {
            get { return m_Id; }
        }

        [SerializeField]
        protected int m_Hash;

        /// <summary>
        /// The hash of this DefaultCollectionDefinition's id.
        /// </summary>
        /// <returns>The hash of this DefaultCollectionDefinition's id.</returns>
        public int hash
        {
            get { return m_Hash; }
        }

        [SerializeField]
        protected string m_DisplayName;

        /// <summary>
        /// The name of this DefaultCollectionDefinition for the user to display.
        /// </summary>
        /// <returns>The name of this DefaultCollectionDefinition for the user to display.</returns>
        public string displayName
        {
            get { return m_DisplayName; }
            set { m_DisplayName = value; }
        }

        protected T1 m_CollectionDefinition;

        /// <summary>
        /// CollectionDefinition used for this DefaultCollectionDefinition.
        /// </summary>
        /// <returns>CollectionDefinition used for this DefaultCollectionDefinition.</returns>
        public T1 collectionDefinition
        {
            get { return m_CollectionDefinition; }
        }
    }
}
