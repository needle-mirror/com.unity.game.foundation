using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.MiniJson;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(JsonDetailAsset))]
    class JsonDetailAssetEditor : BaseDetailAssetEditor
    {
        //Need to be static because the editor is constructed every frame.
        static string s_NewFieldName;

        //Need to be static because the editor is constructed every frame.
        static int s_NewFieldTypeIndex;

        JsonDetailAsset m_TargetAsset;

        void OnEnable()
        {
            if (targets.IsNullOrEmpty())
                return;

            m_TargetAsset = (JsonDetailAsset)target;
        }

        public override void OnInspectorGUI()
        {
            if (targets.IsNullOrEmpty())
                return;

            serializedObject.Update();

            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                //Create empty object if nothing is set yet.
                if (string.IsNullOrEmpty(m_TargetAsset.m_JsonData))
                    m_TargetAsset.m_JsonData = GenerateEmptyJson();

                var rawDeserialization = Json.Deserialize(m_TargetAsset.m_JsonData);

                //Reset json data if it has been corrupted.
                if (!(rawDeserialization is Dictionary<string, object> rawDictionary))
                {
                    m_TargetAsset.m_JsonData = GenerateEmptyJson();
                    rawDictionary = new Dictionary<string, object>();
                }

                var foo = new JsonData().FillFromDictionary(rawDictionary);

#if GAME_FOUNDATION_DEBUG
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.LabelField("Raw JSON data");
                    EditorGUILayout.TextArea(
                        m_TargetAsset.m_JsonData,
                        GUILayout.ExpandWidth(true),
                        GUILayout.MaxWidth(400));
                }
#endif

                //Draw into a try block to avoid the inspector to be completely broken if an error occur.
                try
                {
                    DrawAssetEditor(foo);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                if (changeCheck.changed)
                {
                    //Re-serialize the dictionary into the asset.
                    rawDictionary = foo.ToDictionary();
                    m_TargetAsset.m_JsonData = Json.Serialize(rawDictionary);

                    serializedObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(m_TargetAsset);
                }
            }
        }

        static void DrawAssetEditor(JsonData jsonData)
        {
            //Create list from keys collection to be able to edit the dictionary inside the foreach.
            var typeMapKeys = new List<string>(jsonData.gfTypeMap.Keys);

            //Edit existing entries.
            foreach (var typeToDraw in typeMapKeys)
            {
                var fieldNames = jsonData.gfTypeMap[typeToDraw];
                var fieldDrawer = GetDrawerFor(typeToDraw);
                var isMultiLineField = fieldDrawer is JsonArrayDrawer
                    || fieldDrawer is JsonDictionaryDrawer
                    || fieldDrawer is JsonDictionaryConvertibleDrawer;
                foreach (var fieldName in fieldNames)
                {
                    using (new EditorGUILayout.VerticalScope(GameFoundationEditorStyles.boxStyle))
                    {
                        bool doDelete;

                        //Field update.
                        var fieldValue = jsonData.userDefined[fieldName];

                        //Field deletion.
                        using (new EditorGUILayout.HorizontalScope(GUIStyle.none))
                        {
                            if (isMultiLineField)
                            {
                                EditorGUILayout.LabelField(fieldName);

                                GUILayout.FlexibleSpace();
                            }
                            else
                                fieldDrawer.DrawEditor(fieldName, ref fieldValue);

                            doDelete = GUILayout.Button(
                                "",
                                GameFoundationEditorStyles.deleteButtonStyle,
                                GUILayout.Width(20.0f));
                        }

                        if (isMultiLineField)
                            fieldDrawer.DrawEditor(fieldName, ref fieldValue);

                        if (doDelete)
                            jsonData.TryRemoveData(fieldName);
                        else
                        {
                            //Don't forget to reassign the value to the dictionary in case it was a ValueType.
                            jsonData.userDefined[fieldName] = fieldValue;
                        }
                    }
                }
            }

            EditorGUILayout.Space();

            //Add new entry.
            if (JsonDataHelper.DrawAddNewEntry(
                newEntryName => !jsonData.userDefined.ContainsKey(newEntryName),
                ref s_NewFieldName,
                ref s_NewFieldTypeIndex,
                out var emptyValue))
            {
                jsonData.TryAddData(s_NewFieldName, emptyValue);

                //Don't forget to reset new field data.
                s_NewFieldName = "";
                s_NewFieldTypeIndex = 0;
            }
        }

        static string GenerateEmptyJson()
        {
            var emptyData = new JsonData().ToDictionary();
            var json = Json.Serialize(emptyData);

            return json;
        }

        static JsonFieldDrawer GetDrawerFor(string typeName)
        {
            if (typeName.EndsWith(JsonData.arrayTypeNameExtension))
            {
                var elementTypeName = typeName.Remove(typeName.Length - JsonData.arrayTypeNameExtension.Length);
                var subDrawer = GetDrawerFor(elementTypeName);

                return new JsonArrayDrawer(subDrawer);
            }

            if (typeName.EndsWith(JsonData.customTypeNameExtension))
            {
                var customTypeName = typeName.Remove(typeName.Length - JsonData.customTypeNameExtension.Length);

                return new JsonDictionaryConvertibleDrawer(customTypeName);
            }

            if (typeName == typeof(byte).FullName
                || typeName == typeof(sbyte).FullName
                || typeName == typeof(short).FullName
                || typeName == typeof(ushort).FullName
                || typeName == typeof(int).FullName
                || typeName == typeof(uint).FullName
                || typeName == typeof(long).FullName
                || typeName == typeof(ulong).FullName)
                return new JsonLongDrawer();

            if (typeName == typeof(float).FullName
                || typeName == typeof(double).FullName
                || typeName == typeof(decimal).FullName)
                return new JsonDoubleDrawer();

            if (typeName == typeof(bool).FullName)
                return new JsonBoolDrawer();

            if (typeName == typeof(DateTime).FullName)
                return new JsonDateDrawer();

            if (typeName == typeof(string).FullName)
                return new JsonStringDrawer();

            if (typeName == JsonData.dictionaryTypeName)
                return new JsonDictionaryDrawer();

            throw new ArgumentException($"No drawer found for the type \"{typeName}\".");
        }
    }
}
