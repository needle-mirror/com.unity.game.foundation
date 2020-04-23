using System;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    public class AssetsDetail : BaseDetail
    {
        // These lists will help out in some methods so we avoid having to allocate each time they're called.
        [ThreadStatic] static List<string> ts_Matches;
        static List<string> s_Matches
        {
            get
            {
                if (ts_Matches is null) ts_Matches = new List<string>();
                return ts_Matches;
            }
        }

        [ThreadStatic] static List<string> ts_UniqueNames;
        static List<string> s_UniqueNames
        {
            get
            {
                if (ts_UniqueNames is null) ts_UniqueNames = new List<string>();
                return ts_UniqueNames;
            }
        }
        
        internal readonly List<string> m_Names = new List<string>();
        internal readonly List<string> m_Values = new List<string>();

        /// <summary>
        /// Check if this collection contains the given name.
        /// </summary>
        /// <param name="assetName">The name to search for.</param>
        /// <returns>Returns true if this collection contains the given name, otherwise returns false.</returns>
        public bool ContainsName(string assetName)
        {
            return !string.IsNullOrEmpty(assetName) && m_Names.Contains(assetName);
        }

        /// <summary>
        /// Load an asset that is stored in a resources folder.
        /// </summary>
        /// <param name="assetName">The name to search for.</param>
        /// <returns>Returns an asset reference that is stored in a resources folder. Could return null.</returns>
        public T GetAsset<T>(string assetName) where T : Object
        {
            var path = GetAssetPath(assetName);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            
            return Resources.Load<T>(path);
        }
        
        /// <summary>
        /// Find an path that an asset stored in a resources folder.
        /// </summary>
        /// <param name="assetName">The name to search for.</param>
        /// <returns>Returns a path than asset stored in a resources folder. Could return null.</returns>
        public string GetAssetPath(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            var index = m_Names.IndexOf(assetName);
            return index < 0 ? null : m_Values[index];
        }

        /// <summary>
        /// Load all assets that use the associated name from resources folder.
        /// </summary>
        /// <param name="assetName">The name to check for.</param>
        /// <returns>Returns an array containing each asset reference assigned the given name.</returns>
        public T[] GetAssets<T>(string assetName) where T : Object
        {
            var paths = GetAssetPaths(assetName);
            if (paths == null || paths.Length == 0)
            {
                return null;
            }
            
            var assets = new List<T>();
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }
                
                var asset = Resources.Load<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            
            return assets.ToArray();
        }
        
        /// <summary>
        /// Finds all resources folder paths that use the associated name.
        /// </summary>
        /// <param name="assetName">The name to search for.</param>
        /// <returns>Returns paths than assets stored in a resources folder. Could return null.</returns>
        public string[] GetAssetPaths(string assetName)
        {
            try
            {
                for (var i = 0; i < m_Values.Count; i++)
                {
                    if (m_Names[i] == assetName)
                    {
                        s_Matches.Add(m_Values[i]);
                    }
                }

                return s_Matches.ToArray();
            }
            finally
            {
                s_Matches.Clear();
            }
        }

        /// <summary>
        /// Get a copy of the list of names as an array.
        /// </summary>
        /// <returns>An array of strings containing all the names.</returns>
        public string[] GetNames()
        {
            try
            {
                foreach (var assetName in m_Names)
                {
                    if (!s_UniqueNames.Contains(assetName))
                    {
                        s_UniqueNames.Add(assetName);
                    }
                }

                return s_UniqueNames.ToArray();
            }
            finally
            {
                s_UniqueNames.Clear();
            }
        }

        /// <summary>
        /// Populate a list of strings with all the names from this detail.
        /// Note: this returns the current state of names for this detail definition.
        /// To ensure that there are no invalid or duplicate entries, the list will 
        /// always be cleared and 'recycled' (i.e. updated) with current data.
        /// </summary>
        /// <param name="list">The list to clear and fill with updated name data.</param>
        public void GetNames(ICollection<string> list)
        {
            if (list == null)
            {
                throw new System.ArgumentNullException(nameof(list));
            }

            list.Clear();

            foreach (var assetName in m_Names)
            {
                if (!list.Contains(assetName))
                {
                    list.Add(assetName);
                }
            }
        }
    }
}
