using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Describes a currency.
    /// </summary>
    public class Currency : CatalogItem, IEquatable<Currency>
    {
        /// <summary>
        /// Tells whether the currency is <see cref="CurrencyType.Soft"/> or <see cref="CurrencyType.Hard"/>.
        /// </summary>
        public CurrencyType type { get; internal set; }

        /// <summary>
        /// The maximum balance the player can have. 0 means no limit.
        /// </summary>
        public long maximumBalance { get; internal set; }

        /// <inheritdoc/>
        bool IEquatable<Currency>.Equals(Currency other)
        {
            return key == other.key;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => key.GetHashCode();

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is Currency other)
            {
                return key == other.key;
            }

            return false;
        }

        /// <inheritdoc/>
        public override string ToString() => $"[{nameof(Currency)} {key} ({type})]";
    }
}
