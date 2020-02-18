using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// CategoryDefinition describes a Category.
    /// CategoryDefinitions are usable across multiple systems, but each system is 
    /// responsible for containing and managing its own Categories.
    /// </summary>
    public class CategoryDefinition
    {
        /// <summary>
        /// The string Id of this CategoryDefinition.
        /// </summary>
        /// <returns>The string Id of this CategoryDefinition.</returns>
        public string id { get; }

        /// <summary>
        /// The Hash of this CategoryDefinition.
        /// </summary>
        /// <returns>The Hash of this CategoryDefinition.</returns>
        public int hash { get; }

        /// <summary>
        /// The name of this CategoryDefinition for the user to display.
        /// </summary>
        /// <returns>The name of this CategoryDefinition for the user to display.</returns>
        public string displayName { get; }

        /// <summary>
        /// Constructor for a CategoryDefinition.
        /// </summary>
        /// <param name="id">The Id this CategoryDefinition will use.</param>
        /// <param name="displayName">The name this CategoryDefinition will use.</param>
        /// <exception cref="ArgumentException">Thrown if given id or display name is null or empty or if the id is invalid.  Valid ids are alphanumeric with optional dashes or underscores.</exception>
        internal CategoryDefinition(string id, string displayName)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("CategoryDefinition cannot have null or empty id.");
            }
            
            if (!Tools.IsValidId(id))
            {
                throw new ArgumentException("CategoryDefinition must be alphanumeric. Dashes (-) and underscores (_) allowed.");
            }
            
            if (string.IsNullOrEmpty(displayName))
            {
                throw new ArgumentException("CategoryDefinition cannot have null or empty display name.");
            }
            
            this.displayName = displayName;
            this.id = id;
            hash = Tools.StringToHash(this.id); 
        }

        /// <summary>
        /// == Overload. If ID and DisplayName match then the CategoryDefinitions are deemed equal.
        /// Note: Two null objects are considered equal.
        /// </summary>
        /// <param name="x">A CategoryDefinition to compare.</param>
        /// <param name="y">A CategoryDefinition to compare.</param>
        /// <returns>True if both CategoryDefinitions are the same (id & display name must match).</returns>
        public static bool operator ==(CategoryDefinition x, CategoryDefinition y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.id == y.id && x.displayName == y.displayName;
        }

        /// <summary>
        /// != Overload. If ID and DisplayName don't match the CategoryDefinitions are deemed not equal.
        /// Note: Two null objects are considered equal.
        /// </summary>
        /// <param name="x">A CategoryDefinition to compare.</param>
        /// <param name="y">A CategoryDefinition to compare.</param>
        /// <returns>False if both CategoryDefinitions are the same (id & display name's both match).</returns>
        public static bool operator !=(CategoryDefinition x, CategoryDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// If ID and DisplayName match then the CategoryDefinitions are deemed equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if both CategoryDefinitions are the same (id & display name must match).</returns>
        public override bool Equals(object obj)
        {
            var categoryDefinition = obj as CategoryDefinition;
            return categoryDefinition != null && this == categoryDefinition;
        }

        /// <summary>
        /// Returns the Hash code associated with this CategoryDefinition's Id.
        /// </summary>
        /// <returns>The Hash code associated with this CategoryDefinition's Id.</returns>
        public override int GetHashCode()
        {
            return hash;
        }
    }
    
}
