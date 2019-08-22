using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Anything that goes into a Collection should be an Item, and
    /// Items should only exist in Collections.
    /// </summary>
    /// <typeparam name="T3">The type of ItemDefinitions this BaseItem uses.</typeparam>
    /// <typeparam name="T4">The type of Items this BaseItem uses.</typeparam>
    public abstract class BaseItem<T3, T4> : GameItem
        where T3 : BaseItemDefinition<T3, T4>
        where T4 : BaseItem<T3, T4>
    {
        protected BaseItem(T3 definition) : base(definition, definition.id)
        {
            m_Definition = definition;
        }

        [SerializeField] 
        protected new T3 m_Definition;

        /// <summary>
        /// The ItemDefinition this Item is based on.
        /// </summary>
        /// <returns>The ItemDefinition this Item is based on.</returns>
        public new T3 definition
        {
            get { return m_Definition; }
        }

        /// <summary>
        /// The ItemDefinition id (string) this Item is based on.
        /// </summary>
        /// <returns>ItemDefinition id this Item is based on.</returns>
        public string definitionId
        {
            get { return m_Definition.id; }
        }

        [SerializeField]
        protected int m_intValue;

        /// <summary>
        /// The integer value associated with this Item, usually signifying quantity of Items in a Collection.
        /// </summary>
        /// <returns>The integer value associated with this Item.</returns>
        public int intValue
        {
            get { return m_intValue; }
            internal set { m_intValue = value; }
        }

        /// <summary>
        /// Gets the specified DetailsDefinition attached to this Item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The specified DetailsDefinition attached to this Item.</returns>
        public T GetDetailsDefinition<T>() where T : BaseDetailsDefinition
        {
            return m_Definition?.GetDetailsDefinition<T>();
        }
    }
}
