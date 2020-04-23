using System;

namespace UnityEngine.GameFoundation.Exceptions
{
    /// <summary>
    /// Thrown when a <see cref="Category"/> fails to be found by its
    /// <see cref="Category.id"/>.
    /// </summary>
    public class CategoryNotFoundException : Exception
    {
        /// <summary>
        /// The id of the category not found.
        /// </summary>
        public readonly string categoryId;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="CategoryNotFoundException"/> class.
        /// </summary>
        /// <param name="id">The id of the <see cref="Category"/> not
        /// found.</param>
        public CategoryNotFoundException(string id)
        {
            this.categoryId = id;
        }

        /// <inheritdoc/>
        public override string Message
            => $"{nameof(Category)} {categoryId} not found";
    }
}
