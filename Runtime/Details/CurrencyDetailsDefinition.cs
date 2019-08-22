namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// CurrencyDetailsDefinition.  Attach to a GameItemDefinition to store the currency information.
    /// </summary>
    public class CurrencyDetailsDefinition : BaseDetailsDefinition
    {
        /// <summary>
        /// Returns 'friendly' display name for this CurrencyDetailsDefinition.
        /// </summary>
        /// <returns>The 'friendly' display name for this CurrencyDetailsDefinition.</returns>
        public override string DisplayName() { return "Currency Details"; }

        /// <summary>
        /// Enum for currency type.  Sets to soft, hard, special, event or other types of currency.
        /// </summary>
        public enum CurrencyType
        {
            Soft,
            Hard,
            Special,
            Event,
            Other,
        }

        [SerializeField]
        CurrencyType m_CurrencyType;

        /// <summary>
        /// Currency type for this CurrencyDetailsDefinition.  Soft, hard, special, event or other types of currency.
        /// </summary>
        /// <returns>Currency type for this CurrencyDetailsDefinition.</returns>
        public CurrencyType currencyType
        {
            get { return m_CurrencyType; }
            internal set { m_CurrencyType = value; }
        }
    }
}
