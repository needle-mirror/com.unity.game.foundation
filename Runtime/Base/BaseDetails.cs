namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Details are used to track runtime modifications to a Collections and Items.  
    /// If desired, helper methods can be added to a individual Details, as needed.
    /// Important note: Details must ALWAYS be constructed using DetailsDefinitions 
    /// which is the same pattern used for all ‘runtime’ versions of other Definitions as well.
    /// </summary>
    public abstract class BaseDetails     //TODO API: (also see todo below) consider <T> for basebase or baseitem or basedetails   OR  use BaseItemDetails : BaseDetails  and  BaseCollectionDetails : BaseDetails so you can derive from either of THREE base classes
    {
        // pointer to GameItem that owns this Details
        protected GameItem m_Owner;

        /// <summary>
        /// The GameItem that this Details is attached to. May be castable to either a BaseItem or BaseCollection.
        /// </summary>
        /// <returns>The GameItem that this Details is attached to.</returns>
        public GameItem owner
        {
            get { return m_Owner; }
            internal set { m_Owner = value; }
        }        //TODO API: even if neither above tech works, you COULD POSSIBLY use this so dev can say details.ownerItem  VS   details.ownerCollection and let us do the cast for him
	
        // definition used to create this Details (or null)           
        private BaseDetailsDefinition m_Definition;

        /// <summary>
        /// Retrieve a reference to the DetailsDefinition used to make this Details.
        /// </summary>
        /// <returns>DetailsDefinition associated with this Details.</returns>
        public BaseDetailsDefinition definition
        {
            get { return m_Definition; } 
        }

        /// <summary>
        /// Creates BaseDetails with information about its owner and DetailsDefinition it was made from. 
        /// Use DetailsDefinition.CreateDetails() if you need to create a Details.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="def"></param>
        protected BaseDetails(GameItem owner, BaseDetailsDefinition def)
        {
            m_Definition = def;
            m_Owner = owner;
        }
    }
}
