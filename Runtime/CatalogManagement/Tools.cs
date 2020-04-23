using System.Text.RegularExpressions;
using System.IO; 

namespace UnityEngine.GameFoundation.CatalogManagement
{
    internal static class Tools
    {
        internal const string k_GameFoundationPersistenceId = "gamefoundation_persistence";

        /// <summary>
        /// Checks to see if the argument is a valid Id. Valid Ids are alphanumeric with optional dashes or underscores.
        /// No whitespace is permitted
        /// </summary>
        /// <param name="id">id to check</param>
        /// <returns>whether Id is valid or not</returns>
        public static bool IsValidId(string id)
        {
            return id != null && Regex.IsMatch(id, @"^[\w-_]+$");
        }
    }
}
