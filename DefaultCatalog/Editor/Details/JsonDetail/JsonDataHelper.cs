using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation.DefaultCatalog.Details
{
    /// <summary>
    /// Utility & extensions functions to edit <see cref="JsonData"/>.
    /// </summary>
    [Obsolete]
    public static class JsonDataHelper
    {
        static readonly Dictionary<string, object> k_ValueTypeNames = new Dictionary<string, object>
        {
            ["None"] = null,
            ["Bool"] = false,
            ["Int"] = 0,
            ["Long"] = (long)0,
            ["Float"] = 0.0f,
            ["Double"] = 0.0d,
            ["String"] = "",
            ["DateTime"] = new DateTime(),
            ["BoolArray"] = new bool[0],
            ["IntArray"] = new int[0],
            ["LongArray"] = new long[0],
            ["FloatArray"] = new float[0],
            ["DoubleArray"] = new double[0],
            ["StringArray"] = new string[0],
            ["DateTimeArray"] = new DateTime[0],

            //TODO: Dictionary drawer needs to be fixed to handle nested custom type & nested array draw.
//            ["Dictionary"] = new Dictionary<string, object>(),
        };

        static readonly string[] k_SelectableTypeName;

        static JsonDataHelper()
        {
            var interfaceType = typeof(IDictionaryConvertible);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var assemblyType in assembly.GetTypes())
                {
                    if (assemblyType.IsPublic
                        && !assemblyType.IsAbstract
                        && interfaceType.IsAssignableFrom(assemblyType)
                        && assemblyType != typeof(JsonData))
                    {
                        var assemblyTypeName = assemblyType.Name;
                        try
                        {
                            var emptyValue = Activator.CreateInstance(assemblyType);
                            k_ValueTypeNames.Add(assemblyTypeName, emptyValue);
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning($"Couldn't create an empty value for \"{assemblyTypeName}\".");

                            continue;
                        }

                        var emptyArray = Array.CreateInstance(assemblyType, 0);
                        k_ValueTypeNames.Add(assemblyTypeName + " Array", emptyArray);
                    }
                }
            }

            k_SelectableTypeName = new string[k_ValueTypeNames.Count];
            var typeNames = new List<string>(k_ValueTypeNames.Keys);
            for (var i = 0; i < typeNames.Count; i++)
            {
                var typeName = typeNames[i];
                k_SelectableTypeName[i] = typeName;
            }
        }

        /// <summary>
        /// Draws a widget to let user create new <see cref="JsonData"/> entry.
        /// </summary>
        /// <param name="isNewEntryNameValid">
        /// Predicate to use to determine if the input name is valid.
        /// </param>
        /// <param name="fieldName">
        /// Name written by the user.
        /// Updated only if the predicate validated it.
        /// </param>
        /// <param name="popupIndex">
        /// Index selected by the user from the popup.
        /// </param>
        /// <param name="emptyEntry">
        /// Default entry created if the user validated the creation.
        /// </param>
        /// <returns>
        /// Returns true if the user created a new valid entry;
        /// returns false otherwise.
        /// </returns>
        public static bool DrawAddNewEntry(
            Func<string, bool> isNewEntryNameValid,
            ref string fieldName,
            ref int popupIndex,
            out object emptyEntry)
        {
            EditorGUILayout.LabelField("Add new entry");

            using (new EditorGUILayout.VerticalScope())
            {
                var inputFieldName = EditorGUILayout.TextField("Entry Name", fieldName);

                //Regex based on C# identifier names
                //(see: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/identifier-names)
                if (!string.IsNullOrEmpty(inputFieldName)
                    && Regex.IsMatch(inputFieldName, @"^([a-zA-Z_])\w*$"))
                    fieldName = inputFieldName;

                popupIndex = EditorGUILayout.Popup(
                    "fieldType",
                    popupIndex,
                    k_SelectableTypeName);

                var isReadyToCreate = !string.IsNullOrEmpty(fieldName)
                    && isNewEntryNameValid(fieldName)
                    && popupIndex != 0;
                using (new EditorGUI.DisabledScope(!isReadyToCreate))
                {
                    if (GUILayout.Button("+", GameFoundationEditorStyles.createButtonStyle)
                        && k_ValueTypeNames.TryGetValue(k_SelectableTypeName[popupIndex], out emptyEntry))
                        return true;
                }
            }

            emptyEntry = null;

            return false;
        }

        /// <summary>
        /// Parse the given object to make it serializable into a JSON. 
        /// </summary>
        /// <param name="value">
        /// The object to prepare for serialization.
        /// Handled types are:
        /// - All base Types.
        /// - <see cref="IDictionaryConvertible"/>.
        /// - Arrays
        /// - Lists
        /// - Dictionary{string, object}
        /// </param>
        /// <returns>
        /// Returns a JSON-serialization-ready version of the given value.
        /// </returns>
        public static object PrepareValueForSerialization(object value)
        {
            if (value == null)
                return null;

            var valueType = value.GetType();
            switch (value)
            {
                case IDictionaryConvertible convertible:
                {
                    var preparedDictionary = convertible.ToDictionary();
                    if (preparedDictionary == null)
                        return null;

                    //Recursive preparation for dictionary entries.
                    var keys = new List<string>(preparedDictionary.Keys);
                    foreach (var key in keys)
                    {
                        var preparedValue = PrepareValueForSerialization(preparedDictionary[key]);
                        if (preparedValue == null)
                        {
                            preparedDictionary.Remove(key);
                            continue;
                        }

                        preparedDictionary[key] = preparedValue;
                    }

                    return preparedDictionary;
                }

                case byte _:
                case sbyte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                case ulong _:
                case float _:
                case double _:
                case bool _:
                case char _:
                case string _:
                case DateTime _:
                    return value;

                case Array array:
                {
                    //Recursive preparation for arrays.
                    var preparedList = new List<object>(array.Length);
                    for (var i = 0; i < array.Length; ++i)
                    {
                        var preparedValue = PrepareValueForSerialization(array.GetValue(i));
                        if (preparedValue != null)
                            preparedList.Add(preparedValue);
                    }

                    return preparedList;
                }

                case IList array
                    when valueType.IsConstructedGenericType
                    && valueType.GetGenericTypeDefinition() == typeof(List<>):
                {
                    //Recursive preparation for lists.
                    var preparedList = new List<object>(array.Count);
                    for (var i = 0; i < array.Count; ++i)
                    {
                        var preparedValue = PrepareValueForSerialization(array[i]);
                        if (preparedValue != null)
                            preparedList.Add(preparedValue);
                    }

                    return preparedList;
                }

                case Dictionary<string, object> preparedDictionary:
                {
                    var keys = new List<string>(preparedDictionary.Keys);
                    foreach (var key in keys)
                    {
                        var preparedValue = PrepareValueForSerialization(preparedDictionary[key]);
                        if (preparedValue == null)
                        {
                            preparedDictionary.Remove(key);
                            continue;
                        }

                        preparedDictionary[key] = preparedValue;
                    }

                    return preparedDictionary;
                }

                default:
                    return null;
            }
        }

        /// <summary>
        /// Add the given value to this <see cref="JsonData"/> with the field name.
        /// Does nothing if the field name is already used.
        /// </summary>
        /// <returns>
        /// Returns true if the value has been successfully added;
        /// returns false otherwise.
        /// </returns>
        public static bool TryAddData(this JsonData @this, string fieldName, object value)
        {
            if (@this.userDefined.ContainsKey(fieldName))
                return false;

            var entryValue = PrepareValueForSerialization(value);
            if (value == null)
                return false;

            @this.userDefined[fieldName] = entryValue;

            var genericType = value.GetType();

            //Create mapKey based on the value's type.
            var mapKey = GetJsonDataMapKeyFor(genericType);

            //Add or update the type map.
            if (@this.gfTypeMap.TryGetValue(mapKey, out var typeMap))
                typeMap.Add(fieldName);
            else
                @this.gfTypeMap.Add(mapKey, new List<string> { fieldName });

            return true;
        }

        static string GetJsonDataMapKeyFor(Type type)
        {
            string mapKey;
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                mapKey = GetJsonDataMapKeyFor(elementType) + JsonData.arrayTypeNameExtension;
            }
            else if (type.IsGenericType
                && typeof(IList).IsAssignableFrom(type))
            {
                var genericType = type.GetGenericArguments()[0];
                mapKey = GetJsonDataMapKeyFor(genericType) + JsonData.arrayTypeNameExtension;
            }
            else if (type == typeof(Dictionary<string, object>))
                mapKey = JsonData.dictionaryTypeName;
            else if (typeof(IDictionaryConvertible).IsAssignableFrom(type))
                mapKey = type.FullName + JsonData.customTypeNameExtension;
            else
                mapKey = type.FullName;

            return mapKey;
        }

        /// <summary>
        /// Removes the field of the given name from this <see cref="JsonData"/>.
        /// Does nothing if the field name doesn't exist.
        /// </summary>
        /// <returns>
        /// Returns true if the value has been successfully removed;
        /// returns false otherwise.
        /// </returns>
        public static bool TryRemoveData(this JsonData @this, string fieldName)
        {
            if (!@this.userDefined.Remove(fieldName))
                return false;

            //Don't forget to remove the field from the meta data field.
            var typeMapKey = "";
            foreach (var typeMaps in @this.gfTypeMap)
            {
                if (typeMaps.Value.Contains(fieldName))
                {
                    typeMapKey = typeMaps.Key;
                    break;
                }
            }

            @this.gfTypeMap.Remove(typeMapKey);

            return true;
        }
    }
}
