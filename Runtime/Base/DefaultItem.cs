namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This struct contains a pair of data, a GameItemDefinition hash and an int for quantity.
    /// These are used to setup the DefaultItems in a CollectionDefinition.
    /// </summary>
    [System.Serializable]
    public struct DefaultItem
    {
        /// <summary>
        /// The hash of the GameItemDefinition to use.
        /// </summary>
        public int definitionHash;

        /// <summary>
        /// The starting quantity of the Item instance once setup.
        /// </summary>
        public int quantity;

        /// <summary>
        /// Basic constructor that takes in fields for all variables this struct uses.
        /// </summary>
        /// <param name="definitionHash">The GameItemDefinition hash to use.</param>
        /// <param name="quantity">The starting quantity of this Item.</param>
        public DefaultItem(int definitionHash, int quantity)
        {
            this.definitionHash = definitionHash;
            this.quantity = quantity;
        }
    }
}
