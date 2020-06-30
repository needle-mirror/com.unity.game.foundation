using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// The catalog storing tags.
    /// </summary>
    public class TagCatalog
    {
        /// <summary>
        /// The list of <see cref="Tag"/> we use within this catalog.
        /// </summary>
        internal Tag[] m_Tags;

        /// <summary>
        /// Gets a Tag from its <paramref name="id"/> or throw an
        /// <see cref="ArgumentException"/> if not found.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="Tag"/> to
        /// find.</param>
        /// <param name="paramName">The name of the <paramref name="id"/>
        /// variable in the caller method.
        /// Used to build a better exception.</param>
        /// <returns>The <see cref="Tag"/> instance with the specified
        /// <paramref name="id"/>.</returns>
        internal Tag GetTagOrDie(string id, string paramName)
        {
            Tools.ThrowIfArgNullOrEmpty(id, paramName);

            foreach (var tag in m_Tags)
            {
                if (tag.key == id)
                {
                    return tag;
                }
            }

            throw new ArgumentException
                ($"{nameof(Tag)} {id} not found", paramName);
        }

        /// <summary>
        /// Gets a collection of tags from their <paramref name="ids"/> or
        /// throw an <see cref="ArgumentException"/> if not found.
        /// </summary>
        /// <param name="ids">The identifiers of the <see cref="Tag"/>
        /// instances to find.</param>
        /// <param name="paramName">The name of the <paramref name="ids"/>
        /// variable in the caller method.
        /// Used to build a better exception.</param>
        /// <param name="target">The target collection for the tags.</param>
        internal void GetTagsOrDie(
            ICollection<string> ids,
            ICollection<Tag> target,
            string paramName)
        {
            Tools.ThrowIfArgNull(ids, paramName);

            foreach (var id in ids)
            {
                var tag = GetTagOrDie(id, paramName);
                target.Add(tag);
            }
        }

        /// <summary>
        /// Looks for a <see cref="Tag"/> by its <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The <see cref="Tag.key"/> of the
        /// <see cref="Tag"/> instance to find.</param>
        /// <returns>The requested <see cref="Tag"/> instance, or
        /// <pre>null</pre> if not found.</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="id"/>
        /// parameter is <c>null</c>.</exception>
        public Tag FindTag(string id)
        {
            Tools.ThrowIfArgNull(id, nameof(id));

            foreach(var tag in m_Tags)
            {
                if(tag.key == id)
                {
                    return tag;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells whether or not this catalog contains a <see cref="Tag"/>
        /// instance with the specified <paramref name="id"/> as
        /// <see cref="Tag.key"/>.
        /// </summary>
        /// <param name="id">The <see cref="Tag.key"/> to find.</param>
        /// <returns><c>true</c> if a <see cref="Tag"/> instance exists in
        /// this cataglog with the specified <paramref name="id"/>, <c>false</c>
        /// otherwise</returns>
        /// <exception cref="ArgumentNullException">If the <paramref name="id"/>
        /// parameter is <c>null</c>.</exception>
        public bool ContainsTag(string id) => FindTag(id) != null;

        /// <summary>
        /// Tells whether or not this catalog contains the specified
        /// <see cref="Tag"/> instance.
        /// </summary>
        /// <param name="tag">The <see cref="Tag"/> to find.</param>
        /// <returns><c>true</c> if a <see cref="Tag"/> instance exists in
        /// this cataglog with the specified <paramref name="id"/>, <c>false</c>
        /// otherwise</returns>
        /// <exception cref="ArgumentNullException">If the
        /// <paramref name="tag"/> parameter is <c>null</c>.</exception>
        public bool ContainsTag(Tag tag)
        {
            Tools.ThrowIfArgNull(tag, nameof(tag));
            return Array.IndexOf(m_Tags, tag) >= 0;
        }

        /// <summary>
        /// Returns an array of all the <see cref="Tag"/> instances of this
        /// catalog.
        /// </summary>
        /// <remarks>
        /// Keep in mind that this method allocates an array.
        /// If you want to avoid allocations, please consider using
        /// <see cref="GetTags(ICollection{Tag})"/> instead.
        /// </remarks>
        /// <returns>An array of all the tags.</returns>
        public Tag[] GetTags() => Tools.ToArray(m_Tags);

        /// <summary>
        /// Fills in the given <paramref name="target"/> collection with all the
        /// <see cref="Tag"/> instances of this catalog, and returns their
        /// count.
        /// The <paramref name="target"/> collection is cleared before being
        /// populated.
        /// </summary>
        /// <param name="target">The target container of all the
        /// <see cref="Tag"/> instances.</param>
        /// <returns>The number of <see cref="Tag"/> instances of this
        /// catalog.</returns>
        public int GetTags(ICollection<Tag> target)
        {
            return Tools.Copy(m_Tags, target);
        }
    }
}
