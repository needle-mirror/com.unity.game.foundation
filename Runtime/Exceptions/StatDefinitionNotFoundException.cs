using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    /// Thrown when a <see cref="StatDefinition"/> is in an item but not found.
    /// </summary>
    public class StatDefinitionNotFoundException : Exception
    {
        /// <summary>
        /// The identifier of the item where the stat diodn't have been found.
        /// </summary>
        public readonly string itemId;

        /// <summary>
        /// The identifier of the <see cref="StatDefinition"/> nott found.
        /// </summary>
        public readonly string statDefinitionId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statDefinitionId"></param>
        public StatDefinitionNotFoundException(string itemId, string statDefinitionId)
        {
            this.itemId = itemId;
            this.statDefinitionId = statDefinitionId;
        }

        /// <inheritdoc/>
        public override string Message =>
            $"{nameof(StatDefinition)} {statDefinitionId} not found in item {itemId}";
    }
}
