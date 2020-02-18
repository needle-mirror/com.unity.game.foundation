namespace UnityEngine.GameFoundation.CatalogManagement
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
            return "Currency details can be attached to GameItemDefinitions and InventoryItemDefinitions. They specify the currency type and let you set special currency conditions.";
        }
        
        /// <summary>
        /// Creates a runtime CurrencyDetailDefinition based on this editor-time CurrencyDetailDefinition.
        /// </summary>
        /// <returns>Runtime version of CurrencyDetailDefinition.</returns>
        public override UnityEngine.GameFoundation.BaseDetailDefinition CreateRuntimeDefinition()
        {
            UnityEngine.GameFoundation.CurrencyDetailDefinition.CurrencySubType runtimeCurrencySubType;

            var runtimeCurrencyType = currencyType == CurrencyType.Hard ?
                UnityEngine.GameFoundation.CurrencyDetailDefinition.CurrencyType.Hard : UnityEngine.GameFoundation.CurrencyDetailDefinition.CurrencyType.Soft;

            switch (currencySubType)
            {
                case CurrencySubType.Event:
                    runtimeCurrencySubType = UnityEngine.GameFoundation.CurrencyDetailDefinition.CurrencySubType.Event;
                    break;
                case CurrencySubType.Social:
                    runtimeCurrencySubType = UnityEngine.GameFoundation.CurrencyDetailDefinition.CurrencySubType.Social;
                    break;
                case CurrencySubType.Other:
                    runtimeCurrencySubType = UnityEngine.GameFoundation.CurrencyDetailDefinition.CurrencySubType.Other;
                    break;
                case CurrencySubType.None:
                    runtimeCurrencySubType = UnityEngine.GameFoundation.CurrencyDetailDefinition.CurrencySubType.None;
                    break;
                default:
                    Debug.LogWarning("Trying to create runtime currency detail with an unsupported editor currency subtype " + currencySubType);
                    runtimeCurrencySubType = UnityEngine.GameFoundation.CurrencyDetailDefinition.CurrencySubType.None;
                    break;
            }

            return new UnityEngine.GameFoundation.CurrencyDetailDefinition(runtimeCurrencyType, runtimeCurrencySubType);
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
