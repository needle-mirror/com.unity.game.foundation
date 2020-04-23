using System;
using System.Collections.Generic;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// The catalog storing the <see cref="StatDefinitionAsset"/> instances.
    /// </summary>
    public sealed partial class StatCatalogAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The list of <see cref="StatDefinitionAsset"/> instances.
        /// </summary>
        [SerializeField]
        internal List<StatDefinitionAsset> m_StatDefinitions;

        /// <inheritdoc cref="database"/>
        [SerializeField, HideInInspector]
        internal GameFoundationDatabase m_Database;

        /// <summary>
        /// A reference to the database containing this
        /// <see cref="StatCatalogAsset"/>.
        /// </summary>
        public GameFoundationDatabase database => m_Database;


        /// <summary>
        /// Prevent from exposing a parameter-less constructor.
        /// </summary>
        internal StatCatalogAsset() { }

        /// <summary>
        /// Initializes this instance of the <see cref="StatCatalogAsset"/> class.
        /// </summary>
        void Awake()
        {
            if (m_StatDefinitions is null)
            {
                m_StatDefinitions = new List<StatDefinitionAsset>();
            }
        }


        /// <summary>
        /// Finds a <see cref="StatDefinitionAsset"/> by its
        /// <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Identifier of the <see cref="StatDefinitionAsset"/>
        /// to find.</param>
        /// <returns>StatDefinition for specified Stat Id</returns>
        public StatDefinitionAsset FindStatDefinition(string id)
        {
            GFTools.ThrowIfArgNull(id, nameof(id));

            foreach(var definition in m_StatDefinitions)
            {
                if (definition.id == id)
                {
                    return definition;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells whether or not the catalog contains a
        /// <see cref="StatDefinitionAsset"/> with the given <paramref name="id"/>
        /// </summary>
        /// <param name="id">Identifier of the <see cref="StatDefinitionAsset"/>
        /// instance to find.</param>
        /// <returns><c>true</c> if found, <c>false</c> othewise.</returns>
        public bool HasStatDefinition(string id)
            => FindStatDefinition(id);

        /// <summary>
        /// Returns an array of all the <see cref="StatDefinitionAsset"/>
        /// instances in this catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetStatDefinitions(ICollection{StatDefinitionAsset})"/>
        /// instead.
        /// </remarks>
        /// <returns>An array of all <see cref="StatDefinitionAsset"/>
        /// instances.</returns>
        public StatDefinitionAsset[] GetStatDefinitions()
            => m_StatDefinitions.ToArray();

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all the
        /// <see cref="StatDefinitionAsset"/> instance of this catalog.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <see cref="StatDefinitionAsset"/> instances.</param>
        /// <return>The number of <see cref="StatDefinitionAsset"/> of this
        /// catalog.</return>
        public int GetStatDefinitions
            (ICollection<StatDefinitionAsset> target = null)
            => GFTools.Copy(m_StatDefinitions, target);

        /// <summary>
        /// Configures the specified <paramref name="builder"/> with the content
        /// of this catalog.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        internal void Configure(CatalogBuilder builder)
        {
            foreach (var statAsset in m_StatDefinitions)
            {
                statAsset.Configure(builder);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (m_StatDefinitions is null) return;

            foreach (var definition in m_StatDefinitions)
            {
                if (definition is null) continue;
                definition.catalog = this;
            }
        }
    }
}
