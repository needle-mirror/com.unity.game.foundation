using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;

using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
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

        /// <inheritdoc cref="id"/>
        [SerializeField]
        internal string m_Id;

        /// <summary>
        /// The string Id of this GameItemDefinition.
        /// </summary>
        /// <returns>The string Id of this GameItemDefinition.</returns>
        public string id => m_Id;

        /// <summary>
        /// The <see cref="CategoryAsset"/> instances this item is linked to.
        /// Those categories are stored in the same catalog than the one storing
        /// this item.
        /// </summary>
        [SerializeField]
        internal List<CategoryAsset> m_Categories;

        /// <inheritdoc cref="catalog"/>
        [SerializeField, HideInInspector]
        internal BaseCatalogAsset m_Catalog;

        /// <summary>
        /// Reference to the catalog of this item.
        /// </summary>
        public BaseCatalogAsset catalog => m_Catalog;

        /// <summary>
        /// Returns an array of all the <see cref="CategoryAsset"/> instances
        /// linked to this <see cref="CatalogItemAsset"/> instance.
        /// catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetCategories(ICollection{CategoryAsset})"/> instead.
        /// </remarks>
        /// <returns>An array of all the categories linked to this
        /// <see cref="CatalogItemAsset"/> instance.</returns>
        public CategoryAsset[] GetCategories() => m_Categories.ToArray();

        /// <summary>
        /// Fills the given <paramref name="target"/> collection with all the
        /// <see cref="CategoryAsset"/> instances linked to this
        /// <see cref="CatalogItemAsset"/> instance.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <see cref="CategoryAsset"/> instances.</param>
        /// <returns>The number of <see cref="CategoryAsset"/> instances linked
        /// to this <see cref="CatalogItemAsset"/> instance.</returns>
        public int GetCategories(ICollection<CategoryAsset> target)
        {
            GFTools.ThrowIfArgNull(target, nameof(target));
            return GFTools.Copy(m_Categories, target);
        }

        /// <summary>
        /// The serialized list of details of this item.
        /// </summary>
        [SerializeField]
        private List<BaseDetailAsset> m_DetailValues = new List<BaseDetailAsset>();

        /// <summary>
        /// The details of this item.
        /// </summary>
        internal Dictionary<Type, BaseDetailAsset> m_Details;

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
        /// Looks for a <see cref="CategoryAsset"/>, linked to this
        /// <see cref="CatalogItemAsset"/> instance, by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The <see cref="CategoryAsset.id"/> of the
        /// <see cref="Category"/> to find.</param>
        /// <returns>If found, returns the <see cref="CategoryAsset"/> instance,
        /// othewise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/>
        /// cannot be null as a <see cref="CategoryAsset.id"/> cannot be
        /// null.</exception>
        public CategoryAsset FindCategory(string id)
        {
            GFTools.ThrowIfArgNull(id, nameof(id));

            foreach (var category in m_Categories)
            {
                if (category.id == id) return category;
            }

            return null;
        }

        /// <summary>
        /// Tells whether or not the given <paramref name="category"/> is within
        /// this <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="category">The <see cref="CategoryAsset"/> instance. to
        /// search for.</param>
        /// <returns>Whether or not this <see cref="CatalogItemAsset"/> instance
        /// has the specified <see cref="CategoryAsset"/> included.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="category"/> is <c>null</c>.</exception>
        public bool HasCategory(CategoryAsset category)
        {
            GFTools.ThrowIfArgNull(category, nameof(category));
            return m_Categories.Contains(category);
        }

        /// <summary>
        /// Tells whether or not a <see cref="CategoryAsset"/> instance with the
        /// given <paramref name="id"/> is within this
        /// <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="id">The identifier of a <see cref="CategoryAsset"/>
        /// instance.</param>
        /// <returns>Whether or not this <see cref="CatalogItemAsset"/> instance
        /// has a <see cref="Category"/> with the given <paramref name="id"/>
        /// included.</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="id"/> is <c>null</c>.</exception>
        public bool HasCategory(string id) => FindCategory(id) != null;

        /// <summary>
        /// Initializes the internal collections.
        /// </summary>
        protected void Awake()
        {
            if (m_Details is null)
            {
                m_Details = new Dictionary<Type, BaseDetailAsset>();
            }

            if (m_Categories is null)
            {
                m_Categories = new List<CategoryAsset>();
            }

            AwakeDefinition();
        }

        /// <summary>
        /// Overriden by inherited classes to initialize specific members.
        /// </summary>
        protected virtual void AwakeDefinition()
        {
        }

        /// <summary>
        /// Called before serialization, this will copy over all keys and values
        /// from the <see cref="m_Details"/> dictionary into their serializable
        /// lists <see cref="m_DetailValues"/>.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            m_DetailValues.Clear();

            foreach (var kv_Detail in m_Details)
            {
                m_DetailValues.Add(kv_Detail.Value);
            }
        }

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

            OnAfterItemDeserialize();
        }

        /// <summary>
        /// Called at the end of
        /// <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>, it
        /// enabled inheritance to add specific deserialization process.
        /// </summary>
        protected virtual void OnAfterItemDeserialize()
        {
        }

        /// <summary>
        /// Configures a specified <paramref name="builder"/> with this item.
        /// </summary>
        /// <param name="builder">The target builder.</param>
        internal void Configure(CatalogBuilder builder)
        {
            var item = ConfigureItem(builder);
            item.displayName = displayName;

            foreach (var categoryAsset in m_Categories)
            {
                item.categories.Add(categoryAsset.id);
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
    }
}
