using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Data;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    ///     Asset version of a definition for inventory items.
    /// </summary>
    public sealed partial class InventoryItemDefinitionAsset : CatalogItemAsset
    {
        /// <summary>
        /// Determines how many of this <see cref="InventoryItemDefinition"/>
        /// to automatically add to player's inventory.
        /// </summary>
        [SerializeField]
        int m_InitialAllocation = 0;

        /// <summary>
        /// Determines how many of this <see cref="InventoryItemDefinition"/>
        /// to automatically add to player's inventory.
        /// </summary>
        public int initialAllocation
        {
            get { return m_InitialAllocation; }
            internal set { m_InitialAllocation = value; }
        }

        [SerializeField]
        List<string> m_PropertyKeys;

        [SerializeField]
        List<Property> m_PropertyDefaultValues;

        /// <summary>
        ///     Stores all properties default values for inventory items.
        ///     Key: Property's key.
        ///     Value: Property's type & default value.
        /// </summary>
        internal Dictionary<string, Property> properties { get; }
            = new Dictionary<string, Property>();

        /// <summary>
        ///     Get all default properties stored in this definition.
        /// </summary>
        /// <returns>
        ///     Return a list of <see cref="PropertyData"/>
        ///     for each properties stored in this definition.
        ///     The returned list is never null. 
        /// </returns>
        public List<PropertyData> GetDefaultProperties()
        {
            var defaultProperties = new List<PropertyData>(properties.Count);
            foreach (var propertyEntry in properties)
            {
                var data = new PropertyData
                {
                    key = propertyEntry.Key,
                    value = propertyEntry.Value
                };
                defaultProperties.Add(data);
            }

            return defaultProperties;
        }

        /// <inheritdoc />
        protected override CatalogItemConfig ConfigureItem(CatalogBuilder builder)
        {
            var item = builder.Create<InventoryItemDefinitionConfig>(key);
            foreach (var property in properties)
            {
                item.properties.Add(property.Key, property.Value);
            }

            return item;
        }

        /// <inheritdoc />
        protected override void OnBeforeItemSerialize()
        {
            m_PropertyKeys = new List<string>(properties.Keys);
            m_PropertyDefaultValues = new List<Property>(properties.Values);
        }

        /// <inheritdoc />
        protected override void OnAfterItemDeserialize()
        {
            DeserializeListsToDictionary(m_PropertyKeys, m_PropertyDefaultValues, properties);
        }
    }
}
