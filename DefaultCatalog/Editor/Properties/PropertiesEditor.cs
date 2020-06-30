using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.DefaultCatalog;

namespace UnityEditor.GameFoundation.DefaultCatalog
{
    abstract class PropertiesEditor
    {
        public static readonly GUIContent staticPropertiesLabel = new GUIContent(
            "Static Properties",
            "Store a set of fixed data inside your catalog item you can read at runtime.");

        protected static readonly GUIContent k_KeyLabel = new GUIContent(
            "Key",
            "This property's identifier. May be visible in the UI as well as code and logs. Must be alphanumeric, with at least one letter. Dashes (-) and underscores (_) allowed.");

        protected static readonly GUIContent k_TypeLabel = new GUIContent(
            "Type",
            "This property's value type. It cannot be changed at runtime.");

        protected static readonly GUIContent k_ValueLabel = new GUIContent(
            "Value",
            "This property's value. It cannot be changed at runtime.");

        protected static readonly GUIContent k_AddButton = new GUIContent(
            "Add",
            "A property can be added only if the new key isn't already used and is alphanumeric, with at least one letter, dashes (-) and underscores (_) allowed.");

        protected static readonly GUIContent k_EmptyStaticCollectionLabel
            = new GUIContent("No static properties configured");

        protected const float k_KeyWidth = 140;

        protected const float k_TypeWidth = 120;

        protected const float k_ActionButtonWidth = 30;

        protected static string[] s_PropertyTypeNames =
        {
            k_LongPropertyDisplayName,
            k_DoublePropertyDisplayName,
            nameof(PropertyType.Bool),
            nameof(PropertyType.String),
        };

        const string k_LongPropertyDisplayName = "Integer number";

        const string k_DoublePropertyDisplayName = "Real number";

        protected static GUIContent GetLabelFor(PropertyType type)
        {
            switch (type)
            {
                case PropertyType.Long:
                    return new GUIContent(
                        k_LongPropertyDisplayName,
                        "This property can store integer values in the range of a long.\nIt can be cast into a long or an int if the value is small enough.");

                case PropertyType.Double:
                    return new GUIContent(
                        k_DoublePropertyDisplayName,
                        "This property can store floating-point values in the range of a double.\nIt can be cast into a float or a double.");

                case PropertyType.Bool:
                    return new GUIContent(
                        type.ToString(),
                        "This property stores a boolean value.\nIt can be cast into a bool.");

                case PropertyType.String:
                    return new GUIContent(
                        type.ToString(),
                        "This property stores a string value.\nIt can be cast into a string.");

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

    class PropertiesEditor<TCatalogItemAsset> : PropertiesEditor
        where TCatalogItemAsset : CatalogItemAsset
    {
        protected TCatalogItemAsset m_Asset;

        string m_NewPropertyName = "";

        PropertyType m_SelectedPropertyType;

        public void SelectItem(TCatalogItemAsset asset)
        {
            m_Asset = asset;

            m_NewPropertyName = "";

            m_SelectedPropertyType = default;
        }

        public void Draw()
        {
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                var properties = GetAssetProperties();

                DrawExistingProperties(properties);

                //Draw horizontal separator.
                var separator = EditorGUILayout.GetControlRect(false, 1);
                EditorGUI.DrawRect(separator, EditorGUIUtility.isProSkin ? Color.black : Color.gray);

                DrawPropertyCreation(properties);

                if (changeCheck.changed)
                    EditorUtility.SetDirty(m_Asset);
            }
        }

        void DrawExistingProperties(Dictionary<string, Property> properties)
        {
            //Draw list header.
            using (new GUILayout.HorizontalScope(GameFoundationEditorStyles.tableViewToolbarStyle))
            {
                GUILayout.Label(
                    k_KeyLabel,
                    GameFoundationEditorStyles.tableViewToolbarTextStyle,
                    GUILayout.Width(k_KeyWidth));
                GUILayout.Label(
                    k_TypeLabel,
                    GameFoundationEditorStyles.tableViewToolbarTextStyle,
                    GUILayout.Width(k_TypeWidth));
                GUILayout.Label(GetValueLabel(), GameFoundationEditorStyles.tableViewToolbarTextStyle);

                //"X" column.
                GUILayout.Space(k_ActionButtonWidth);
            }

            if (properties.Count <= 0)
            {
                EditorGUILayout.Space();

                GUILayout.Label(GetEmptyCollectionLabel(), GameFoundationEditorStyles.centeredGrayLabel);

                EditorGUILayout.Space();

                return;
            }

            //Duplicate keys to be able to edit the dictionary in the foreach loop.
            var propertyKeys = new List<string>(properties.Keys);
            string toDelete = null;
            foreach (var propertyKey in propertyKeys)
            {
                //Draw row: Property key | value type | default value | delete button
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(propertyKey, GUILayout.Width(k_KeyWidth));

                    var property = properties[propertyKey];
                    var propertyLabel = GetLabelFor(property.type);
                    GUILayout.Label(propertyLabel, GUILayout.Width(k_TypeWidth));

                    var controlName = $"valueField{propertyKey}";
                    GUI.SetNextControlName(controlName);

                    switch (property.type)
                    {
                        case PropertyType.Long:
                        {
                            property.longValue = EditorGUILayout.LongField(property.longValue);

                            break;
                        }

                        case PropertyType.Double:
                        {
                            property.doubleValue = EditorGUILayout.DoubleField(property.doubleValue);

                            break;
                        }

                        case PropertyType.Bool:
                        {
                            property.boolValue = EditorGUILayout.Toggle(property.boolValue);

                            break;
                        }

                        case PropertyType.String:
                        {
                            property.stringValue = EditorGUILayout.TextField(property.stringValue);

                            break;
                        }

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    //Don't forget to update the entry since we are working with structs.
                    properties[propertyKey] = property;

                    if (GUILayout.Button(
                        "X",
                        GameFoundationEditorStyles.tableViewButtonStyle,
                        GUILayout.Width(k_ActionButtonWidth)))
                    {
                        toDelete = propertyKey;
                    }
                }
            }

            //Do any actual deletion outside the rendering loop to prevent sync issues.
            if (toDelete != null)
                properties.Remove(toDelete);
        }

        void DrawPropertyCreation(IDictionary<string, Property> properties)
        {
            using (new GUILayout.HorizontalScope())
            {
                m_NewPropertyName = EditorGUILayout.TextField(
                    m_NewPropertyName,
                    GUILayout.Width(k_KeyWidth));

                m_SelectedPropertyType = (PropertyType)EditorGUILayout.Popup(
                    (int)m_SelectedPropertyType,
                    s_PropertyTypeNames,
                    GUILayout.Width(k_TypeWidth));

                GUILayout.FlexibleSpace();

                var disabled = !UnityEngine.GameFoundation.Tools.IsValidId(m_NewPropertyName)
                    || properties.ContainsKey(m_NewPropertyName);
                using (new EditorGUI.DisabledScope(disabled))
                {
                    var doAddProperty = GUILayout.Button(
                        k_AddButton,
                        GameFoundationEditorStyles.tableViewButtonStyle,
                        GUILayout.Width(k_ActionButtonWidth));

                    if (doAddProperty)
                    {
                        var property = new Property
                        {
                            type = m_SelectedPropertyType
                        };
                        properties.Add(m_NewPropertyName, property);

                        m_SelectedPropertyType = default;
                        m_NewPropertyName = "";

                        //Lose focus to avoid a weird issue where the new property's
                        //default value will display its key. Even if it is a number.
                        GUI.FocusControl(null);
                    }
                }
            }
        }

        protected virtual Dictionary<string, Property> GetAssetProperties()
            => m_Asset.staticProperties;

        protected virtual GUIContent GetValueLabel()
            => k_ValueLabel;

        protected virtual GUIContent GetEmptyCollectionLabel()
            => k_EmptyStaticCollectionLabel;
    }
}
