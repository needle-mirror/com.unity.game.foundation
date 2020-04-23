using System;
using System.Runtime.CompilerServices;
using UnityEngine.Internal;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Base class for runtime instances in Game Foundation.
    /// </summary>
    public abstract class GameItem : IEquatable<GameItem>, IComparable<GameItem>
    {
        /// <summary>
        /// The unique id of the <see cref="GameItem"/> instance.
        /// </summary>
        internal readonly string m_Id;

        /// <summary>
        /// The local, session-wide instance id of the item.
        /// See <seealso cref="GameItemLookup"/>.
        /// </summary>
        internal int m_InstanceId;

        /// <summary>
        /// The definition of the <see cref="GameItem"/> instance.
        /// Gives access to the static data linked to this item.
        /// </summary>
        internal CatalogItem m_Definition;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameItem"/> class.
        /// It also registers this new instance to the
        /// <see cref="GameItemLookup"/> utility class for faster internal
        /// lookup.
        /// The result of this registration is that the <see cref="instanceId"/>
        /// property of this <see cref="GameItem"/> instance is being assigned
        /// a unique (session-wide) indentifier.
        /// </summary>
        /// <param name="definition">The <see cref="CatalogItem"/> storing the
        /// static data of this <see cref="GameItem"/> instance.</param>
        /// <param name="id">The unique identifier of this
        /// <see cref="GameItem"/> instance.
        /// If <c>null</c>, the constructor will create one itself.</param>
        /// <exception cref="ArgumentException">If the given
        /// <paramref name="id"/> is not valid.</exception>
        internal GameItem(CatalogItem definition, string id = null)
        {
            // determine Id
            if (string.IsNullOrEmpty(id))
            {
                m_Id = Guid.NewGuid().ToString();
            }
            else
            {
                if (!Tools.IsValidId(id))
                {
                    throw new ArgumentException("GameItem must be alphanumeric. Dashes (-) and underscores (_) allowed.");
                }

                m_Id = id;
            }

            GameItemLookup.Register(this);

            m_Definition = definition;
        }

        /// <summary>
        /// in finalizer, remove gameItem from gameItem instance lookup
        /// </summary>
        ~GameItem()
        {
            Discard();
        }

        /// <summary>
        /// The unique identifier of this <see cref="GameItem"/> instance.
        /// </summary>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        public string id => m_Id;

        /// <summary>
        /// A session-wide identifier that Game Foundation internals uses to
        /// speed up the look ups.
        /// </summary>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        internal int instanceId
        {
            get
            {
                AssertActive();
                return m_InstanceId;
            }
        }

        /// <summary>
        /// The dfinition of this <see cref="GameItem"/> instance.
        /// </summary>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        public CatalogItem definition => m_Definition;

        /// <summary>
        /// Determines if this <see cref="GameItem"/> instance has been
        /// discarded (removed from Game Foundation).
        /// <see cref="GameItem"/> instances being standard objets, they cannot
        /// be destroyed and garbage collected while all their references are
        /// not set to <c>null</c>.
        /// The <see cref="discarded"/> property is a way for you to know if the
        /// object is still active within Game Foundation.
        /// </summary>
        /// <returns>True if the <see cref="GameItem"/> instance has been
        /// removed from Game Foundation.</returns>
        public bool discarded { get; private set; }

        /// <summary>
        /// Event triggered right after this <see cref="GameItem"/> is removed.
        /// </summary>
        public event GameItemEventHandler removed;

        /// <summary>
        /// Event triggered when a stat is updated.
        /// </summary>
        public event StatChangedEventHandler statChanged;

        /// <summary>
        /// Triggers the <see cref="removed"/> event.
        /// </summary>
        internal void onRemoved() => removed?.Invoke(this);

        /// <summary>
        /// Triggers the <see cref="statChanged"/> event.
        /// </summary>
        /// <param name="definition">The <see cref="StatDefinition"/> whose
        /// value has changed.</param>
        /// <param name="value">The new value of the stat.</param>
        internal void onStatChanged(StatDefinition definition, StatValue value)
            => statChanged?.Invoke(this, definition, value);

        /// <summary>
        /// Throws a <see cref="NullReferenceException"/> if this
        /// <see cref="GameItem"/> is discarded.
        /// </summary>
        [ExcludeFromDocs]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AssertActive()
        {
            if (discarded)
            {
                throw new NullReferenceException
                    ($"Item already disposed of for id: {m_Id}. Be sure to release all references to GameItems, Inventories and InventoryItems when they are removed or manager is reset.");
            }
        }

        /// <summary>
        /// Initializes the <see cref="GameItem"/> instance.
        /// This method is called by Game Foundation during the item creation
        /// process.
        /// The instantiation is separated from the initialization because the
        /// StatManager needs to access this <see cref="GameItem"/> instance
        /// through the InventoryManager to validate the assignment of the
        /// stat values.
        /// So Game Foundation:
        /// - creates the item
        /// - adds the item into the internal collections
        /// - initializes the item
        ///   - initialization asks StatManager to initialize the stat values
        ///   - StatManager tries to find the item in the internal collections.
        /// </summary>
        internal void Initialize()
        {
            var statDetail = m_Definition.GetDetail<StatDetail>();
            if (statDetail == null) return;

            foreach (var kv in statDetail.m_DefaultValues)
            {
                var definition = Tools.GetStatDefinitionOrDie(kv.Key, "statDefinitionId");
                StatManager.SetValueInternal(this, definition, kv.Value);
            }
        }

        /// <summary>
        /// Remove all references to this <see cref="GameItem"/> instance.
        /// </summary>
        internal void Discard()
        {
            if (discarded) return;

            // remove all stats for this game item
            StatManager.OnGameItemDiscarded(this);

            GameItemLookup.Unregister(this);

            // remember the item was discarded so we don't do it again
            discarded = true;
        }

        /// <inheritdoc />
        public override int GetHashCode() => id.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        bool IEquatable<GameItem>.Equals(GameItem other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        int IComparable<GameItem>.CompareTo(GameItem other)
            => id.CompareTo(other.id);

        #region StatsHelpers

        /// <summary>
        /// Tells if this <see cref="GameItem"/> instance has a
        /// <see cref="StatDetail"/> with the given
        /// <paramref name="statDefinition"/> references within this detail.
        /// </summary>
        /// <param name="statDefinition">The <see cref="StatDefinition"/>
        /// instance to look for</param>
        /// <returns><c>true</c> if the <paramref name="statDefinition"/> is
        /// found, <c>false</c> otherwise.</returns>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="statDefinition"/> parameter is null.</exception>
        public bool HasStat(StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            AssertActive();

            var statDetail = definition.GetDetail<StatDetail>();
            if (statDetail is null) return false;

            return statDetail.HasStat(statDefinition);
        }

        /// <summary>
        /// Tells if this <see cref="GameItem"/> instance has a
        /// <see cref="StatDetail"/> with the given
        /// <paramref name="id"/> references within this detail.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="StatDefinition"/>
        /// to look for</param>
        /// <returns><c>true</c> if the <see cref="StatDefinition"/> is found
        /// within the <see cref="StatDetail"/> of this <see cref="GameItem"/>
        /// instance, <c>false</c> otherwise.</returns>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        /// <exception cref="ArgumentException">If the
        /// <paramref name="id"/> parameter is null, empty, or
        /// whitespace.</exception>
        /// <exception cref="ArgumentNullException">If there is no
        /// <see cref="StatDefinition"/> identified by this
        /// <paramref name="id"/></exception>
        public bool HasStat(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));
            AssertActive();

            var catalog = GameFoundation.catalogs.statCatalog;

            var statDefinition = catalog.FindStatDefinition(id);
            Tools.ThrowIfArgNull(statDefinition, nameof(id));

            return HasStat(statDefinition);
        }

        /// <summary>
        /// Gets the value of the given <paramref name="statDefinition"/>
        /// for this <see cref="GameItem"/> instance.
        /// </summary>
        /// <param name="statDefinition">The <see cref="StatDefinition"/> to get
        /// the value from.</param>
        /// <returns>The value of the <see cref="StatDefinition"/> in this
        /// <see cref="GameItem"/> instance.</returns>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        public StatValue GetStat(StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            AssertActive();
            return StatManager.GetValue(this, statDefinition);
        }

        /// <summary>
        /// Gets the value of the <see cref="StatDefinition"/> by its given
        /// <paramref name="id"/> for this <see cref="GameItem"/> instance.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="StatDefinition"/> to
        /// get the value.</param>
        /// <returns>The value of the <see cref="StatDefinition"/> in this
        /// <see cref="GameItem"/> instance.</returns>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        /// <exception cref="ArgumentNullException">If the <paramref name="id"/>
        /// parameter is null or empty.</exception>
        public StatValue GetStat(string statDefinitionId)
        {
            Tools.ThrowIfArgNullOrEmpty(statDefinitionId, nameof(statDefinitionId));
            AssertActive();
            return StatManager.GetValue(this, statDefinitionId);
        }

        /// <summary>
        /// Sets the value of the given <paramref name="statDefinition"/>
        /// for this <see cref="GameItem"/> instance.
        /// </summary>
        /// <param name="statDefinition">The <see cref="StatDefinition"/> to set
        /// a value for this <see cref="GameItem"/> instance.</param>
        /// <param name="value">The value to assign to the
        /// <paramref name="statDefinition"/> entry of this <see cref="GameItem"/>
        /// instance.</param>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="statDefinition"/> parameter is null.</exception>
        public void SetStat(StatDefinition statDefinition, StatValue value)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            AssertActive();
            StatManager.SetValue(this, statDefinition, value);
        }

        /// <summary>
        /// Sets the value of the <see cref="StatDefinition"/> by its given
        /// <paramref name="statDefinitionId"/> for this
        /// <see cref="GameItem"/> instance.
        /// </summary>
        /// <param name="statDefinitionId">Identifier of the
        /// <see cref="StatDefinition"/> to set a value to.</param>
        /// <param name="value">The value to assign to the
        /// <see cref="StatDefinition"/> entry of this <see cref="GameItem"/>
        /// instance.</param>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="statDefinitionId"/> parameter is null, empty or
        /// whitespace.</exception>
        public void SetStat(string statDefinitionId, StatValue value)
        {
            Tools.ThrowIfArgNullOrEmpty(statDefinitionId, nameof(statDefinitionId));
            AssertActive();
            StatManager.SetValue(this, statDefinitionId, value);
        }

        /// <summary>
        /// Adjusts the value of the <see cref="StatDefinition"/> by its given
        /// <paramref name="statDefinitionId"/> by adding the
        /// <paramref name="change"/> to its current value.
        /// </summary>
        /// <param name="statDefinitionId">Identifier of the
        /// <see cref="StatDefinition"/> to adjust.</param>
        /// <param name="change">Change to apply to the current value of the
        /// stat.</param>
        /// <returns>The new value of the stat.</returns>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> instance has been discarded.</exception>
        /// <exception cref="ArgumentException">If the
        /// <paramref name="statDefinitionId"/> is null, empty, or
        /// whitespace.</exception>
        /// <exception cref="InvalidOperationException">The parameter refers to
        /// a stat of a different type.</exception>
        /// <exception cref="ArgumentNullException">The
        /// <paramref name="statDefinitionId"/> parameter refers to a
        /// non-existing <see cref="StatDefinition"/>.</exception>
        public StatValue AdjustStat(string statDefinitionId, StatValue change)
        {
            Tools.ThrowIfArgNullOrEmpty(statDefinitionId, nameof(statDefinitionId));
            AssertActive();
            return StatManager.AdjustValue(this, statDefinitionId, change);
        }

        /// <summary>
        /// Adjusts the stat by specified amount.
        /// </summary>
        /// <param name="statDefinition">The stat to adjust.</param>
        /// <param name="change">Change in value to the Stat.</param>
        /// <returns>The new value of Stat on this GameItem.</returns>
        /// <exception cref="InvalidOperationException">The parameter refers to a stat of a different type.</exception>
        /// <exception cref="ArgumentNullException">The parameter is null or empty.</exception>
        public StatValue AdjustStat(StatDefinition statDefinition, StatValue change)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            AssertActive();
            return StatManager.AdjustValue(this, statDefinition, change);
        }

        /// <summary>
        /// Resets the stat to the default value of its given definition.
        /// </summary>
        /// <param name="statDefinition">The <see cref="StatDefinition"/> to
        /// reset for this <see cref="GameItem"/> instance.</param>
        /// <returns><c>true</c> if the stat is successfully reset, <c>false</c>
        /// otherwise.</returns>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> has been discarded</exception>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="statDefinition"/> is null.</exception>
        public StatValue ResetStat(StatDefinition statDefinition)
        {
            Tools.ThrowIfArgNull(statDefinition, nameof(statDefinition));
            AssertActive();
            return StatManager.ResetToDefaultValue(this, statDefinition);
        }

        /// <summary>
        /// Resets the stat to the default value of its given definition.
        /// </summary>
        /// <param name="statDefinitionId">The identifier of the
        /// <see cref="StatDefinition"/> to reset for this <see cref="GameItem"/>
        /// instance.</param>
        /// <returns><c>true</c> if the stat is successfully reset, <c>false</c>
        /// otherwise.</returns>
        /// <exception cref="NullReferenceException">If this
        /// <see cref="GameItem"/> has been discarded</exception>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="statDefinitionId"/> is null, empty, or
        /// whitespace.</exception>
        public StatValue ResetStat(string statDefinitionId)
        {
            Tools.ThrowIfArgNullOrEmpty(statDefinitionId, nameof(statDefinitionId));
            AssertActive();
            return StatManager.ResetToDefaultValue(this, statDefinitionId);
        }

        #endregion
    }
}
