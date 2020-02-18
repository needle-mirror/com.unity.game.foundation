using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// UI Module for displayName and Id fields.
    /// Contains logic for converting displayName to a Id
    /// suggestion when Id field is empty.
    /// </summary>
    internal class ReadableNameIdEditor
    {
        private bool m_AutomaticIdGenerationMode;
        private readonly bool m_IdEditingAllowedMode;
        private readonly HashSet<string> m_OldIds;

        private static readonly GUIContent s_DisplayNameLabel = new GUIContent("Display Name", "This definition's name, as displayed in the UI and logs. Must be alphanumeric, with at least one letter. Spaces, special characters, and underscores are allowed.");
        private static readonly GUIContent s_IdLabel = new GUIContent("Id", "This definition's identifier within the application. May be visible in the UI as well as code and logs. Must be alphanumeric, with at least one letter.  Dashes (-) and underscores (_) allowed.");

        public ReadableNameIdEditor(bool createNewMode, HashSet<string> oldIds)
        {
            m_AutomaticIdGenerationMode = createNewMode;
            m_IdEditingAllowedMode = createNewMode;

            m_OldIds = oldIds;
        }

        /// <summary>
        /// Draws UI input fields for displayName and Id.
        /// Will create a suggested Id based on input displayName based
        /// on the following conditions: displayName has a value, Id has
        /// not been edited manually or is blank, displayName field loses focus by
        /// tab or mouse click event, and item is being created new (vs editing existing item).
        /// ref parameters may change what has been passed in.
        /// </summary>
        /// <param name="itemId">Text to display for Id text field.</param>
        /// <param name="displayName">Text to display for display name text field.</param>
        public void DrawReadableNameIdFields(ref string itemId, ref string displayName)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUI.SetNextControlName("displayName");
                displayName = EditorGUILayout.TextField(s_DisplayNameLabel, displayName);

                if (m_IdEditingAllowedMode)
                {
                    ConvertIdIfNecessary(ref itemId, ref displayName);

                    GUI.changed = false;
                    GUI.SetNextControlName("id");
                    itemId = EditorGUILayout.TextField(s_IdLabel, itemId);
                    if (GUI.changed)
                    {
                        m_AutomaticIdGenerationMode = false;
                    }

                    if (HasRegisteredId(itemId))
                    {
                        EditorGUILayout.HelpBox("The current Id conflicts with existing Ids.", MessageType.Error);
                    }
                    else if (!string.IsNullOrWhiteSpace(itemId) && !CollectionEditorTools.IsValidId(itemId))
                    {
                        EditorGUILayout.HelpBox("The current Id is not valid. A valid Id is alphanumeric, with at least one letter. Dashes (-) and underscores (_) allowed.", MessageType.Error);
                    }
                }
                else
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(s_IdLabel, GUILayout.Width(145));
                        EditorGUILayout.SelectableLabel(itemId, GUILayout.Height(15), GUILayout.ExpandWidth(true));
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the proposed Id will conflict with those registered in the system that this ReadableNameIdEditor object exists in.
        /// </summary>
        /// <param name="itemId">Id to check</param>
        /// <returns>Whether this Id is problematic</returns>
        public bool HasRegisteredId(string itemId)
        {
            return m_OldIds.Contains(itemId);
        }

        private void ConvertIdIfNecessary(ref string itemId, ref string displayName)
        {
            var e = Event.current;
            var desiredEvent = e.Equals(Event.KeyboardEvent("tab")) || e.type.Equals(EventType.MouseDown);
            var desiredControlFocus = GUI.GetNameOfFocusedControl() == "displayName" || GUI.GetNameOfFocusedControl() == "id";

            if (!desiredEvent
                || !desiredControlFocus
                || string.IsNullOrEmpty(displayName)
                || (!m_AutomaticIdGenerationMode && !string.IsNullOrEmpty(itemId)))
            {
                return;
            }

            if (string.IsNullOrEmpty(itemId))
            {
                m_AutomaticIdGenerationMode = true;
            }

            itemId = CollectionEditorTools.CraftUniqueId(displayName, m_OldIds);
        }
    }
}
