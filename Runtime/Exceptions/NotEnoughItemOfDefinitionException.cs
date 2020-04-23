using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    /// Thrown during a process manipulating the wallet when the player tries to
    /// spend more than he has.
    /// </summary>
    public class NotEnoughItemOfDefinitionException : Exception
    {
        /// <summary>
        /// The id of the currency.
        /// </summary>
        public readonly string definitionId;

        /// <summary>
        /// The expected balance.
        /// </summary>
        public readonly long expectedCount;

        /// <summary>
        /// The actual balance.
        /// </summary>
        public readonly long actualCount;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="NotEnoughItemOfDefinitionException"/> class.
        /// </summary>
        /// <param name="definitionId">The identifier of the
        /// <see cref="InventoryItemDefinition"/>.</param>
        /// <param name="expectedCount">The expected number of the given
        /// <see cref="InventoryItemDefinition"/>.</param>
        /// <param name="actualCount">The available number of items in the
        /// <see cref="InventoryManager"/></param>
        internal NotEnoughItemOfDefinitionException
            (string definitionId, long expectedCount, long actualCount)
        {
            this.definitionId = definitionId;
            this.expectedCount = expectedCount;
            this.actualCount = actualCount;
        }

        /// <inheritdoc />
        public override string Message
            => $"Not enough item of definition {definitionId}. Expected: {expectedCount}, found: {actualCount}";
    }
}
