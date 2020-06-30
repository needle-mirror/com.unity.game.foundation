using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Data;
using UnityEngine.Serialization;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Base class for most of the static data in Game Foundation.
    /// </summary>
    public abstract partial class CatalogItemAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <inheritdoc cref="displayName"/>
        [SerializeField]
        internal string m_DisplayName;

        /// <summary>
        /// The readable name of this <see cref="CatalogItemAsset"/> instance.
        /// It is used to make the Editor more comfortable, but it can also be
        /// used at runtime to display a readable information to the players.
        /// </summary>
        public string displayName => m_DisplayName;

        /// <inheritdoc cref="key"/>
        [SerializeField, FormerlySerializedAs("m_Id")]
        internal string m_Key;

        /// <summary>
        /// The identifier of the <see cref="CatalogItem"/> constructed from this asset.
        /// </summary>
        public string key => m_Key;

        /// <summary>
        /// The <see cref="TagAsset"/> instances this item is linked to.
        /// Those tags are stored in the same catalog than the one storing
        /// this item.
        /// </summary>
        [SerializeField]
        internal List<TagAsset> m_Tags;

        /// <inheritdoc cref="catalog"/>
        [SerializeField, HideInInspector]
        internal BaseCatalogAsset m_Catalog;

        /// <summary>
        /// Reference to the catalog of this item.
        /// </summary>
        public BaseCatalogAsset catalog => m_Catalog;

        [SerializeField]
        List<string> m_StaticPropertyKeys;

        [SerializeField]
        List<Property> m_StaticPropertyValues;

        /// <summary>
        ///     Stores all properties default values for inventory items.
        ///     Key: Property's key.
        ///     Value: Property's type & default value.
        /// </summary>
        internal Dictionary<string, Property> staticProperties { get; }
            = new Dictionary<string, Property>();

        /// <summary>
        /// The serialized list of details of this item.
        /// </summary>
        [SerializeField]
        List<BaseDetailAsset> m_DetailValues = new List<BaseDetailAsset>();

        /// <summary>
        /// The details of this item.
        /// </summary>
        internal Dictionary<Type, BaseDetailAsset> m_Details;

        /// <summary>
        /// Returns an array of all the <see cref="TagAsset"/> instances
        /// linked to this <see cref="CatalogItemAsset"/> instance.
        /// catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetTags(ICollection{TagAsset})"/> instead.
        /// </remarks>
        /// <returns>An array of all the tags linked to this
        /// <see cref="CatalogItemAsset"/> instance.</returns>
        public TagAsset[] GetTags() => m_Tags.ToArray();

        /// <summary>
        /// Fills the given <paramref name="target"/> collection with all the
        /// <see cref="TagAsset"/> instances linked to this
        /// <see cref="CatalogItemAsset"/> instance.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <see cref="TagAsset"/> instances.</param>
        /// <returns>The number of <see cref="TagAsset"/> instances linked
        /// to this <see cref="CatalogItemAsset"/> instance.</returns>
        public int GetTags(ICollection<TagAsset> target)
        {
            GFTools.ThrowIfArgNull(target, nameof(target));
            return GFTools.Copy(m_Tags, target);
        }

        /// <summary>
        /// Returns an array of all detail definitions on this game item
        /// definition.
        /// </summary>
        /// <returns>An array of all detail definitions on this game item
        /// definition.</returns>
        public BaseDetailAsset[] GetDetails()
        {
            var copy = new BaseDetailAsset[m_Details.Count];
            m_Details.Values.CopyTo(copy, 0);
            return copy;
        }

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all the
        /// <see cref="BaseDetailAsset"/> instance of this
        /// <see cref="CatalogItemAsset"/> instance.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <see cref="BaseDetailAsset"/> instances.</param>
        /// <return>The number of <see cref="BaseDetailAsset"/> of this
        /// <see cref="CatalogItemAsset"/> instance.</return>
        public int GetDetails(ICollection<BaseDetailAsset> target)
        {
            GFTools.ThrowIfArgNull(target, nameof(target));
            return GFTools.Copy(m_Details.Values, target);
        }

        /// <summary> 
        /// Gets the <typeparamref name="TDetailAsset"/> instance by its type.
        /// </summary>
        /// <typeparam name="TDetailAsset">The type of detail
        /// requested.</typeparam>
        /// <returns>The <typeparamref name="TDetailAsset"/> instance stored in
        /// this <see cref="CatalogItemAsset"/> instance.</returns>
        public TDetailAsset GetDetail<TDetailAsset>()
            where TDetailAsset : BaseDetailAsset
        {
            var type = typeof(TDetailAsset);
            m_Details.TryGetValue(type, out var detail);
            return detail as TDetailAsset;
        }

        /// <summary>
        /// Looks for a <see cref="TagAsset"/>, linked to this
        /// <see cref="CatalogItemAsset"/> instance, by its <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="TagAsset.key"/> of the
        /// <see cref="Tag"/> to find.</param>
        /// <returns>If found, returns the <see cref="TagAsset"/> instance,
        /// othewise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/>
        /// cannot be null as a <see cref="TagAsset.key"/> cannot be
        /// null.</exception>
        public TagAsset FindTag(string key)
        {
            GFTools.ThrowIfArgNull(key, nameof(key));

            foreach (var tag in m_Tags)
            {
                if (tag.key == key) return tag;
            }

            return null;
        }

        /// <summary>
        /// Tells whether or not the given <paramref name="tag"/> is within
        /// this <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="tag">The <see cref="TagAsset"/> instance. to
        /// search for.</param>
        /// <returns>Whether or not this <see cref="CatalogItemAsset"/> instance
        /// has the specified <see cref="TagAsset"/> included.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="tag"/> is <c>null</c>.</exception>
        public bool HasTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));
            return m_Tags.Contains(tag);
        }

        /// <summary>
        /// Tells whether or not a <see cref="TagAsset"/> instance with the
        /// given <paramref name="key"/> is within this
        /// <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="key">The identifier of a <see cref="TagAsset"/>
        /// instance.</param>
        /// <returns>Whether or not this <see cref="CatalogItemAsset"/> instance
        /// has a <see cref="Tag"/> with the given <paramref name="key"/>
        /// included.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="key"/> is <c>null</c>.</exception>
        public bool HasTag(string key) => FindTag(key) != null;

        /// <summary>
        ///     Get all static properties stored in this item.
        /// </summary>
        /// <returns>
        ///     Return a list of <see cref="PropertyData" />
        ///     for each properties stored in this item.
        ///     The returned list is never null.
        /// </returns>
        public List<PropertyData> GetStaticProperties()
        {
            var propertiesData = new List<PropertyData>(staticProperties.Count);
            foreach (var propertyEntry in staticProperties)
            {
                var data = new PropertyData
                {
                    key = propertyEntry.Key,
                    value = propertyEntry.Value
                };
                propertiesData.Add(data);
            }

            return propertiesData;
        }

        /// <summary>
        /// Initializes the internal collections.
        /// </summary>
        protected void Awake()
        {
            if (m_Details is null)
            {
                m_Details = new Dictionary<Type, BaseDetailAsset>();
            }

            if (m_Tags is null)
            {
                m_Tags = new List<TagAsset>();
            }

            AwakeDefinition();
        }

        /// <summary>
        /// Overriden by inherited classes to initialize specific members.
        /// </summary>
        protected virtual void AwakeDefinition() { }

        /// <summary>
        /// Called before serialization, this will copy over all keys and values
        /// from the <see cref="m_Details"/> dictionary into their serializable
        /// lists <see cref="m_DetailValues"/>.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_DetailValues.Clear();

            foreach (var detailEntry in m_Details)
            {
                m_DetailValues.Add(detailEntry.Value);
            }

            m_StaticPropertyKeys = new List<string>(staticProperties.Keys);
            m_StaticPropertyValues = new List<Property>(staticProperties.Values);

            OnBeforeItemSerialize();
        }

        /// <summary>
        /// Called at the end of <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>.
        /// Enable inheritance to add specific serialization process.
        /// </summary>
        protected virtual void OnBeforeItemSerialize() { }

        /// <summary>
        /// Called after serialization, this will pull out the
        /// <see cref="BaseDetailAsset"/> rom the list and store them into the
        /// <see cref="m_Details"/> dictionary.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_Details = new Dictionary<Type, BaseDetailAsset>();

            for (int i = 0; i < m_DetailValues.Count;)
            {
                if (m_DetailValues[i] != null)
                {
                    m_Details.Add(m_DetailValues[i].GetType(), m_DetailValues[i]);
                    i++;
                }
                else
                {
                    m_DetailValues.RemoveAt(i);
                }
            }

            DeserializeListsToDictionary(m_StaticPropertyKeys, m_StaticPropertyValues, staticProperties);

            OnAfterItemDeserialize();
        }

        /// <summary>
        /// Called at the end of <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>.
        /// Enable inheritance to add specific deserialization process.
        /// </summary>
        protected virtual void OnAfterItemDeserialize() { }

        /// <summary>
        /// Configures a specified <paramref name="builder"/> with this item.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        internal void Configure(CatalogBuilder builder)
        {
            var item = ConfigureItem(builder);
            item.displayName = displayName;
            foreach (var property in staticProperties)
            {
                item.staticProperties.Add(property.Key, property.Value);
            }

            foreach (var tagAsset in m_Tags)
            {
                item.tags.Add(tagAsset.key);
            }

            foreach (var detailAsset in m_Details.Values)
            {
                var detailConfig = detailAsset.CreateConfig();
                item.details.Add(detailConfig);
            }
        }

        /// <summary>
        /// Configures a specified <paramref name="builder"/> with the specifics
        /// of this item.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        /// <returns>The item config.</returns>
        protected abstract CatalogItemConfig ConfigureItem(CatalogBuilder builder);

        /// <summary>
        ///     Deserialize the given lists into the given dictionary.
        /// </summary>
        /// <param name="keys">
        ///     A list of keys.
        ///     It is cleared after the dictionary has been filled.
        /// </param>
        /// <param name="values">
        ///     A list of values.
        ///     It is cleared after the dictionary has been filled.
        /// </param>
        /// <param name="container">
        ///     The dictionary that will be filled with serialized data.
        ///     It is cleared even if the given lists are empty.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the argument is null.
        /// </exception>
        /// <exception cref="SerializationException">
        ///     Thrown if the given lists have a different item count.
        /// </exception>
        protected static void DeserializeListsToDictionary<TKey, TValue>(List<TKey> keys, List<TValue> values, Dictionary<TKey, TValue> container)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.Clear();

            var itemCount = keys.Count;
            if (itemCount != values.Count)
                throw new SerializationException(
                    "An error occured during the deserialization of this object. It contains corrupted data.");

            if (itemCount <= 0)
                return;

            for (var i = 0; i < itemCount; i++)
            {
                var key = keys[i];
                var value = values[i];

                container[key] = value;
            }

            //Clear lists to avoid storing duplicated data.
            keys.Clear();
            values.Clear();
        }
    }
}
