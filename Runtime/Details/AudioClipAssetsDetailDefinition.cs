using System.Collections.Generic;

namespace UnityEngine.GameFoundation
{
    public class AudioClipAssetsDetailDefinition : BaseDetailDefinition
    {
        public override string DisplayName()
        {
            return "Audio Clip Assets Detail";
        }

        public override string TooltipMessage()
        {
            return "This detail allows the attachment of one or more audio clips to the given definition.";
        }

        [SerializeField]
        private List<string> m_Keys = new List<string>();

        [SerializeField]
        private List<AudioClip> m_Values = new List<AudioClip>();

        // this should be considered a failsafe for fixing invalid keys
        // a custom editor should be implemented which can do more user-friendly validation
        // try to prevent invalid keys from getting assigned in the first place
        private void OnValidate()
        {
            List<string> validatedKeys = new List<string>();

            for (int i = 0; i < m_Keys.Count; i++)
            {
                if (string.IsNullOrEmpty(m_Keys[i]))
                {
                    m_Keys[i] = "Audio Clip";
                }

                if (validatedKeys.Contains(m_Keys[i]))
                {
                    m_Keys[i] = MakeKeyValid(m_Keys[i]);
                }

                validatedKeys.Add(m_Keys[i]);
            }
        }

        /// <summary>
        /// Get a key that is guaranteed to be valid for use in this dictionary.
        /// </summary>
        /// <returns>Returns a guaranteed valid key string.</returns>
        internal string GetNextValidKey()
        {
            return MakeKeyValid("Audio Clip");
        }

        /// <summary>
        /// Check to see if a given key is valid.
        /// </summary>
        /// <param name="key">The key to validate.</param>
        /// <returns>True if the key is valid, otherwise false.</returns>
        private bool IsValidKey(string key)
        {
            return !string.IsNullOrEmpty(key) && !m_Keys.Contains(key);
        }

        /// <summary>
        /// If the given key string isn't valid, make it valid by adding or incrementing a number on the end.
        /// </summary>
        /// <param name="key">The key to validate.</param>
        /// <returns>Returns a guaranteed valid key string. It could return the original key string, or a slightly modified one if needed.</returns>
        private string MakeKeyValid(string key)
        {
            if (!IsValidKey(key))
            {
                key = DetailHelper.GetUniqueName(m_Keys, key);
            }

            return key;
        }

        /// <summary>
        /// Check if this collection contains the given key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>Returns true if this collection contains the given key, otherwise returns false.</returns>
        public bool ContainsKey(string key)
        {
            return m_Keys.Contains(key);
        }

        /// <summary>
        /// Find an audio clip by its key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>Returns an AudioClip asset reference. Could return null. Could throw a KeyNotFoundException.</returns>
        public AudioClip GetAsset(string key)
        {
            int index = m_Keys.IndexOf(key);

            if (index < 0)
            {
                throw new KeyNotFoundException($"The key '{key}' was not found.");
            }

            return m_Values[index];
        }

        /// <summary>
        /// A square bracket operator to find an AudioClip by key string, similar to how you would find a value by key in a Dictionary.
        /// </summary>
        /// <param name="key">The AudioClip we are searching for</param>
        /// <returns>Returns an AudioClip if one is found with that key. Could return null. Could throw a KeyNotFoundException.</returns>
        public AudioClip this[string key] => GetAsset(key);

        /// <summary>
        /// Get a copy of the list of keys as an array.
        /// </summary>
        /// <returns>An array of strings containing all the keys.</returns>
        public string[] GetKeys()
        {
            return m_Keys.ToArray();
        }

        /// <summary>
        /// Populate a list of strings with all the keys from this detail.
        /// </summary>
        /// <param name="list">The list to populate. Keys will be appended to it.</param>
        public void GetKeys(List<string> list)
        {
            if (list == null)
            {
                throw new System.ArgumentNullException(nameof(list));
            }

            list.AddRange(m_Keys);
        }
    }
}
