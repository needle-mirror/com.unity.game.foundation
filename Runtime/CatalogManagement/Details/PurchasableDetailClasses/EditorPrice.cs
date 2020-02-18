using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    [Serializable]
    public class EditorPrice
    {
        [SerializeField]
        private string m_Name;
        
        /// <summary>
        /// Identification for this price point
        /// </summary>
        public string name
        {
            get => m_Name;
            private set => m_Name = value;
        }

        [SerializeField]
        private List<EditorInputItem> m_InputItems;
        /// <summary>
        /// List of payment items to be paid.
        /// </summary>
        public List<EditorInputItem> inputItems
        {
            get => new List<EditorInputItem>(m_InputItems);
            private set => m_InputItems = value;
        }

        /// <summary>
        /// Constructor for defining a price with multiple items of input.
        /// </summary>
        /// <param name="inputItems">The input items for this price.</param>
        public EditorPrice(List<EditorInputItem> inputItems = null, string name = "default")
        {
            m_InputItems = inputItems;
            m_Name = name;
        }

        /// <summary>
        /// Constructor for defining a price with only a single input item.
        /// Many prices will be simple like this so this constructor avoids the need of passing a whole list.
        /// </summary>
        /// <param name="inputItem">The input of this price.</param>
        public EditorPrice(EditorInputItem inputItem, string name = "default")
        {
            m_InputItems = new List<EditorInputItem> {inputItem};
            m_Name = name;
        }
    }
}
