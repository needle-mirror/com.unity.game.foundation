using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    ///     Base class for most of the static data in Game Foundation.
    /// </summary>
    public abstract partial class CatalogItem : IEquatable<CatalogItem>, IComparable<CatalogItem>
    {
        /// <summary>
        ///     The <see cref="Tag" /> instances this item is linked to.
        /// </summary>
        internal Tag[] tags;

        /// <summary>
        ///     The details of this item.
        /// </summary>
        internal Dictionary<Type, BaseDetail> details;

        /// <summary>
        ///     The readable name of this <see cref="CatalogItem" /> instance.
        ///     It is used to make the Editor more comfortable, but it can also be
        ///     used at runtime to display a readable information to the players.
        /// </summary>
        public string displayName { get; internal set; }

        /// <summary>
        ///     The unique identifier of this <see cref="CatalogItem" />.
        /// </summary>
        public string key { get; internal set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CatalogItem" /> class.
        /// </summary>
        internal CatalogItem() { }

        /// <summary>
        ///     Looks for a <see cref="Tag" />, linked to this
        ///     <see cref="CatalogItem" /> instance, by its <paramref name="tagKey" />.
        /// </summary>
        /// <param name="tagKey">
        ///     The <see cref="Tag.key" /> of the <see cref="Tag" /> to find.
        /// </param>
        /// <returns>
        ///     If found, returns the <see cref="Tag" /> instance,
        ///     otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="tagKey" /> cannot be null
        ///     as a <see cref="Tag.key" /> cannot be null.
        /// </exception>
        public Tag FindTag(string tagKey)
        {
            Tools.ThrowIfArgNull(tagKey, nameof(tagKey));

            foreach (var tag in tags)
            {
                if (tag.key == tagKey)
                {
                    return tag;
                }
            }

            return null;
        }

        /// <summary>
        ///     Returns an array of all the <see cref="Tag" /> instances
        ///     linked to this <see cref="CatalogItem" /> instance.
        ///     catalog.
        /// </summary>
        /// <remarks>
        ///     Keep in mind that this method allocates an array.
        ///     If you want to avoid allocations, please consider using
        ///     <see cref="GetTags(ICollection{Tag})" /> instead.
        /// </remarks>
        /// <returns>
        ///     An array of all the tags linked to this
        ///     <see cref="CatalogItem" /> instance.
        /// </returns>
        public Tag[] GetTags() => Tools.ToArray(tags);

        /// <summary>
        ///     Fills the given <paramref name="target" /> collection with all the
        ///     <see cref="Tag" /> instances linked to this
        ///     <see cref="CatalogItem" /> instance.
        ///     The <paramref name="target" /> collection is cleared before being
        ///     populated.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <see cref="Tag" /> instances.
        /// </param>
        /// <returns>
        ///     The number of <see cref="Tag" /> instances linked to
        ///     this <see cref="CatalogItem" /> instance.
        /// </returns>
        public int GetTags(ICollection<Tag> target)
        {
            Tools.ThrowIfArgNull(target, nameof(target));
            return Tools.Copy(tags, target);
        }

        /// <summary>
        ///     Tells whether or not the given <paramref name="tag" /> is within
        ///     this <see cref="CatalogItem" /> instance.
        /// </summary>
        /// <param name="tag">
        ///     The <see cref="Tag" /> instance to search for.
        /// </param>
        /// <returns>
        ///     Whether or not this <see cref="CatalogItem" /> instance has
        ///     the specified <see cref="Tag" /> included.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="tag" /> is <c>null</c>.
        /// </exception>
        public bool HasTag(Tag tag)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            return Array.IndexOf(tags, tag) >= 0;
        }

        /// <summary>
        ///     Tells whether or not a <see cref="Tag" /> instance with the
        ///     given <paramref name="tagKey" /> is within this <see cref="CatalogItem" />
        ///     instance.
        /// </summary>
        /// <param name="tagKey">
        ///     The identifier of a <see cref="Tag" /> instance.
        /// </param>
        /// <returns>
        ///     Whether or not this <see cref="CatalogItem" /> instance has
        ///     a <see cref="Tag" /> with the given <paramref name="tagKey" />
        ///     included.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="tagKey" /> is <c>null</c>.
        /// </exception>
        public bool HasTag(string tagKey) => FindTag(tagKey) != null;

        /// <summary>
        ///     Returns an array of all the <see cref="BaseDetail" /> instances of
        ///     this <see cref="CatalogItem" /> instance.
        /// </summary>
        /// <remarks>
        ///     Keep in mind that this method allocates an array.
        ///     If you want to avoid allocations, please consider using
        ///     <see cref="GetDetails(ICollection{BaseDetail})" /> instead.
        /// </remarks>
        /// <returns>An array of all the details.</returns>
        public BaseDetail[] GetDetails() => Tools.ToArray(details.Values);

        /// <summary>
        ///     Fills in the given <paramref name="target" /> collection with all the
        ///     <see cref="BaseDetail" /> instance of this <see cref="CatalogItem" />
        ///     instance.
        ///     The <paramref name="target" /> collection is cleared before being
        ///     populated.
        /// </summary>
        /// <param name="target">
        ///     The target container of all the <see cref="BaseDetail" /> instances.
        /// </param>
        /// <return>
        ///     The number of <see cref="BaseDetail" /> of this
        ///     <see cref="CatalogItem" /> instance.
        /// </return>
        public int GetDetails(ICollection<BaseDetail> target)
            => Tools.Copy(details.Values, target);

        /// <summary>
        ///     Gets the <typeparamref name="TDetail" /> instance by its type.
        /// </summary>
        /// <typeparam name="TDetail">The type of detail requested.</typeparam>
        /// <returns>
        ///     The <typeparamref name="TDetail" /> instance stored in this
        ///     <see cref="CatalogItem" /> instance.
        /// </returns>
        public TDetail GetDetail<TDetail>() where TDetail : BaseDetail
        {
            details.TryGetValue(typeof(TDetail), out var definition);
            return definition as TDetail;
        }

        /// <summary>
        ///     Requests key be removed from item.
        /// </summary>
        /// <param name="tagKey">
        ///     Tag key id string to remove from this catalog item.
        /// </param>
        internal void OnRemoveTag(string tagKey)
        {
            if (tags != null && tags.Length > 0)
            {
                // count how many keys match (should be 0 or 1, but better to be safe and remove all matches)
                var keyMatchesFound = 0;
                foreach (var tag in tags)
                {
                    if (tag.key == tagKey)
                    {
                        ++keyMatchesFound;
                    }
                }

                // if we found any matches
                if (keyMatchesFound > 0)
                {
                    // determine how many tags we will have after removing matching tags
                    var newTagsLength = tags.Length - keyMatchesFound;
                    if (newTagsLength <= 0)
                    {
                        // no tags remain after removing matching tags--tags array now null
                        tags = null;
                    }

                    // if we will still have some tags (ones that did NOT match tag we are removing)
                    else
                    {
                        // setup new array for remaining (non-matching) tags and copy over remaining tags
                        var newTags = new Tag[newTagsLength];
                        var keyOn = 0;
                        foreach (var tag in tags)
                        {
                            if (tag.key != tagKey)
                            {
                                newTags[keyOn] = tag;
                                ++keyOn;
                            }
                        }

                        // replace old tag array with new one that does not contain removed tag
                        tags = newTags;
                    }
                }
            }
        }

        /// <inheritdoc />
        public override int GetHashCode() => key.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        bool IEquatable<CatalogItem>.Equals(CatalogItem other)
            => ReferenceEquals(this, other);

        /// <inheritdoc />
        int IComparable<CatalogItem>.CompareTo(CatalogItem other)
            => key.CompareTo(other.key);
    }
}
