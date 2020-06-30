using System;
using System.Runtime.CompilerServices;
using UnityEngine.GameFoundation.Exceptions;
using UnityEngine.Internal;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Item handled by the <see cref="InventoryManager"/>. 
    /// </summary>
    public partial class InventoryItem : IEquatable<InventoryItem>, IComparable<InventoryItem>
    {
        /// <summary>
        /// The unique identifier of this item.
        /// </summary>
        string m_Id;

        /// <inheritdoc cref="m_Id" />
        /// <exception cref="NullReferenceException">
        /// If this item has been discarded.
        /// </exception>
        public string id
        {
            get
            {
                AssertActive();

                return m_Id;
            }
        }

        /// <summary>
        /// A session-wide identifier used by <seealso cref="ItemLookup"/> to ease internal searches.
        /// </summary>
        int m_InstanceId;

        /// <inheritdoc cref="m_InstanceId" />
        /// <exception cref="NullReferenceException">
        /// If this item has been discarded.
        /// </exception>
        internal int instanceId
        {
            get
            {
                AssertActive();

                return m_InstanceId;
            }
            set => m_InstanceId = value;
        }

        /// <summary>
        /// The definition used to create this item.
        /// </summary>
        InventoryItemDefinition m_Definition;

        /// <inheritdoc cref="m_Definition" />
        /// <exception cref="NullReferenceException">
        /// If this item has been discarded.
        /// </exception>
        public InventoryItemDefinition definition
        {
            get
            {
                AssertActive();

                return m_Definition;
            }
        }

        /// <summary>
        /// Determines if this item has been discarded (removed from Game Foundation).
        /// Items being standard objects, they cannot be destroyed
        /// and garbage collected as long as all their references are not set to <c>null</c>.
        ///
        /// This property is a way for you to know if
        /// the object is still active within Game Foundation.
        /// </summary>
        /// <returns>True if the item has been
        /// removed from Game Foundation.</returns>
        public bool hasBeenDiscarded { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InventoryItem"/> class.
        /// </summary>
        /// <param name="itemDefinition">
        /// The definition used to create the item.
        /// It stores all static data.
        /// </param>
        /// <param name="id">
        /// The unique identifier of this item.
        /// If <c>null</c>, the constructor will create one itself.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the given <paramref name="itemDefinition"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the given <paramref name="id"/> is not valid.
        /// </exception>
        /// <exception cref="PropertyInvalidCastException">
        /// If one of the given properties has a type different from its matching key in the definition.
        /// </exception>
        internal InventoryItem(InventoryItemDefinition itemDefinition, string id = null)
        {
            if (itemDefinition == null)
                throw new ArgumentNullException(nameof(itemDefinition));

            // determine Id
            if (string.IsNullOrEmpty(id))
            {
                m_Id = Guid.NewGuid().ToString();
            }
            else
            {
                if (!Tools.IsValidId(id))
                {
                    throw new ArgumentException(
                        "GameItem's id must be alphanumeric with dashes (-) and underscores (_) allowed.");
                }

                m_Id = id;
            }

            m_Definition = itemDefinition;
        }

        /// <summary>
        /// Throws a <see cref="NullReferenceException"/> if this
        /// <see cref="InventoryItem"/> is discarded.
        /// </summary>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AssertActive()
        {
            if (hasBeenDiscarded)
            {
                throw new NullReferenceException(
                    $"Item already disposed of for id: {m_Id}. Be sure to release all references to GameItems, Inventories and InventoryItems when they are removed or manager is reset.");
            }
        }

        /// <summary>
        /// Remove all references to this item.
        /// </summary>
        internal void Discard()
        {
            if (hasBeenDiscarded) return;

            CleanProperties();

            // remember the item was discarded so we don't do it again
            hasBeenDiscarded = true;
        }

        /// <inheritdoc />
        public override int GetHashCode() => id.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        bool IEquatable<InventoryItem>.Equals(InventoryItem other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        int IComparable<InventoryItem>.CompareTo(InventoryItem other)
            => id.CompareTo(other.id);
    }
}
