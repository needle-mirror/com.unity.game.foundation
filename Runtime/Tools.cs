using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace UnityEngine.GameFoundation
{
    internal static class Tools
    {
        /// <summary>
        /// Handy function for converting a string value to a unique hash.
        /// Right now we are just hijacking the Animator's StringToHash but down the road we'll make our own implementation.
        /// </summary>
        /// <param name="value">The string value to hash.</param>
        /// <returns>The unique int hash of value.</returns>
        public static int StringToHash(string value)
        {
            return Animator.StringToHash(value);
        }

        /// <summary>
        /// Checks to see if the argument is a valid Id. Valid Ids are alphanumeric with optional dashes or underscores.
        /// No whitespace is permitted
        /// </summary>
        /// <param name="id">id to check</param>
        /// <returns>whether id is valid or not</returns>
        public static bool IsValidId(string id)
        {
            return id != null && Regex.IsMatch(id, @"^[\w-_]+$");
        }
        
        /// <summary>
        /// Helper method for making sure the application is not in play mode.
        /// This will mainly be used to make sure users aren't modifying definitions in play mode.
        /// </summary>
        /// <param name="errorMessage">The error message to display if we are in play mode.</param>
        /// <exception cref="Exception">Thrown when in play mode with the given error message.</exception>
        public static void ThrowIfPlayMode(string errorMessage)
        {
            if (Application.isPlaying)
            {
                throw new System.Exception(errorMessage);
            }
        }
    }
}
