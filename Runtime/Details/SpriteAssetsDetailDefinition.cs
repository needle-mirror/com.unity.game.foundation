using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    public class SpriteAssetsDetailDefinition : BaseDetailDefinition
    {
        private readonly List<string> m_Names;
        private readonly List<Sprite> m_Values;

        // These lists will help out in some methods so we avoid having to allocate each time they're called.
        private List<Sprite> m_Matches = new List<Sprite>();
        private List<string> m_UniqueNames = new List<string>();

        /// <summary>
        /// Constructor to build a SpriteAssetsDetailDefinition object.
        /// </summary>
        /// <param name="spriteAssetNames">The list of string names used to look up a corresponding Sprites from values list. If null value is passed in an empty list will be created.</param>
        /// <param name="spriteAssetValues">The list of Sprites. If null value is passed in an empty list will be created.</param>
        /// <param name="owner">The GameItemDefinition that is attached to this DetailDefinition.</param>
        internal SpriteAssetsDetailDefinition(List<string> spriteAssetNames, List<Sprite> spriteAssetValues, GameItemDefinition owner = null)
            : base(owner)
        {
            m_Names = spriteAssetNames ?? new List<string>();
            m_Values = spriteAssetValues ?? new List<Sprite>();
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
        /// Find a sprite by its name.
        /// </summary>
        /// <param name="assetName">The name to search for.</param>
        /// <returns>Returns a Sprite asset reference. Could return null. Could throw a KeyNotFoundException.</returns>
        public Sprite GetAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            var index = m_Names.IndexOf(assetName);

            return index < 0 ? null : m_Values[index];
        }

        /// <summary>
        /// A square bracket operator to find a Sprite by name string, similar to how you would find a value by key in a Dictionary.
        /// </summary>
        /// <param name="assetName">The Sprite we are searching for</param>
        /// <returns>Returns a Sprite if one is found with that name. Could return null. Could throw a KeyNotFoundException.</returns>
        public Sprite this[string assetName] => GetAsset(assetName);

        /// <summary>
        /// Finds all assets that use the associated name.
        /// </summary>
        /// <param name="assetName">The name to check for.</param>
        /// <returns>an array containing each value assigned the given name.</returns>
        public Sprite[] GetAssets(string assetName)
        {
            m_Matches.Clear();

            for (var i = 0; i < m_Values.Count; i++)
            {
                if (m_Names[i] == assetName)
                {
                    m_Matches.Add(m_Values[i]);
                }
            }

            return m_Matches.ToArray();
        }

        /// <summary>
        /// Get a copy of the list of names as an array.
        /// </summary>
        /// <returns>An array of strings containing all the names.</returns>
        public string[] GetNames()
        {
            m_UniqueNames.Clear();

            foreach (var assetName in m_Names)
            {
                if (!m_UniqueNames.Contains(assetName))
                {
                    m_UniqueNames.Add(assetName);
                }
            }

            return m_UniqueNames.ToArray();
        }

        /// <summary>
        /// Populate a list of strings with all the names from this detail.
        /// Note: this returns the current state of names for this detail definition.
        /// To ensure that there are no invalid or duplicate entries, the list will 
        /// always be cleared and 'recycled' (i.e. updated) with current data.
        /// </summary>
        /// <param name="list">The list to clear and fill with updated name data.</param>
        public void GetNames(List<string> list)
        {
            if (list == null)
            {
                throw new System.ArgumentNullException(nameof(list));
            }

            list.Clear();

            list.AddRange(GetNames());
        }
    }
}
