namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// IconDetailsDefinition.  Attach to a GameItemDefinition to store sprite information.
    /// </summary>
    public class IconDetailsDefinition : BaseDetailsDefinition
    {
        /// <summary>
        /// Returns 'friendly' display name for this IconDetailsDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this IconDetailsDefinition.</returns>
        public override string DisplayName() { return "Icon Details"; }

        [SerializeField]
        protected Sprite m_Icon;

        /// <summary>
        /// Actual icon (sprite) used by the GameItemDefinition upon which this IconDetailsDefinition is attached.
        /// </summary>
        /// <returns>Actual icon (sprite) used by the GameItemDefinition upon which this IconDetailsDefinition is attached.</returns>
        public Sprite icon
        {
            get { return m_Icon; }
            internal set { m_Icon = value; }
        }
    }
}
