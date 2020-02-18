using System.Text.RegularExpressions;
using System.IO; 

namespace UnityEngine.GameFoundation
{
    internal static class Tools
    {
        /// <summary>
        /// Handy function for converting a string value to a unique Hash.
        /// Right now we are just hijacking the Animator's StringToHash but down the road we'll make our own implementation.
        /// </summary>
        /// <param name="value">The string value to Hash.</param>
        /// <returns>The unique int Hash of value.</returns>
        public static int StringToHash(string value)
        {
            return Animator.StringToHash(value);
        }

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
