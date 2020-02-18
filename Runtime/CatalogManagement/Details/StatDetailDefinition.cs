using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using StatValueType = UnityEngine.GameFoundation.CatalogManagement.StatDefinition.StatValueType;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Detail definition to establish that item uses certain stats, and also to set default values for those stats.
    /// </summary>
    public class StatDetailDefinition : BaseDetailDefinition, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Returns 'friendly' display name for this StatDetailDefinition.
        /// </summary>
        /// <returns>'friendly' display name for this detail definition.</returns>
        public override string DisplayName() => "Stat Detail";

        /// <summary>
        /// Returns string message which explains the purpose of this StatDetailDefinition, for the purpose of displaying as a tooltip in editor.
        /// </summary>
        /// <returns>The string tooltip message of this StatDetailDefinition.</returns>
        public override string TooltipMessage() => "The stat detail allows the attachment of stats with specific default values to the given definition.";

        /// <summary>
        /// Creates a runtime StatDetailDefinition based on this editor-time StatDetailDefinition.
        /// </summary>
        /// <returns>Runtime version of StatDetailDefinition.</returns>
        public override UnityEngine.GameFoundation.BaseDetailDefinition CreateRuntimeDefinition()
        {
            if (statDefaultValues == null)
            {
                return new UnityEngine.GameFoundation.StatDetailDefinition(new Dictionary<int, UnityEngine.GameFoundation.StatUnion>());
            }

            Dictionary<int, UnityEngine.GameFoundation.StatUnion> runtimeStatDefaultValues = new Dictionary<int, UnityEngine.GameFoundation.StatUnion>(statDefaultValues.Count);

            foreach (var statValue in statDefaultValues)
            {
                switch (statValue.Value.type)
                {
                    case StatDefinition.StatValueType.Int:
                        runtimeStatDefaultValues.Add(statValue.Key, statValue.Value.intValue);
                        break;

                    case StatDefinition.StatValueType.Float:
                        runtimeStatDefaultValues.Add(statValue.Key, statValue.Value.floatValue);
                        break;
                    default:
                        Debug.LogWarning("Attempted to convert unsupported statValue type. Stat values can only be int or float.");
                        break;
                }
            }

            return new UnityEngine.GameFoundation.StatDetailDefinition(runtimeStatDefaultValues);
        }

        Dictionary<int, StatUnion> m_StatDefaultValues = new Dictionary<int, StatUnion>();
        internal Dictionary<int, StatUnion> statDefaultValues => m_StatDefaultValues;

        // TODO: I don't know why we're hiding these from the inspector when we have a custom editor for this class anyway (seeing in inspector debug mode helps with debugging)

        // note: these lists are used to serialize and deserialize the above dictionary
        [SerializeField, HideInInspector]
        List<int> m_DefaultKeyList = new List<int>();

        [SerializeField, HideInInspector]
        List<StatUnion> m_DefaultValueList = new List<StatUnion>();

        // these are the old fields, kept around so we don't break older databases and can migrate the data automatically
        [SerializeField, HideInInspector]
        List<int> m_StatDefaultIntValues_Keys = new List<int>();
        [SerializeField, HideInInspector]
        List<int> m_StatDefaultIntValues_Values = new List<int>();
        [SerializeField, HideInInspector]
        List<int> m_StatDefaultFloatValues_Keys = new List<int>();
        [SerializeField, HideInInspector]
        List<float> m_StatDefaultFloatValues_Values = new List<float>();

        // internal constructor to prohibit developers instantiating StatDetailDefinitions.
        internal StatDetailDefinition() { }

#if UNITY_EDITOR
        private void HandleStatCatalogWillRemoveStatDefinition(object sender, StatDefinition statDefinition)
        {
            switch (statDefinition.statValueType)
            {
                case StatDefinition.StatValueType.Int:
                    RemoveStatInt(statDefinition.idHash);
                    EditorUtility.SetDirty(this);
                    return;

                case StatDefinition.StatValueType.Float:
                    RemoveStatFloat(statDefinition.idHash);
                    EditorUtility.SetDirty(this);
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(statDefinition), "statDefinition has an unrecognized StatValueType");
            }
        }

        private void OnEnable()
        {
            StatCatalog.OnWillRemoveStatDefinition += HandleStatCatalogWillRemoveStatDefinition;
        }

        private void OnDisable()
        {
            StatCatalog.OnWillRemoveStatDefinition -= HandleStatCatalogWillRemoveStatDefinition;
        }
#endif

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionId">The StatDefinition ID to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public int GetStatInt(string statDefinitionId)
        {
            return GetStatInt(Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public int GetStatInt(int statDefinitionHash)
        {
            if (statDefaultValues != null
                && statDefaultValues.TryGetValue(statDefinitionHash, out var value))
            {
                return value.intValue;
            }

            throw new KeyNotFoundException("Attempted to get stat default int value for stat that does not exist.");
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionId">The StatDefinition ID to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public float GetStatFloat(string statDefinitionId)
        {
            return GetStatFloat(Tools.StringToHash(statDefinitionId));
        }

        /// <summary>
        /// Get default value for specified stat.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>Default value for specified stat.</returns>
        public float GetStatFloat(int statDefinitionHash)
        {
            if (statDefaultValues != null
                && statDefaultValues.TryGetValue(statDefinitionHash, out var value))
            {
                return value.floatValue;
            }

            throw new KeyNotFoundException("Attempted to get stat default float value for stat that does not exist.");
        }

        /// <summary>
        /// Adds default value to StatDetailDefinition for specified stat.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <param name="value">The default value for the stat.</param>
        public void AddStatInt(int statDefinitionHash, int value)
        {
            Tools.ThrowIfPlayMode("Cannot Add Stat Default Int Value while in play mode.");

            m_StatDefaultValues[statDefinitionHash] = value;
        }

        /// <summary>
        /// Adds default value to StatDetailDefinition for specified stat.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <param name="value">The default value for the stat.</param>
        public void AddStatFloat(int statDefinitionHash, float value)
        {
            Tools.ThrowIfPlayMode("Cannot Add Stat Default Float Value while in play mode.");

            m_StatDefaultValues[statDefinitionHash] = value;
        }

        /// <summary>
        /// Remove default value to StatDetailDefinition for specified stat.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>True if the specified default stat was removed.</returns>
        public bool RemoveStatInt(int statDefinitionHash)
        {
            Tools.ThrowIfPlayMode("Cannot Remove Stat Default Int Value while in play mode.");

            return statDefaultValues != null
                && statDefaultValues.Remove(statDefinitionHash);
        }

        /// <summary>
        /// Remove default value to StatDetailDefinition for specified stat.
        /// This method throws if not in editor mode.
        /// </summary>
        /// <param name="statDefinitionHash">The StatDefinition Hash to address.</param>
        /// <returns>True if the specified default stat was removed.</returns>
        public bool RemoveStatFloat(int statDefinitionHash)
        {
            Tools.ThrowIfPlayMode("Cannot Remove Stat Default Float Value while in play mode.");

            return statDefaultValues != null
                && statDefaultValues.Remove(statDefinitionHash);
        }

        /// <summary>
        /// Called before serializing to prepare dictionary to be serialized
        /// </summary>
        public void OnBeforeSerialize()
        {
            CopyDictionaryToLists(statDefaultValues, out m_DefaultKeyList, out m_DefaultValueList);
        }

        /// <summary>
        /// Called after deserializing to update dictionary from serialized data
        /// </summary>
        public void OnAfterDeserialize()
        {
            // migrate data from 0.2.0 to 0.3.0
            // if the old lists have stuff in them and the new lists do not,
            // then copy the old stuff to the new stuff and delete the old stuff

            if (m_DefaultKeyList.Count <= 0 && m_DefaultValueList.Count <= 0)
            {
                if (m_StatDefaultIntValues_Keys.Count == m_StatDefaultIntValues_Values.Count)
                {
                    for (var i = 0; i < m_StatDefaultIntValues_Keys.Count; i++)
                    {
                        m_DefaultKeyList.Add(m_StatDefaultIntValues_Keys[i]);
                        m_DefaultValueList.Add(m_StatDefaultIntValues_Values[i]);
                        m_StatDefaultIntValues_Keys.RemoveAt(i);
                        m_StatDefaultIntValues_Values.RemoveAt(i);
                        i--;
                    }
                }

                if (m_StatDefaultFloatValues_Keys.Count == m_StatDefaultFloatValues_Values.Count)
                {
                    for (var i = 0; i < m_StatDefaultFloatValues_Keys.Count; i++)
                    {
                        m_DefaultKeyList.Add(m_StatDefaultFloatValues_Keys[i]);
                        m_DefaultValueList.Add(m_StatDefaultFloatValues_Values[i]);
                        m_StatDefaultFloatValues_Keys.RemoveAt(i);
                        m_StatDefaultFloatValues_Values.RemoveAt(i);
                        i--;
                    }
                }
            }

            // this is not part of the migration but should happen after the migration

            CopyListsToDictionary(m_DefaultKeyList, m_DefaultValueList, ref m_StatDefaultValues);
        }

        private static void CopyDictionaryToLists<T>(Dictionary<int, T> dictionary, out List<int> keys, out List<T> values)
        {
            if (dictionary == null || dictionary.Count <= 0)
            {
                keys = null;
                values = null;
            }
            else
            {
                int count = dictionary.Count;

                keys = new List<int>(count);
                values = new List<T>(count);

                foreach (var kv in dictionary)
                {
                    keys.Add(kv.Key);
                    values.Add(kv.Value);
                }
            }
        }

        private static void CopyListsToDictionary<T>(List<int> keys, List<T> values, ref Dictionary<int, T> dictionary)
        {
            if (keys == null || values == null)
            {
                dictionary = null;
            }
            else
            {
                int count = Math.Min(keys.Count, values.Count);
                if (count <= 0)
                {
                    dictionary = null;
                }
                else
                {
                    if (dictionary == null)
                    {
                        dictionary = new Dictionary<int, T>();
                    }
                    else
                    {
                        dictionary.Clear();
                    }

                    for (int i = 0; i < count; ++i)
                    {
                        dictionary.Add(keys[i], values[i]);
                    }
                }
            }
        }
    }
}
