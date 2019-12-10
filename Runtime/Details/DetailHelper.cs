using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    internal static class DetailHelper
    {
        // ObjectNames.GetUniqueName is nice, but it's blocked
        // from being called while in a serialization method
        // so we have created a custom version here

        /// <summary>
        /// Creates a unique name based on a given proposed name compared to a list of existing names.
        /// </summary>
        /// <param name="existingNames">The list of names to compare to. If any of these names match the proposed new name, the proposed new name is adjusted.</param>
        /// <param name="name">The proposed new name. It will be adjusted until it no longer matches anything in the existingNames collection.</param>
        /// <returns>Returns a unique name. Could return the original name if no change was needed.</returns>
        public static string GetUniqueName(List<string> existingNames, string name)
        {
            if (existingNames == null)
            {
                throw new System.ArgumentNullException(nameof(existingNames));
            }

            if (name == null)
            {
                throw new System.ArgumentNullException(nameof(name));
            }

            int loopSafety = 0;

            while (existingNames.Contains(name))
            {
                int lastIndex = name.LastIndexOf('(') + 1;
                string lastString = name.Substring(lastIndex);
                lastString = lastString.TrimEnd(')');

                if (int.TryParse(lastString, out var lastInt)
                    && lastInt.ToString().Equals(lastString))
                {
                    name = $"{name.Substring(0, lastIndex)}{lastInt + 1})";
                }
                else
                {
                    name = $"{name} (1)";
                }

                loopSafety++;
                if (loopSafety > 9999)
                {
                    break;
                }
            }

            return name;
        }
    }
}
