using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Represents the set of items that need to be paid with for a transaction.
    /// </summary>
    public class Price
    {
        private List<InputItem> m_InputItems;
        
        /// <summary>
        /// Identification for this price point
        /// </summary>
        public string name { get; }

        /// <summary>
        /// Array of InputItems to be paid out.
        /// </summary>
        public InputItem[] GetInputItems()
        {
            if (m_InputItems == null)
                return null;

            InputItem[] items = new InputItem[m_InputItems.Count];
            m_InputItems.CopyTo(items, 0);
            return items;
        }

        /// <summary>
        /// Constructor for defining a price with multiple items of input.
        /// </summary>
        /// <param name="inputItems">The input items for this price.</param>
        /// <param name="name">name of this price point</param>
        public Price(List<InputItem> inputItems, string name = "default")
        {
            m_InputItems = new List<InputItem>();
            foreach (InputItem inputItem in inputItems)
            {
                m_InputItems.Add(new InputItem(inputItem));
            }
            this.name = name;
        }

        /// <summary>
        /// Constructor for defining a price with only a single input item.
        /// Many prices will be simple like this so this constructor avoids the need of passing a whole list.
        /// </summary>
        /// <param name="inputItem">The input of this price.</param>
        /// <param name="name">name of this price point</param>
        public Price(InputItem inputItem, string name = "default")
        {
            m_InputItems = new List<InputItem>();
            m_InputItems.Add(new InputItem(inputItem));
            this.name = name;
        }

        /// <summary>
        /// Copy constructor for setting up a new Price based off on a existing one.
        /// </summary>
        /// <param name="other">The other price to base this one off of.</param>
        public Price(Price other)
        {
            m_InputItems = new List<InputItem>();
            foreach (InputItem inputItem in other.m_InputItems)
            {
                m_InputItems.Add(new InputItem(inputItem));
            }
            name = other.name;
        }
    }
}
