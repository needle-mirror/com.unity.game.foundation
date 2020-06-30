using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation.DefaultCatalog.Details
{
    /// <summary>
    /// A utility class to draw/edit json data for a given type.
    /// </summary>
    /// <remarks>
    /// TODO: Optimize these drawers (especially the dictionaries).
    /// </remarks>
    [Obsolete]
    abstract class JsonFieldDrawer
    {
        public abstract Type serializedType { get; }

        public abstract object CreateEmptyValue();

        public abstract void DrawEditor(string label, ref object value);

        protected static JsonFieldDrawer GetDrawerFor(Type entryType)
        {
            if (entryType == null)
                return null;

            if (entryType == typeof(byte)
                || entryType == typeof(sbyte)
                || entryType == typeof(short)
                || entryType == typeof(ushort)
                || entryType == typeof(int)
                || entryType == typeof(uint)
                || entryType == typeof(long)
                || entryType == typeof(ulong))
                return new JsonLongDrawer();

            if (typeof(IList<byte>).IsAssignableFrom(entryType)
                || typeof(IList<sbyte>).IsAssignableFrom(entryType)
                || typeof(IList<short>).IsAssignableFrom(entryType)
                || typeof(IList<ushort>).IsAssignableFrom(entryType)
                || typeof(IList<int>).IsAssignableFrom(entryType)
                || typeof(IList<uint>).IsAssignableFrom(entryType)
                || typeof(IList<long>).IsAssignableFrom(entryType)
                || typeof(IList<ulong>).IsAssignableFrom(entryType))
                return new JsonArrayDrawer(new JsonLongDrawer());

            if (entryType == typeof(float)
                || entryType == typeof(double)
                || entryType == typeof(decimal))
                return new JsonDoubleDrawer();

            if (typeof(IList<float>).IsAssignableFrom(entryType)
                || typeof(IList<double>).IsAssignableFrom(entryType)
                || typeof(IList<decimal>).IsAssignableFrom(entryType))
                return new JsonArrayDrawer(new JsonDoubleDrawer());

            if (entryType == typeof(bool))
                return new JsonBoolDrawer();

            if (typeof(IList<bool>).IsAssignableFrom(entryType))
                return new JsonArrayDrawer(new JsonBoolDrawer());

            if (entryType == typeof(DateTime))
                return new JsonDateDrawer();

            if (typeof(IList<DateTime>).IsAssignableFrom(entryType))
                return new JsonArrayDrawer(new JsonDateDrawer());

            if (entryType == typeof(string))
                return new JsonStringDrawer();

            if (typeof(IList<string>).IsAssignableFrom(entryType))
                return new JsonArrayDrawer(new JsonStringDrawer());

            if (entryType == typeof(Dictionary<string, object>))
                return new JsonDictionaryDrawer();

            if (entryType.IsEnum)
                return new JsonEnumDrawer(entryType);

            return new JsonDictionaryConvertibleDrawer(entryType.Name);
        }
    }

    [Obsolete]
    class JsonLongDrawer : JsonFieldDrawer
    {
        public override Type serializedType => typeof(long);

        public override object CreateEmptyValue() => 0;

        public override void DrawEditor(string label, ref object value)
        {
            var castValue = Convert.ToInt64(value);
            value = EditorGUILayout.LongField(label, castValue);
        }
    }

    [Obsolete]
    class JsonDoubleDrawer : JsonFieldDrawer
    {
        public override Type serializedType => typeof(double);

        public override object CreateEmptyValue() => 0.0d;

        public override void DrawEditor(string label, ref object value)
        {
            var castValue = Convert.ToDouble(value);
            value = EditorGUILayout.DoubleField(label, castValue);
        }
    }

    [Obsolete]
    class JsonBoolDrawer : JsonFieldDrawer
    {
        public override Type serializedType => typeof(bool);

        public override object CreateEmptyValue() => false;

        public override void DrawEditor(string label, ref object value)
        {
            var castValue = Convert.ToBoolean(value);
            value = EditorGUILayout.Toggle(label, castValue);
        }
    }

    [Obsolete]
    class JsonStringDrawer : JsonFieldDrawer
    {
        public override Type serializedType => typeof(string);

        public override object CreateEmptyValue() => "";

        public override void DrawEditor(string label, ref object value)
        {
            var castValue = value.ToString();
            value = EditorGUILayout.TextField(label, castValue);
        }
    }

    [Obsolete]
    class JsonDateDrawer : JsonFieldDrawer
    {
        public override Type serializedType => typeof(string);

        public override object CreateEmptyValue()
            => new DateTime().ToString(CultureInfo.InvariantCulture);

        public override void DrawEditor(string label, ref object value)
        {
            //Parse serialized date
            var stringifiedDate = value.ToString();
            if (!DateTime.TryParse(stringifiedDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                date = new DateTime();

            //Display date in a standard format
            stringifiedDate = date.ToString("s", CultureInfo.InvariantCulture);
            stringifiedDate = EditorGUILayout.TextField(label, stringifiedDate);

            //Update date only if it is valid
            if (DateTime.TryParse(stringifiedDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var inputDate))
                value = inputDate.ToString("s", CultureInfo.InvariantCulture);
        }
    }

    [Obsolete]
    class JsonEnumDrawer : JsonFieldDrawer
    {
        string[] m_EnumValueNames;
        int[] m_EnumValues;

        public override Type serializedType => typeof(int);

        public JsonEnumDrawer(Type enumType)
        {
            var rawValues = Enum.GetValues(enumType);
            var intValues = new List<int>(rawValues.Length);
            var isLongEnum = false;
            try
            {
                foreach (var rawValue in rawValues)
                {
                    var intValue = Convert.ToInt32(rawValue);
                    intValues.Add(intValue);
                }
            }
            catch (OverflowException)
            {
                //Cast can fail if the enum is a long
                Debug.LogWarning("Long values won't be supported by this enum drawer.");

                isLongEnum = true;
            }

            m_EnumValues = intValues.ToArray();

            if (isLongEnum)
            {
                var valueCount = m_EnumValues.Length;
                m_EnumValueNames = new string[valueCount];
                var allNames = Enum.GetNames(enumType);

                for (var i = 0; i < valueCount; ++i)
                    m_EnumValueNames[i] = allNames[i];
            }
            else
                m_EnumValueNames = Enum.GetNames(enumType);
        }

        public override object CreateEmptyValue() => 0;

        public override void DrawEditor(string label, ref object value)
        {
            var castValue = Convert.ToInt32(value);

            value = EditorGUILayout.IntPopup(label, castValue, m_EnumValueNames, m_EnumValues);
        }
    }

    [Obsolete]
    class JsonArrayDrawer : JsonFieldDrawer
    {
        public override Type serializedType => typeof(Array);

        JsonFieldDrawer m_ItemDrawer;

        public JsonArrayDrawer(JsonFieldDrawer itemDrawer)
            => m_ItemDrawer = itemDrawer;

        public override object CreateEmptyValue()
            => Array.CreateInstance(serializedType, 0);

        public override void DrawEditor(string label, ref object value)
        {
            if (!(value is IList rawArrayValue))
                throw new ArgumentException("The given value isn't an array.", nameof(value));

            var elementType = m_ItemDrawer.serializedType;
            var isMultiLineField = m_ItemDrawer is JsonDictionaryDrawer
                || m_ItemDrawer is JsonArrayDrawer
                || m_ItemDrawer is JsonDictionaryConvertibleDrawer;
            var rawLength = rawArrayValue.Count;
            var localArray = Array.CreateInstance(elementType, rawLength);
            var hasDeleted = false;

            using (new EditorGUI.IndentLevelScope(1))
            using (new EditorGUILayout.VerticalScope(GUIStyle.none))
            {
                for (var i = 0; i < rawLength; i++)
                {
                    var item = rawArrayValue[i];

                    bool doDelete;

                    //Field deletion.
                    using (new EditorGUILayout.HorizontalScope(GUIStyle.none))
                    {
                        var elementLabel = $"Element {i.ToString()}";
                        if (isMultiLineField)
                            EditorGUILayout.LabelField(elementLabel);
                        else
                            m_ItemDrawer.DrawEditor(elementLabel, ref item);

                        doDelete = GUILayout.Button(
                            "",
                            GameFoundationEditorStyles.deleteButtonStyle,
                            GUILayout.Width(20.0f));
                    }

                    if (doDelete)
                    {
                        hasDeleted = true;
                        var newLocal = Array.CreateInstance(elementType, rawLength - 1);
                        if (i > 0)
                            Array.Copy(localArray, newLocal, i);

                        localArray = newLocal;
                    }
                    else
                    {
                        if (isMultiLineField)
                            m_ItemDrawer.DrawEditor(null, ref item);

                        if (hasDeleted)
                            localArray.SetValue(item, i - 1);
                        else
                            localArray.SetValue(item, i);
                    }
                }
            }

            if (GUILayout.Button("Add Element"))
            {
                var newLocal = Array.CreateInstance(elementType, rawLength + 1);
                Array.Copy(localArray, newLocal, rawLength);

                localArray = newLocal;

                var emptyItem = m_ItemDrawer.CreateEmptyValue();
                localArray.SetValue(emptyItem, rawLength);
            }

            value = localArray;
        }
    }

    [Obsolete]
    class JsonDictionaryDrawer : JsonFieldDrawer
    {
        //Need to be static because the editor is constructed every frame.
        static string s_NewFieldName;

        //Need to be static because the editor is constructed every frame.
        static int s_NewFieldTypeIndex;

        public override Type serializedType
            => typeof(Dictionary<string, object>);

        public override object CreateEmptyValue()
            => new Dictionary<string, object>();

        public override void DrawEditor(string label, ref object value)
        {
            if (!(value is Dictionary<string, object> dictionaryValue))
                throw new ArgumentException("The given value isn't a compatible type.", nameof(value));

            using (new EditorGUI.IndentLevelScope(1))
            using (new EditorGUILayout.VerticalScope(GUIStyle.none))
            {
                var keys = new List<string>(dictionaryValue.Keys);

                foreach (var key in keys)
                {
                    var entryValue = dictionaryValue[key];
                    if (entryValue == null)
                    {
                        Debug.LogWarning($"Value for \"{key}\" is null, so this entry will be removed.");

                        dictionaryValue.Remove(key);

                        //TODO: Object needs to be set dirty.

                        continue;
                    }

                    //TODO: Custom types are considered as dictionaries.
                    var drawer = GetDrawerFor(entryValue.GetType());

                    var isMultiLine = drawer is JsonArrayDrawer
                        || drawer is JsonDictionaryConvertibleDrawer
                        || drawer is JsonDictionaryDrawer;

                    bool doDelete;
                    using (new EditorGUILayout.HorizontalScope(GUIStyle.none))
                    {
                        if (isMultiLine)
                            EditorGUILayout.LabelField(key);
                        else
                        {
                            drawer.DrawEditor(key, ref entryValue);
                            dictionaryValue[key] = entryValue;
                        }

                        GUILayout.FlexibleSpace();

                        doDelete = GUILayout.Button(
                            "",
                            GameFoundationEditorStyles.deleteButtonStyle,
                            GUILayout.Width(20.0f));
                    }

                    if (isMultiLine)
                    {
                        drawer.DrawEditor(key, ref entryValue);
                        dictionaryValue[key] = entryValue;
                    }

                    if (doDelete)
                        dictionaryValue.Remove(key);
                }

                if (JsonDataHelper.DrawAddNewEntry(
                    newEntryName => !dictionaryValue.ContainsKey(newEntryName),
                    ref s_NewFieldName,
                    ref s_NewFieldTypeIndex,
                    out var emptyValue))
                {
                    var preparedValue = JsonDataHelper.PrepareValueForSerialization(emptyValue);
                    dictionaryValue.Add(s_NewFieldName, preparedValue);

                    s_NewFieldName = "";
                    s_NewFieldTypeIndex = 0;
                }
            }

            value = dictionaryValue;
        }
    }

    [Obsolete]
    class JsonDictionaryConvertibleDrawer : JsonFieldDrawer
    {
        public override Type serializedType
            => typeof(Dictionary<string, object>);

        Dictionary<string, JsonFieldDrawer> m_MemberDrawers;

        public JsonDictionaryConvertibleDrawer(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException(nameof(typeName));

            var targetType = Type.GetType(
                typeName,
                assemblyName =>
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (assembly.GetName().Name == assemblyName.Name)
                            return assembly;
                    }

                    return null;
                },
                (assembly, searchedTypeName, isCaseSensitive) =>
                {
                    Type foundType = null;
                    if (assembly == null)
                    {
                        foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (TryFindType(domainAssembly, isCaseSensitive, searchedTypeName, out foundType))
                                return foundType;
                        }
                    }
                    else
                        TryFindType(assembly, isCaseSensitive, searchedTypeName, out foundType);

                    return foundType;
                });

            if (targetType == null)
                throw new ArgumentException("Impossible to find a type with the given name.", nameof(typeName));

            if (!(Activator.CreateInstance(targetType) is IDictionaryConvertible dummy))
                throw new ArgumentException($"The given type {typeName} ({targetType}) isn't a {nameof(IDictionaryConvertible)}.", nameof(typeName));

            var serializedData = dummy.ToDictionary();
            m_MemberDrawers = new Dictionary<string, JsonFieldDrawer>(serializedData.Count);
            const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var targetFields = targetType.GetFields(bindingFlags);
            var targetProperties = targetType.GetProperties(bindingFlags);

            foreach (var targetMemberName in new List<string>(serializedData.Keys))
            {
                Type memberType = null;
                foreach (var targetField in targetFields)
                {
                    if (targetField.Name == targetMemberName)
                    {
                        memberType = targetField.FieldType;
                        break;
                    }
                }

                if (memberType == null)
                {
                    foreach (var targetProperty in targetProperties)
                    {
                        if (targetProperty.Name == targetMemberName)
                        {
                            memberType = targetProperty.PropertyType;
                            break;
                        }
                    }
                }

                if (memberType == null)
                {
                    Debug.LogWarning($"Couldn't find a type for the member \"{targetMemberName}\". It will be skipped.");
                    continue;
                }

                var drawer = GetDrawerFor(memberType);
                if (drawer == null)
                {
                    Debug.LogWarning($"Couldn't find a drawer for the member \"{targetMemberName}\". It will be skipped.");
                    continue;
                }

                m_MemberDrawers.Add(targetMemberName, drawer);
            }
        }

        public override object CreateEmptyValue()
        {
            var emptyValue = new Dictionary<string, object>(m_MemberDrawers.Count);
            foreach (var entry in m_MemberDrawers)
                emptyValue.Add(entry.Key, entry.Value.CreateEmptyValue());

            return emptyValue;
        }

        public override void DrawEditor(string label, ref object value)
        {
            if (!(value is Dictionary<string, object> dictionaryValue))
                throw new ArgumentException("The given value isn't a compatible type.", nameof(value));

            using (new EditorGUI.IndentLevelScope(1))
            using (new EditorGUILayout.VerticalScope(GUIStyle.none))
            {
                foreach (var entry in m_MemberDrawers)
                {
                    var key = entry.Key;
                    var drawer = entry.Value;

                    if (!dictionaryValue.TryGetValue(key, out var entryValue))
                        entryValue = drawer.CreateEmptyValue();

                    if (drawer is JsonArrayDrawer
                        || drawer is JsonDictionaryDrawer)
                        EditorGUILayout.LabelField(key);

                    drawer.DrawEditor(key, ref entryValue);
                    dictionaryValue[key] = entryValue;
                }
            }

            value = dictionaryValue;
        }

        static bool TryFindType(Assembly assembly, bool isCaseSensitive, string searchedTypeName, out Type type)
        {
            foreach (var assemblyType in assembly.GetTypes())
            {
                if (isCaseSensitive)
                {
                    if (assemblyType.FullName == searchedTypeName)
                    {
                        type = assemblyType;

                        return true;
                    }
                }
                else if (assemblyType.FullName.Equals(searchedTypeName, StringComparison.InvariantCultureIgnoreCase))
                {
                    type = assemblyType;

                    return true;
                }
            }

            type = null;

            return false;
        }
    }
}
