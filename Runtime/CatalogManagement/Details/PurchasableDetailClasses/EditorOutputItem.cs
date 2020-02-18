using System;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    [Serializable]
    public class EditorOutputItem
    {
        [SerializeField]
        private string m_DefinitionId;
        /// <summary>
        /// Get the ids relevant to this price reward
        /// </summary>
        public string definitionId => m_DefinitionId;

        [SerializeField]
        private int m_Quantity;
        /// <summary>
        /// Get the payout quantity.
        /// </summary>
        public int quantity => m_Quantity;

        [SerializeField]
        private string m_DestinationInventoryId;
        /// <summary>
        /// Returns the id of the destination inventory to dispense items into
        /// </summary>
        public string destinationInventoryId => m_DestinationInventoryId;

        /// <summary>
        /// Standard constructor for an output item, takes an id, quantity, and optional destination inventory.
        /// </summary>
        /// <param name="definitionId">The id of the item this gives.</param>
        /// <param name="quantity">The quantity of the item to give.</param>
        /// <param name="destinationInventoryId">The override inventory to try and put the item into.</param>
        public EditorOutputItem(string definitionId, int quantity, string destinationInventoryId = null)
        {
            m_DefinitionId = definitionId;
            m_Quantity = quantity;
            m_DestinationInventoryId = destinationInventoryId;
        }

        /// <summary>
        /// Outputs all main data of the output item into a single string.
        /// TODO: Definitely may want to improve this formatting once we're ready to release this.
        /// </summary>
        /// <returns>As above.</returns>
        public override string ToString()
        {
            string data = definitionId + "," + quantity;
            if (!string.IsNullOrEmpty(destinationInventoryId))
            {
                data += "," + destinationInventoryId;
            }

            return data;
        }
    } 
}
