namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// CurrencyDetailDefinition.  Attach to a GameItemDefinition to store the currency information.
    /// </summary>
    /// <inheritdoc/>
    public class CurrencyDetailDefinition : BaseDetailDefinition
    {
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

        /// <summary>
        /// Currency type for this CurrencyDetailDefinition.
        /// </summary>
        /// <returns>Currency type for this CurrencyDetailDefinition.</returns>
        public CurrencyType currencyType { get; }

        /// <summary>
        /// Currency sub-type for this CurrencyDetailDefinition.
        /// </summary>
        /// <returns>Currency sub-type for this CurrencyDetailDefinition.</returns>
        public CurrencySubType currencySubType { get; }

        /// <summary>
        /// Constructor to build a CurrencyDetailDefinition object.
        /// </summary>
        /// <param name="currencyType">The CurrencyType of this detail.</param>
        /// <param name="currencySubType">The CurrencySubType of this detail. Default value is CurrencySubType.None.</param>
        /// <param name="owner">The GameItemDefinition that is attached to this DetailDefinition.</param>
        internal CurrencyDetailDefinition(CurrencyType currencyType, CurrencySubType currencySubType = CurrencySubType.None, GameItemDefinition owner = null)
            : base(owner)
        {
            this.currencyType = currencyType;
            this.currencySubType = currencySubType;
        }
    }
}
