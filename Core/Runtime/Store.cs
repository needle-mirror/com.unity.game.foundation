using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Maintains Collection of <see cref="BaseTransaction"/> items for a Store and implements methods to retrieve them.
    /// </summary>
    /// <inheritdoc/>
    public class Store : CatalogItem
    {
        [ThreadStatic]
        static List<BaseTransaction> ts_TempStoreItemList;

        static List<BaseTransaction> s_TempStoreItemList
        {
            get
            {
                if (ts_TempStoreItemList is null) ts_TempStoreItemList = new List<BaseTransaction>();
                return ts_TempStoreItemList;
            }
        }

        [ThreadStatic]
        static List<Tag> ts_TempTagList;

        static List<Tag> s_TempTagList
        {
            get
            {
                if (ts_TempTagList is null)
                {
                    ts_TempTagList = new List<Tag>();
                }
                return ts_TempTagList;
            }
        }

        /// <summary>
        /// Available <see cref="Store"/> <see cref="BaseTransaction"/> items for this <see cref="Store"/>.
        /// </summary>
        internal BaseTransaction[] m_Items;

        /// <summary>
        /// Tells whether or not this <see cref="Store"/> instance contains the given <paramref name="transaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="BaseTransaction"/> instance to find.</param>
        /// <returns><c>true</c> if found, <c>false</c> otherwise.</returns>
        public bool Contains(BaseTransaction transaction)
        {
            foreach (var candidate in m_Items)
            {
                if (candidate == transaction) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns an array of the <see cref="BaseTransaction"/> items in this <see cref="Store"/>.
        /// </summary>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this <see cref="Store"/>.</returns>
        public BaseTransaction[] GetStoreItems() => Tools.ToArray(m_Items);

        /// <summary>
        /// Adds all <see cref="BaseTransaction"/> items with any of specified <see cref="Tag"/> set to target Collection.
        /// </summary>
        /// <param name="tag">Desired <see cref="Tag"/> to be queried by this method.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>Number of <see cref="BaseTransaction"/> items added to the target collection.</returns>
        /// <exception cref="ArgumentNullException">Throws if tag is null.</exception>
        /// <exception cref="NullReferenceException">Throws if target is null.</exception>
        int GetStoreItemsByTagInternal(Tag tag, ICollection<BaseTransaction> target)
        {
            target.Clear();
            foreach (var storeItem in m_Items)
            {
                if (storeItem.HasTag(tag))
                {
                    target.Add(storeItem);
                }
            }

            return target.Count;
        }

        /// <summary>
        /// Returns an array of all <see cref="BaseTransaction"/> items associated with the specified <see cref="Tag"/> in this <see cref="Store"/>.
        /// </summary>
        /// <param name="tag">Desired <see cref="Tag"/> to be queried by this method.</param>
        /// <returns>Array of all <see cref="BaseTransaction"/> items associated with the specified <see cref="Tag"/> in this <see cref="Store"</returns>
        /// <exception cref="ArgumentNullException">Throws if <see cref="Tag"/> is null.</exception>
        BaseTransaction[] GetStoreItemsByTagInternal(Tag tag)
        {
            try
            {
                GetStoreItemsByTagInternal(tag, s_TempStoreItemList);
                return s_TempStoreItemList.ToArray();
            }
            finally
            {
                s_TempStoreItemList.Clear();
            }
        }

        /// <summary>
        /// Adds all <see cref="BaseTransaction"/> items with any of specified <see cref="Tag"/> set to target Collection.
        /// </summary>
        /// <param name="tags">Collection of <see cref="Tag"/> to accept.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>Number of <see cref="BaseTransaction"/> items added to the target collection.</returns>
        /// <exception cref="NullReferenceException">Throws if tags or target is null.</exception>
        /// <exception cref="ArgumentNullException">Throws if any <see cref="Tag"/> in tags Collection is null.</exception>
        int GetStoreItemsByTagInternal(ICollection<Tag> tags, ICollection<BaseTransaction> target)
        {
            target.Clear();
            foreach (var tag in tags)
            {
                foreach (var storeItem in m_Items)
                {
                    if (storeItem.HasTag(tag))
                    {
                        target.Add(storeItem);
                        break;
                    }
                }
            }

            return target.Count;
        }

        /// <summary>
        /// Get an array of all <see cref="BaseTransaction"/> items with any of specified <see cref="Tag"/> set.
        /// </summary>
        /// <param name="tags">Collection of <see cref="Tag"/> to accept.</param>
        /// <returns>Array of all <see cref="BaseTransaction"/> items with any of specified <see cref="Tag"/> set.</returns>
        /// <exception cref="NullReferenceException">Throws if tags is null.</exception>
        /// <exception cref="ArgumentNullException">Throws if any <see cref="Tag"/> in tags Collection is null.</exception>
        BaseTransaction[] GetStoreItemsByTagInternal(ICollection<Tag> tags)
        {
            try
            {
                GetStoreItemsByTagInternal(tags, s_TempStoreItemList);
                return s_TempStoreItemList.ToArray();
            }
            finally
            {
                s_TempStoreItemList.Clear();
            }
        }

        /// <summary>
        /// Adds all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with specified <see cref="Tag"/> set to Collection.
        /// </summary>
        /// <param name="tag">Desired <see cref="Tag"/> to be queried by this method.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>The number of <see cref="BaseTransaction"/> items added.</returns>
        /// <exception cref="ArgumentNullException">Throws if tag or target is null.</exception>
        public int GetStoreItemsByTag(Tag tag, ICollection<BaseTransaction> target)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            Tools.ThrowIfArgNull(target, nameof(target));

            return GetStoreItemsByTagInternal(tag, target);
        }

        /// <summary>
        /// Returns an array of the <see cref="BaseTransaction"/> items in this <see cref="Store"/> with specified <see cref="Tag"/> set.
        /// </summary>
        /// <param name="tag">Desired <see cref="Tag"/> to be queried by this method.</param>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this <see cref="Store"/>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <see cref="Tag"/> is null.</exception>
        public BaseTransaction[] GetStoreItemsByTag(Tag tag)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            return GetStoreItemsByTagInternal(tag);
        }

        /// <summary>
        /// Returns an array of the <see cref="BaseTransaction"/> items in this <see cref="Store"/> with specified <see cref="Tag"/> set.
        /// </summary>
        /// <param name="key">Desired <see cref="Tag"/> identifier string to be added to the output array.</param>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this <see cref="Store"/>.</returns>
        /// <exception cref="ArgumentException">Throws if the catalog key is null or empty or not found in this <see cref="Store"/>.</exception>
        public BaseTransaction[] GetStoreItemsByTag(string key)
        {
            Tools.ThrowIfArgNullOrEmpty(key, nameof(key));

            var tag = GameFoundation.catalogs.tagCatalog.GetTagOrDie(key, nameof(key));

            return GetStoreItemsByTagInternal(tag);
        }

        /// <summary>
        /// Updates Collection of all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of the specified <see cref="Tag"/> set.
        /// </summary>
        /// <param name="tags">Collection of desired <see cref="Tag"/> to be added to the output Collection.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>The number of <see cref="BaseTransaction"/> items added.</returns>
        /// <exception cref="ArgumentNullException">Throws if tags or target is null.</exception>
        /// <exception cref="ArgumentException">Throws if any <see cref="Tag"/> in tags Collection is null.</exception>
        public int GetStoreItemsByTag(ICollection<Tag> tags, ICollection<BaseTransaction> target)
        {
            Tools.ThrowIfArgNull(tags, nameof(tags));
            Tools.ThrowIfArgNull(target, nameof(target));

            foreach (var tag in tags)
            {
                if (tag is null)
                {
                    throw new ArgumentException($"{nameof(tags)} cannot contain null values");
                }
            }

            return GetStoreItemsByTagInternal(tags, target);
        }

        /// <summary>
        /// Returns an array of all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of the specified <see cref="Tag"/> set.
        /// </summary>
        /// <param name="tags">Collection of desired <see cref="Tag"/> to be added to the output array.</param>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this <see cref="Store"/> matching caregories request.</returns>
        /// <exception cref="ArgumentNullException">Throws if tags is null.</exception>
        /// <exception cref="ArgumentException">Throws if any <see cref="Tag"/> in Tags Collection is null.</exception>
        public BaseTransaction[] GetStoreItemsByTag(ICollection<Tag> tags)
        {
            Tools.ThrowIfArgNull(tags, nameof(tags));

            foreach (var tag in tags)
            {
                if (tag is null)
                {
                    throw new ArgumentException($"{nameof(tags)} cannot contain null values");
                }
            }

            return GetStoreItemsByTagInternal(tags);
        }

        /// <summary>
        /// Adds all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of the specified <see cref="Tag"/> set to Collection.
        /// </summary>
        /// <param name="tagKeys">Collection of desired <see cref="Tag"/> identifiers to be added to the target Collection.</param>
        /// <param name="target">Collection to place resultant <see cref="BaseTransaction"/> items into.</param>
        /// <returns>The number of <see cref="BaseTransaction"/> items added.</returns>
        /// <exception cref="ArgumentNullException">Throws if either tagIds or target parameter is null.</exception>
        /// <exception cref="ArgumentException">Throws if any of the catalog ids are null or empty or not found in this <see cref="Store"/>.</exception>
        public int GetStoreItemsByTag(ICollection<string> tagKeys, ICollection<BaseTransaction> target)
        {
            Tools.ThrowIfArgNull(tagKeys, nameof(tagKeys));
            Tools.ThrowIfArgNull(target, nameof(target));

            var tags = s_TempTagList;
            try
            {
                GameFoundation.catalogs.tagCatalog.GetTagsOrDie(tagKeys, tags, nameof(tagKeys));

                return GetStoreItemsByTagInternal(tags, target);
            }
            finally
            {
                tags.Clear();
            }
        }

        /// <summary>
        /// Returns an array of all <see cref="BaseTransaction"/> items in this <see cref="Store"/> with any of the specified <see cref="Tag"/> set.
        /// </summary>
        /// <param name="tagKeys">Collection of desired <see cref="Tag"/> identifiers to be added to the output array.</param>
        /// <returns>An array of <see cref="BaseTransaction"/> items in this Store.</returns>
        /// <exception cref="ArgumentNullException">Throws if <see cref="Tag"/> ids Collection is null.</exception>
        /// <exception cref="ArgumentException">Throws if any of the catalog ids are null or empty or not found in this <see cref="Store"/>.</exception>
        public BaseTransaction[] GetStoreItemsByTag(ICollection<string> tagKeys)
        {
            Tools.ThrowIfArgNull(tagKeys, nameof(tagKeys));

            var tags = s_TempTagList;
            try
            {
                GameFoundation.catalogs.tagCatalog.GetTagsOrDie(tagKeys, tags, nameof(tagKeys));

                return GetStoreItemsByTagInternal(tags);
            }
            finally
            {
                tags.Clear();
            }
        }

        /// <summary>
        /// Returns a summary string for this <see cref="Store"/>.
        /// </summary>
        /// <returns>Summary string for this <see cref="Store"/>.</returns>
        public override string ToString()
        {
            return $"{GetType().Name}(Key: '{key}' DisplayName: '{displayName}')";
        }
    }
}
