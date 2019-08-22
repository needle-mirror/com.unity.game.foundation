namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// BaseDetailsDefinition are used to modify CollectionDefinitions or 
    /// ItemDefinitions with constant values.  They may or may not create 
    /// runtime versions of themselves (i.e. BaseDetails) based on the 
    /// need for non-constant values.  
    /// </summary>
    public abstract class BaseDetailsDefinition : ScriptableObject
    {
        // pointer to BaseItemDefinition OR BaseCollectionDefinition
        [SerializeField]
        private GameItemDefinition m_Owner;

        /// <summary>
        /// The GameItemDefinition this DetailsDefinition is attached to. Can be cast to either a BaseItemDefinition
        /// or BaseCollectionDefinition.
        /// </summary>
        /// <returns>The GameItemDefinition this DetailsDefinition is attached to.</returns>
        public GameItemDefinition owner
        {
            get { return m_Owner; }
            internal set { m_Owner = value; }
        }

        /// <summary>
        /// Returns 'friendly' display name for this DetailsDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this DetailsDefinition.</returns>
        public abstract string DisplayName();

        // build runtime (instance) version of this DetailsDefinition
        // NOTE: this is not abstract because it's perfectly fine for a DetailsDefinition NOT …
        //   …    to have a runtime version of itself

        /// <summary>
        /// Method to create specific type of runtime Details, if needed.
        /// Note: don't override this method if the DetailsDefinition is static and you do not need to
        /// allow GameItem to modify its data independently at runtime.
        /// </summary>
        /// <param name="newOwner">GameItem that owns the Details that will be created (if any).</param>
        /// <returns>The runtime Details instance (if needed) or null if no runtime Details is required for this DetailsDefinition.</returns>
        public virtual BaseDetails CreateDetails(GameItem newOwner)
        {
            return null;
        }
    }
}
