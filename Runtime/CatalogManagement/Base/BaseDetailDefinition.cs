namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// BaseDetailDefinition are used to modify CollectionDefinitions or 
    /// ItemDefinitions with constant values.  They may or may not create 
    /// runtime versions of themselves (i.e. BaseDetail) based on the 
    /// need for non-constant values.  
    /// </summary>
    public abstract class BaseDetailDefinition : ScriptableObject
    {
        // pointer to BaseItemDefinition OR BaseCollectionDefinition
        private GameItemDefinition m_Owner;

        /// <summary>
        /// The GameItemDefinition this DetailDefinition is attached to. Can be cast to either a BaseItemDefinition
        /// or BaseCollectionDefinition.
        /// </summary>
        /// <returns>The GameItemDefinition this DetailDefinition is attached to.</returns>
        public GameItemDefinition owner
        {
            get { return m_Owner; }
            internal set { m_Owner = value; }
        }

        /// <summary>
        /// Returns 'friendly' display name for this DetailDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this DetailDefinition.</returns>
        public abstract string DisplayName();

        /// <summary>
        /// Creates a runtime detail definition based on this editor-time detail definition.
        /// </summary>
        /// <returns>The runtime version of this DetailDefinition.</returns>
        public abstract UnityEngine.GameFoundation.BaseDetailDefinition CreateRuntimeDefinition();

        /// <summary>
        /// Returns string message which explains the purpose of this DetailDefinition, for the purpose of displaying as a tooltip in editor.
        /// </summary>
        /// <returns>The string tooltip message of this DetailDefinition.</returns>
        public virtual string TooltipMessage()
        {
            return string.Empty;
        }
    }
}
