#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class CatalogItemAsset
    {
        /// <summary>
        /// Returns the prefix used to give a name to the asset.
        /// </summary>
        internal abstract string Editor_AssetPrefix { get; }

        /// <summary>
        /// Returns the name to assign to the asset.
        /// </summary>
        internal virtual string Editor_AssetName => $"{Editor_AssetPrefix}_{key}";

        /// <summary>
        /// Initializes the key, displayname and object name.
        /// </summary>
        /// <param name="key">The identifier of the definition</param>
        /// <param name="displayName">The display name of the definition</param>
        /// <param name="name">The name of the asset.</param>
        internal void Editor_Initialize(BaseCatalogAsset catalog, string key, string displayName)
        {
            GFTools.ThrowIfArgNull(catalog, nameof(catalog));
            GFTools.ThrowIfArgNullOrEmpty(key, nameof(key));
            GFTools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            m_Catalog = catalog;
            Editor_SetKey(key);
            Editor_SetDisplayName(displayName);
            name = Editor_AssetName;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Sets the key of <paramref name="this"/>.
        /// </summary>
        /// <param name="key">The identifier to assign to the definition</param>
        internal void Editor_SetKey(string key)
        {
            GFTools.ThrowIfArgNullOrEmpty(key, nameof(key));

            if (!GFTools.IsValidId(key))
            {
                throw new ArgumentException
                    ($"{nameof(CatalogItemAsset)} identifier can only be alphanumeric with optional dashes or underscores.");
            }

            m_Key = key;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Sets the display name of this <see cref="CatalogItemAsset"/>
        /// instance.
        /// </summary>
        /// <param name="displayName">The display name to assign to the
        /// definition.</param>
        internal void Editor_SetDisplayName(string displayName)
        {
            GFTools.ThrowIfArgNullOrEmpty(displayName, nameof(displayName));

            m_DisplayName = displayName;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Adds a static property to the definition of this <see cref="InventoryItemDefinitionAsset"/> instance.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the new static property.
        /// </param>
        /// <param name="value">
        ///     The value of this new static property.
        ///     Also defines its type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if created, <c>false</c> otherwise.
        /// </returns>
        internal bool Editor_AddStaticProperty(string propertyName, Property value)
        {
            if (staticProperties.ContainsKey(propertyName))
                return false;

            staticProperties.Add(name, value);
            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        /// Adds the given <paramref name="tag"/> to this item.
        /// </summary>
        /// <param name="tag">The <see cref="TagAsset"/> instance to
        /// add.</param>
        /// <returns>Whether or not adding the <paramref name="tag"/> was
        /// successful.</returns>
        /// <exception cref="ArgumentException">Thrown if the
        /// <paramref name="tag"/> is already on this definition.</exception>
        internal bool Editor_AddTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            if (m_Tags.Contains(tag))
            {
                throw new ArgumentException("Cannot add a duplicate tag definition.");
            }

            m_Tags.Add(tag);

            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        /// Removes the <paramref name="tag"/> from this
        /// <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="tag">The <see cref="TagAsset"/> to
        /// remove.</param>
        /// <returns>Whether or not the removal was successful.</returns>
        internal bool Editor_RemoveTag(TagAsset tag)
        {
            GFTools.ThrowIfArgNull(tag, nameof(tag));

            var removed = m_Tags.Remove(tag);

            if (removed)
            {
                EditorUtility.SetDirty(this);
            }

            return removed;
        }

        /// <summary>
        /// Adds a detail of type <typeparamref name="TDetail"/> to this
        /// <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <typeparam name="TDetail">The type of the new detail to add.</typeparam>
        /// <returns>A reference to the detail that was just added.</returns>
        internal TDetail Editor_AddDetail<TDetail>() where TDetail : BaseDetailAsset
        {
            var newDetail = CreateInstance<TDetail>();
            Editor_AddDetail(newDetail);
            return newDetail;
        }

        /// <summary>
        /// Adds the <paramref name="detail"/> to this
        /// <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="detail">The <see cref="BaseDetailAsset"/> to add.</param>
        /// <exception cref="ArgumentException">Thrown if the given detail
        /// definition is already on this item.</exception>
        internal void Editor_AddDetail(BaseDetailAsset detail)
        {
            GFTools.ThrowIfArgNull(detail, nameof(detail));

            if (m_Details == null)
            {
                m_Details = new Dictionary<Type, BaseDetailAsset>();
            }

            // if the Detail already exists then throw
            var detailType = detail.GetType();

            if (m_Details.TryGetValue(detailType, out var oldDetailDefinition))
            {
                if (oldDetailDefinition.GetType() == detailType)
                {
                    throw new ArgumentException($"{displayName} ({GetType().Name}) already has a {detailType.Name} detail.");
                }
            }

            m_Details[detailType] = detail;
            detail.m_ItemDefinition = this;

            detail.name = detail.Editor_AssetName;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Removes the <paramref name="detail"/> from this
        /// <see cref="CatalogItemAsset"/> instance.
        /// </summary>
        /// <param name="detail">The <see cref="BaseDetailAsset"/> to
        /// remove.</param>
        /// <returns>Whether or not the <paramref name="detail"/> was
        /// successfully removed.</returns>
        internal bool Editor_RemoveDetail(BaseDetailAsset detail)
        {
            GFTools.ThrowIfArgNull(detail, nameof(detail));

            var detailType = detail.GetType();
            if (!m_Details.TryGetValue(detailType, out _))
            {
                return false;
            }

            if (!m_Details.Remove(detailType))
            {
                return false;
            }

            EditorUtility.SetDirty(this);

            return true;
        }

        /// <summary>
        /// Gets all the subassets of this <see cref="CatalogItemAsset"/>
        /// instance.
        /// </summary>
        /// <param name="target">The target collection to which the subassets
        /// will be added</param>
        internal void Editor_GetSubAssets(ICollection<Object> target)
        {
            if (m_Details != null)
            {
                foreach (var detail in m_Details.Values)
                {
                    if (detail is null) continue;

                    target.Add(detail);
                    detail.Editor_GetSubAssets(target);
                }
            }

            Editor_GetItemSubAssets(target);
        }

        protected virtual void Editor_GetItemSubAssets(ICollection<Object> target) { }

        protected void OnDestroy()
        {
            int count = m_Details.Count;

            // if any DetailDefinitions are actually attached
            if (count > 0)
            {
                var details = new BaseDetailAsset[m_Details.Count];
                m_Details.Values.CopyTo(details, 0);
                m_Details.Clear();

                // remove them from the asset database
                for (var i = 0; i < details.Length; i++)
                {
                    var detailToRemove = details[i];

                    //Debug.Log($"Remove detail {detailToRemove.GetType().Name} from {displayName} ({GetType().Name})");
                    DestroyImmediate(detailToRemove, true);
                }
            }

            OnItemDestroy();
        }

        protected abstract void OnItemDestroy();
    }
}

#endif
