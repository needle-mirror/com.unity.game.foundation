using System.Text.RegularExpressions;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    static class Tools
    {
        internal const string k_GameFoundationPersistenceId = "gamefoundation_persistence";

        /// <summary>
        /// Checks to see if the argument is a valid Identifier. Valid Ids are alphanumeric with optional dashes or underscores.
        /// No whitespace is permitted
        /// </summary>
        /// <param name="key">identifier to check</param>
        /// <returns>whether Identifier is valid or not</returns>
        public static bool IsValidKey(string key)
        {
            return key != null && Regex.IsMatch(key, @"^[\w-_]+$");
        }
    }
}
