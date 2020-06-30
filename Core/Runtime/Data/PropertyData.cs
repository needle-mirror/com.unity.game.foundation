using System;

namespace UnityEngine.GameFoundation.Data
{
    /// <summary>
    ///     Serializable data structure of a Property.
    /// </summary>
    [Serializable]
    public struct PropertyData
    {
        /// <summary>
        ///     Property's identifier.
        /// </summary>
        public string key;

        /// <summary>
        ///     Property's current value.
        /// </summary>
        public Property value;
    }
}
