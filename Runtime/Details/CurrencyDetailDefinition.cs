namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// CurrencyDetailDefinition.  Attach to a GameItemDefinition to store the currency information.
    /// </summary>
    /// <inheritdoc/>
    public class CurrencyDetailDefinition : BaseDetailDefinition
    {
        /// <summary>
        /// Returns 'friendly' display name for this CurrencyDetailDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this CurrencyDetailDefinition.</returns>
        public override string DisplayName() { return "Currency Detail"; }

        /// <summary>
        /// Returns string message which explains the purpose of this CurrencyDetailDefinition,
        /// for the purpose of displaying as a tooltip in editor.
        /// </summary>
        /// <returns>The string tooltip message of this CurrencyDetailDefinition.</returns>
        public override string TooltipMessage()
        {
            return "The currency detail can be attached to GameItemDefinitions and InventoryItemDefinitions to attach special currency conditions to the item, and allow specification of what type of currency they are. This allows them to be added to the wallet inventory and tracked as currency when the analytics detail is also attached.";
        }

        /// <summary>
        /// This better enables identifying and tracking different types of currency in your game.
        /// </summary>
        public enum CurrencyType
        {
            /// <summary>
            /// Also called "regular currency" or "free currency", is a resource designed to be adequately
            /// accessible through normal gameplay, without having to make micro-transactions.
            /// </summary>
            Soft,
            
            /// <summary>
            /// Also called "premium currency", is a resource that is exclusively, or near exclusively,
            /// acquired by paying for it (real-money transactions).  Premium currencies are much harder to
            /// acquire without making purchases, and thus are considered to be premium game content.
            /// </summary>
            Hard,
        }

        /// <summary>
        /// Another level of refinement for describing/tracking a currency.
        /// Any currency type can be combined with any currency sub-type.
        /// </summary>
        public enum CurrencySubType
        {
            /// <summary>
            /// Not categorized by any sub-type.
            /// </summary>
            None,

            /// <summary>
            /// Event currencies are typically gained through time limited special events (e.g. a holiday event).
            /// </summary>
            Event,

            /// <summary>
            /// Social currencies are typically gained through interactions between other players.
            /// </summary>
            Social,

            /// <summary>
            /// For use when you want to sub-categorize a currency,
            /// but it still doesn't fit into Event or Social categories.
            /// </summary>
            Other,
        }

        [SerializeField]
        CurrencyType m_CurrencyType;

        /// <summary>
        /// Currency type for this CurrencyDetailDefinition.
        /// </summary>
        /// <returns>Currency type for this CurrencyDetailDefinition.</returns>
        public CurrencyType currencyType
        {
            get { return m_CurrencyType; }
            internal set { m_CurrencyType = value; }
        }

        [SerializeField]
        CurrencySubType m_CurrencySubType;

        /// <summary>
        /// Currency sub-type for this CurrencyDetailDefinition.
        /// </summary>
        /// <returns>Currency sub-type for this CurrencyDetailDefinition.</returns>
        public CurrencySubType currencySubType
        {
            get { return m_CurrencySubType; }
            internal set { m_CurrencySubType = value; }
        }
    }
}
