using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    [Serializable]
    public class EditorPayout
    {
        [SerializeField]
        private List<EditorOutputItem> m_OutputItems;

        /// <summary>
        /// List of OutputItems to be paid out.
        /// </summary>
        public List<EditorOutputItem> outputItems => new List<EditorOutputItem>(m_OutputItems);

        /// <summary>
        /// Constructor for setting up a payout with multiple outputs.
        /// </summary>
        /// <param name="outputItems">The outputs this payout will give.</param>
        public EditorPayout(List<EditorOutputItem> outputItems = null)
        {
            m_OutputItems = outputItems;
        }

        /// <summary>
        /// Constructor for setting up a payout with a single output.
        /// Many payouts will be simple 1 items so this constructor allows easy setup for that without need of an external list.
        /// </summary>
        /// <param name="outputItem">The item this payout will give.</param>
        public EditorPayout(EditorOutputItem outputItem)
        {
            m_OutputItems = new List<EditorOutputItem> {outputItem};
        }
    }
}
