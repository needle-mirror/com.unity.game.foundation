using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    public class AudioClipAssetsDetailDefinition : BaseDetailDefinition
    {
        private readonly List<string> m_Names;
        private readonly List<AudioClip> m_Values;

        // These lists will help out in some methods so we avoid having to allocate each time they're called.
        private List<AudioClip> m_Matches = new List<AudioClip>();
        private List<string> m_UniqueNames = new List<string>();
        
        /// <summary>
        /// Constructor to build an AudioClipAssetsDetailDefinition object.
        /// </summary>
        /// <param name="names">The list of string names used to look up a corresponding audio clip from values list. If null value is passed in an empty list will be created.</param>
        /// <param name="values">The list of AudioClips. If null value is passed in an empty list will be created.</param>
        /// <param name="owner">The GameItemDefinition that is attached to this DetailDefinition.</param>
        internal AudioClipAssetsDetailDefinition(List<string> names, List<AudioClip> values, GameItemDefinition owner = null) : base(owner)
        {
            m_Names = names ?? new List<string>();
            m_Values = values ?? new List<AudioClip>();
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
        /// Find an audio clip by its name.
        /// </summary>
        /// <param name="assetName">The name to search for.</param>
        /// <returns>Returns an AudioClip asset reference. Could return null. Could throw a KeyNotFoundException.</returns>
        public AudioClip GetAsset(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            var index = m_Names.IndexOf(assetName);

            return index < 0 ? null : m_Values[index];
        }

        /// <summary>
        /// A square bracket operator to find an AudioClip by name string, similar to how you would find a value by key in a Dictionary.
        /// </summary>
        /// <param name="assetName">The AudioClip we are searching for</param>
        /// <returns>Returns an AudioClip if one is found with that name. Could return null. Could throw a KeyNotFoundException.</returns>
        public AudioClip this[string assetName] => GetAsset(assetName);

        /// <summary>
        /// Finds all assets that use the associated name.
        /// </summary>
        /// <param name="assetName">The name to check for.</param>
        /// <returns>an array containing each value assigned the given name.</returns>
        public AudioClip[] GetAssets(string assetName)
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
        /// <param name="list">The list to populate. Names will be appended to it.</param>
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
