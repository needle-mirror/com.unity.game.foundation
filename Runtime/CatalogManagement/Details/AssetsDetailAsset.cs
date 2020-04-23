using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.Configs;
using UnityEngine.GameFoundation.Configs.Details;
using GFTools = UnityEngine.GameFoundation.Tools;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Detail used to reference assets to a <see cref="CatalogItemAsset"/>
    /// using the Resources API.
    /// </summary>
    public partial class AssetsDetailAsset : BaseDetailAsset
    {
        /// <inheritdoc cref="s_TempMatches"/>
        [ThreadStatic] static List<string> ts_Matches;

        /// <summary>
        /// Temporary collection of strings.
        /// </summary>
        internal static List<string> s_TempMatches
        {
            get
            {
                if (ts_Matches == null) ts_Matches = new List<string>();
                return ts_Matches;
            }
        }

        /// <inheritdoc cref="s_UniqueNames"/>
        [ThreadStatic] static List<string> ts_TempUniqueNames;

        /// <summary>
        /// Temporary list of unique names
        /// </summary>
        internal static List<string> s_UniqueNames
        {
            get
            {
                if (ts_TempUniqueNames == null) ts_TempUniqueNames = new List<string>();
                return ts_TempUniqueNames;
            }
        }

        /// <inheritdoc/>
        public override string DisplayName()
        {
            return "Assets Detail";
        }

        /// <inheritdoc/>
        public override string TooltipMessage()
        {
            return "This detail allows the attachment of one or more assets to the given definition.";
        }

        /// <summary>
        /// The asset names, used as keys to get the asset paths
        /// </summary>
        [SerializeField]
        internal List<string> m_Names = new List<string>();

        /// <summary>
        /// The asset paths
        /// </summary>
        [SerializeField]
        internal List<string> m_Values = new List<string>();

        internal const string kNewAssetName = "Asset";

        // this should be considered a failsafe for fixing invalid names
        // a custom editor should be implemented which can do more user-friendly validation
        // try to prevent invalid names from getting assigned in the first place
        private void OnValidate()
        {
            for (var i = 0; i < m_Names.Count; i++)
            {
                if (string.IsNullOrEmpty(m_Names[i]))
                {
                    m_Names[i] = kNewAssetName;
                }
            }
        }

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
            return GetOrLoadAssetByPath<T>(GetAssetPath(assetName));
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
                var asset = GetOrLoadAssetByPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets.ToArray();
        }

        /// <summary>
        /// Finds all resources folder paths that use the associated name.
        /// Note: this returns the current state of assets for this detail definition.
        /// To ensure that there are no invalid or duplicate entries, the list will
        /// always be cleared and 'recycled' (i.e. updated) with current data.
        /// </summary>
        /// <param name="assetName">The name to check for.</param>
        /// <returns>Returns paths than assets stored in a resources folder. Could return null.</returns>
        public string[] GetAssetPaths(string assetName)
        {
            try
            {
                for (var i = 0; i < m_Values.Count; i++)
                {
                    if (m_Names[i] == assetName)
                    {
                        s_TempMatches.Add(m_Values[i]);
                    }
                }

                return s_TempMatches.ToArray();
            }
            finally
            {
                s_TempMatches.Clear();
            }
        }

        /// <summary>
        /// Load and cache an asset that is stored in a resources folder
        /// </summary>
        private T GetOrLoadAssetByPath<T>(string path) where T : Object
        {
            return string.IsNullOrEmpty(path) ? null : Resources.Load<T>(path);
        }

        /// <summary>
        /// Get a copy of the list of names as an array.
        /// Note: this returns the current state of names for this detail definition.
        /// To ensure that there are no invalid or duplicate entries, the list will
        /// always be cleared and 'recycled' (i.e. updated) with current data.
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
            GFTools.ThrowIfArgNull(list, nameof(list));

            list.Clear();

            foreach (var assetName in m_Names)
            {
                if (!list.Contains(assetName))
                {
                    list.Add(assetName);
                }
            }
        }

        /// <inheritdoc />
        internal override BaseDetailConfig CreateConfig()
        {
            var assetsDetail = new AssetsDetailConfig();

            foreach (var name in m_Names)
            {
                var path = GetAssetPath(name);
                if (string.IsNullOrEmpty(path))
                {
                    string message = "Asset name or path is not defined";
                    if (itemDefinition != null)
                    {
                        message += " on " + itemDefinition.id;
                    }

                    throw new Exception(message);
                }

                assetsDetail.entries.Add(new Tuple<string, string>(name, path));
            }

            return assetsDetail;
        }
    }
}
