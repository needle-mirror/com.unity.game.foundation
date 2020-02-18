using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This class contains a pair of data, a GameItemDefinition Hash and an int for quantity.
    /// These are used to setup the DefaultItems in a CollectionDefinition.
    /// </summary>
    public class DefaultItemDefinition
    {
        /// <summary>
        /// The Hash of the GameItemDefinition to use.
        /// </summary>
        public int definitionHash { get; }

        /// <summary>
        /// The ID the GameItemDefinition to use.
        /// </summary>
        public string definitionId { get; }

        /// <summary>
        /// The starting quantity of the Item instance once setup.
        /// </summary>
        public int quantity { get; }

        /// <summary>
        /// Basic constructor that takes in fields for all variables this class uses.
        /// </summary>
        /// <param name="definitionId">The id of the GameItemDefinition to use.</param>
        /// <param name="quantity">The starting quantity of this Item.</param>
        /// <exception cref="System.ArgumentException">Throws if definitionId are null or empty.</exception>
        internal DefaultItemDefinition(string definitionId, int quantity)
        {
            if (string.IsNullOrEmpty(definitionId))
            {
                throw new ArgumentException("DefaultItemDefinition cannot have null or empty definitionId.");
            }

            this.definitionId = definitionId;
            this.quantity = quantity;
            definitionHash = Tools.StringToHash(definitionId);
        }
    }
}
