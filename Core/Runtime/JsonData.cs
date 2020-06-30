using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.GameFoundation.MiniJson;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// A container for JSON data using string parsing to avoid reflection.
    /// </summary>
    [Obsolete]
    public sealed class JsonData : IDictionaryConvertible
    {
        /// <summary>
        /// Extension used to identify stored data arrays.
        /// </summary>
        public const string arrayTypeNameExtension = ".array";

        /// <summary>
        /// Type name used to identify stored Dictionary.
        /// </summary>
        public const string dictionaryTypeName = ".dictionary";

        /// <summary>
        /// Extension used to identify stored <see cref="IDictionaryConvertible"/> objects.
        /// </summary>
        public const string customTypeNameExtension = ".custom";

        internal Dictionary<string, object> userDefined = new Dictionary<string, object>();

        internal Dictionary<string, List<string>> gfTypeMap = new Dictionary<string, List<string>>();

        /// <param name="json">
        /// A JSON representing a <see cref="JsonData"/> instance.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if the given JSON isn't null nor empty and doesn't have a correct JSON format.
        /// </exception>
        public JsonData(string json = null)
        {
            //Nothing to convert.
            if (string.IsNullOrEmpty(json))
                return;

            var rawDeserializedValue = Json.Deserialize(json);
            if (!(rawDeserializedValue is Dictionary<string, object> rawDictionary))
                throw new ArgumentException("The given json isn't properly formatted.", nameof(json));

            FillFromDictionary(rawDictionary);
        }

        /// <summary>
        /// Try to find the data of the given built-in data type with the given name and deserialize it.
        /// </summary>
        /// <param name="fieldName">
        /// The name of the data to retrieve.
        /// </param>
        /// <param name="data">
        /// The deserialized data if it was found.
        /// </param>
        /// <typeparam name="T">
        /// Any built-in data type from C# except <see cref="char"/>.
        /// </typeparam>
        /// <returns>
        /// Returns true if the data was found and deserialized;
        /// returns false otherwise.
        /// </returns>
        public bool TryGetBuiltInData<T>(string fieldName, out T data)
            where T : IComparable, IConvertible, IComparable<T>, IEquatable<T>
        {
            //Early return for invalid names.
            if (string.IsNullOrEmpty(fieldName)
                || !userDefined.TryGetValue(fieldName, out var rawData))
            {
                Debug.LogWarning($"There is no \"{fieldName}\" entry in this detail.");
                data = default;

                return false;
            }

            var genericType = typeof(T);
            var genericTypeName = genericType.FullName;
            if (!IsBuiltInTypeName(genericTypeName))
                throw new Exception($"\"{genericTypeName}\" isn't handle by \"{nameof(TryGetBuiltInData)}\", use \"{nameof(TryGetCustomData)}\" instead.");

            var fieldTypeName = FindFieldTypeName(fieldName);

            if (TryGetConvertibleData(fieldTypeName, rawData, genericType, out data))
                return true;

            Debug.LogWarning($"Impossible to cast \"{fieldName}\" into a \"{genericTypeName}\".");

            data = default;

            return false;
        }

        /// <summary>
        /// Try to find the data with the given name and deserialize it.
        /// </summary>
        /// <param name="fieldName">
        /// The name of the data to retrieve.
        /// </param>
        /// <param name="data">
        /// The deserialized data if it was found.
        /// </param>
        /// <typeparam name="T">
        /// Any <see cref="IDictionaryConvertible"/> object that have a default constructor.
        /// </typeparam>
        /// <returns>
        /// Returns true if the data was found and deserialized;
        /// returns false otherwise.
        /// </returns>
        public bool TryGetCustomData<T>(string fieldName, out T data)
            where T : IDictionaryConvertible, new()
        {
            //Early return for invalid names.
            if (string.IsNullOrEmpty(fieldName)
                || !userDefined.TryGetValue(fieldName, out var rawData))
            {
                Debug.LogWarning($"There is no \"{fieldName}\" entry in this detail.");
                data = default;

                return false;
            }

            var genericType = typeof(T);
            var genericTypeName = genericType.FullName;
            var fieldTypeName = FindFieldTypeName(fieldName);

            //Not explicitly a CustomType.
            if (!fieldTypeName.EndsWith(customTypeNameExtension))
            {
                Debug.LogWarning($"\"{fieldName}\" is stored as a \"{fieldTypeName}\" and can't be deserialized into a \"{genericTypeName}\".");

                data = default;

                return false;
            }

            //TODO: Do we allow all IDictionaryConvertible to be deserialized into another IDictionaryConvertible ?
            var customTypeName = fieldTypeName.Remove(fieldTypeName.Length - customTypeNameExtension.Length);
            if (customTypeName != genericTypeName)
            {
                Debug.LogWarning($"\"{fieldName}\" is stored as a \"{customTypeName}\" and can't be deserialized into a \"{genericTypeName}\".");

                data = default;

                return false;
            }

            if (!(rawData is Dictionary<string, object> castData))
            {
                data = default;

                return true;
            }

            data = new T();
            data.FillFromDictionary(castData);

            return true;
        }

        /// <summary>
        /// Try to find the array of data with the given name and deserialize it.
        /// </summary>
        /// <param name="arrayName">
        /// The name of the array to retrieve.
        /// </param>
        /// <param name="dataArray">
        /// The deserialized array of data if it was found.
        /// </param>
        /// <typeparam name="T">
        /// Any built-in data type from C# except <see cref="char"/>.
        /// </typeparam>
        /// <returns>
        /// Returns true if the array was found and deserialized;
        /// returns false otherwise.
        /// </returns>
        public bool TryGetBuiltInDataArray<T>(string arrayName, out T[] dataArray)
            where T : IComparable, IConvertible, IComparable<T>, IEquatable<T>
        {
            //Early return for invalid names.
            if (string.IsNullOrEmpty(arrayName)
                || !userDefined.TryGetValue(arrayName, out var rawData))
            {
                Debug.LogWarning($"There is no \"{arrayName}\" entry in this detail.");
                dataArray = default;

                return false;
            }

            //Skip type check if the stored data is null.
            if (rawData == null)
            {
                dataArray = null;

                return true;
            }

            //Early return for non array data.
            if (!(rawData is IList rawList))
            {
                Debug.LogWarning($"The data \"{arrayName}\" can't be converted into an array.");

                dataArray = null;

                return false;
            }

            var genericType = typeof(T);
            var genericTypeName = genericType.FullName;
            if (!IsBuiltInTypeName(genericTypeName))
                throw new Exception($"\"{genericTypeName}\" isn't handle by \"{nameof(TryGetBuiltInDataArray)}\", use \"{nameof(TryGetCustomDataArray)}\" instead.");

            var rawDataTypeName = FindFieldTypeName(arrayName);
            if (!rawDataTypeName.EndsWith(arrayTypeNameExtension))
            {
                Debug.LogWarning($"\"{arrayName}\" isn't an array, it is a \"{rawDataTypeName}\".");

                dataArray = default;

                return false;
            }

            var elementTypeName = rawDataTypeName.Remove(rawDataTypeName.Length - arrayTypeNameExtension.Length);

            if (IsIntegerTypeName(genericTypeName)
                && IsIntegerTypeName(elementTypeName))
            {
                dataArray = new T[rawList.Count];
                for (var i = 0; i < rawList.Count; i++)
                {
                    var rawItem = rawList[i];

                    switch (rawItem)
                    {
                        case IConvertible rawConvertible
                            when TryConvertToIntegerValue(genericType, rawConvertible, out T itemData):
                            dataArray[i] = itemData;
                            break;

                        default:
                            dataArray[i] = default;
                            break;
                    }
                }

                return true;
            }

            if (IsDecimalTypeName(genericTypeName)
                && IsDecimalTypeName(elementTypeName))
            {
                dataArray = new T[rawList.Count];
                for (var i = 0; i < rawList.Count; i++)
                {
                    var rawItem = rawList[i];

                    switch (rawItem)
                    {
                        case IConvertible rawConvertible
                            when TryConvertToDecimalValue(genericType, rawConvertible, out T itemData):
                            dataArray[i] = itemData;
                            break;

                        default:
                            dataArray[i] = default;
                            break;
                    }
                }

                return true;
            }

            if (genericType == typeof(string))
            {
                dataArray = new T[rawList.Count];
                for (var i = 0; i < rawList.Count; i++)
                {
                    var rawItem = rawList[i];

                    switch (rawItem)
                    {
                        case IConvertible rawConvertible:
                            dataArray[i] = (T)(object)rawConvertible.ToString(CultureInfo.InvariantCulture);
                            break;

                        default:
                            dataArray[i] = default;
                            break;
                    }
                }

                return true;
            }

            if (genericType == typeof(DateTime)
                && (elementTypeName == typeof(string).FullName
                    || elementTypeName == typeof(DateTime).FullName))
            {
                dataArray = new T[rawList.Count];
                for (var i = 0; i < rawList.Count; i++)
                {
                    var rawItem = rawList[i];

                    switch (rawItem)
                    {
                        case IConvertible rawConvertible:
                            dataArray[i] = (T)(object)rawConvertible.ToDateTime(CultureInfo.InvariantCulture);
                            break;

                        default:
                            dataArray[i] = default;
                            break;
                    }
                }

                return true;
            }

            if (genericType == typeof(bool)
                && elementTypeName != typeof(DateTime).FullName)
            {
                dataArray = new T[rawList.Count];
                for (var i = 0; i < rawList.Count; i++)
                {
                    var rawItem = rawList[i];

                    switch (rawItem)
                    {
                        case IConvertible rawConvertible:
                            dataArray[i] = (T)(object)rawConvertible.ToBoolean(CultureInfo.InvariantCulture);
                            break;

                        default:
                            dataArray[i] = default;
                            break;
                    }
                }

                return true;
            }

            Debug.LogWarning($"Impossible to cast \"{arrayName}\" into an array of \"{genericTypeName}\".");

            dataArray = default;

            return false;
        }

        /// <summary>
        /// Try to find the array of data with the given name and deserialize it into the given list.
        /// </summary>
        /// <param name="arrayName">
        /// The name of the array to retrieve.
        /// </param>
        /// <param name="dataList">
        /// A list of data to fill with the found data.
        /// It must not be null and it will be cleared even if the array isn't found.
        /// </param>
        /// <typeparam name="T">
        /// Any built-in data type from C# except <see cref="char"/>.
        /// </typeparam>
        /// <returns>
        /// Returns true if the array was found and deserialized into the given list;
        /// returns false otherwise.
        /// </returns>
        public bool TryGetBuiltInDataArray<T>(string arrayName, List<T> dataList)
            where T : IComparable, IConvertible, IComparable<T>, IEquatable<T>
        {
            if (dataList == null)
                throw new ArgumentNullException(nameof(dataList));

            dataList.Clear();

            //Early return for invalid names.
            if (string.IsNullOrEmpty(arrayName)
                || !userDefined.TryGetValue(arrayName, out var rawData))
            {
                Debug.LogWarning($"There is no \"{arrayName}\" entry in this detail.");

                return false;
            }

            //Skip type check if the stored data is null.
            if (rawData == null)
                return true;

            //Early return for non array data.
            if (!(rawData is IList rawList))
            {
                Debug.LogWarning($"The data \"{arrayName}\" can't be converted into an array.");

                return false;
            }

            var genericType = typeof(T);
            var genericTypeName = genericType.FullName;
            if (!IsBuiltInTypeName(genericTypeName))
                throw new Exception($"\"{genericTypeName}\" isn't handle by \"{nameof(TryGetBuiltInDataArray)}\", use \"{nameof(TryGetCustomDataArray)}\" instead.");

            var rawDataTypeName = FindFieldTypeName(arrayName);
            if (!rawDataTypeName.EndsWith(arrayTypeNameExtension))
            {
                Debug.LogWarning($"\"{arrayName}\" isn't an array, it is a \"{rawDataTypeName}\".");

                return false;
            }

            var elementTypeName = rawDataTypeName.Remove(rawDataTypeName.Length - arrayTypeNameExtension.Length);

            if (IsIntegerTypeName(genericTypeName)
                && IsIntegerTypeName(elementTypeName))
            {
                foreach (var rawItem in rawList)
                {
                    switch (rawItem)
                    {
                        case IConvertible rawConvertible
                            when TryConvertToIntegerValue(genericType, rawConvertible, out T itemData):
                            dataList.Add(itemData);
                            break;

                        default:
                            dataList.Add(default);
                            break;
                    }
                }

                return true;
            }

            if (IsDecimalTypeName(genericTypeName)
                && IsDecimalTypeName(elementTypeName))
            {
                foreach (var rawItem in rawList)
                {
                    switch (rawItem)
                    {
                        case IConvertible rawConvertible
                            when TryConvertToDecimalValue(genericType, rawConvertible, out T itemData):
                            dataList.Add(itemData);
                            break;

                        default:
                            dataList.Add(default);
                            break;
                    }
                }

                return true;
            }

            if (genericType == typeof(string))
            {
                foreach (var rawItem in rawList)
                {
                    switch (rawItem)
                    {
                        case IConvertible rawConvertible:
                            var dataItem = (T)(object)rawConvertible.ToString(CultureInfo.InvariantCulture);
                            dataList.Add(dataItem);
                            break;

                        default:
                            dataList.Add(default);
                            break;
                    }
                }

                return true;
            }

            if (genericType == typeof(DateTime)
                && (elementTypeName == typeof(string).FullName
                    || elementTypeName == typeof(DateTime).FullName))
            {
                foreach (var rawItem in rawList)
                {
                    switch (rawItem)
                    {
                        case IConvertible rawConvertible:
                            var dataItem = (T)(object)rawConvertible.ToDateTime(CultureInfo.InvariantCulture);
                            dataList.Add(dataItem);
                            break;

                        default:
                            dataList.Add(default);
                            break;
                    }
                }

                return true;
            }

            if (genericType == typeof(bool)
                && elementTypeName != typeof(DateTime).FullName)
            {
                foreach (var rawItem in rawList)
                {
                    switch (rawItem)
                    {
                        case IConvertible rawConvertible:
                            var dataItem = (T)(object)rawConvertible.ToBoolean(CultureInfo.InvariantCulture);
                            dataList.Add(dataItem);
                            break;

                        default:
                            dataList.Add(default);
                            break;
                    }
                }

                return true;
            }

            Debug.LogWarning($"Impossible to cast \"{arrayName}\" into a list of \"{genericTypeName}\".");

            return false;
        }

        /// <summary>
        /// Try to find the array of data with the given name and deserialize it.
        /// </summary>
        /// <param name="arrayName">
        /// The name of the array to retrieve.
        /// </param>
        /// <param name="dataArray">
        /// The deserialized array of data if it was found.
        /// </param>
        /// <typeparam name="T">
        /// Any <see cref="IDictionaryConvertible"/> object that have a default constructor.
        /// </typeparam>
        /// <returns>
        /// Returns true if the array was found and deserialized;
        /// returns false otherwise.
        /// </returns>
        public bool TryGetCustomDataArray<T>(string arrayName, out T[] dataArray)
            where T : IDictionaryConvertible, new()
        {
            //Early return for invalid names.
            if (string.IsNullOrEmpty(arrayName)
                || !userDefined.TryGetValue(arrayName, out var rawData))
            {
                Debug.LogWarning($"There is no \"{arrayName}\" entry in this detail.");
                dataArray = default;

                return false;
            }

            //Skip type check if the stored data is null.
            if (rawData == null)
            {
                dataArray = null;

                return true;
            }

            //Early return for non array data.
            if (!(rawData is IList rawList))
            {
                Debug.LogWarning($"The data \"{arrayName}\" can't be converted into an array.");

                dataArray = null;

                return false;
            }

            var genericType = typeof(T);
            var genericTypeName = genericType.FullName;
            var fieldTypeName = FindFieldTypeName(arrayName);

            //Not stored as an array.
            if (!fieldTypeName.EndsWith(arrayTypeNameExtension))
            {
                Debug.LogWarning($"\"{arrayName}\" is stored as a \"{fieldTypeName}\" and can't be deserialized into a an array of \"{genericTypeName}\".");

                dataArray = default;

                return false;
            }

            var rawElementTypeName = fieldTypeName.Remove(fieldTypeName.Length - arrayTypeNameExtension.Length);
            if (!rawElementTypeName.EndsWith(customTypeNameExtension))
            {
                Debug.LogWarning($"\"{arrayName}\" is stored as an array of \"{rawElementTypeName}\" and can't be deserialized into an array of \"{genericTypeName}\".");

                dataArray = default;

                return false;
            }

            var elementTypeName = rawElementTypeName.Remove(rawElementTypeName.Length - customTypeNameExtension.Length);
            if (elementTypeName != genericTypeName)
            {
                Debug.LogWarning($"\"{arrayName}\" is stored as an array of \"{elementTypeName}\" and can't be deserialized into an array of \"{genericTypeName}\".");

                dataArray = default;

                return false;
            }

            dataArray = new T[rawList.Count];

            for (var i = 0; i < rawList.Count; i++)
            {
                var rawItem = rawList[i];

                T dataItem = default;
                if (rawItem is Dictionary<string, object> rawDictionary)
                {
                    dataItem = new T();
                    dataItem.FillFromDictionary(rawDictionary);
                }

                dataArray[i] = dataItem;
            }

            return true;
        }

        /// <summary>
        /// Try to find the array of data with the given name and deserialize it into the given list.
        /// </summary>
        /// <param name="arrayName">
        /// The name of the array to retrieve.
        /// </param>
        /// <param name="dataList">
        /// A list of data to fill with the found data.
        /// It must not be null and it will be cleared even if the array isn't found.
        /// </param>
        /// <typeparam name="T">
        /// Any <see cref="IDictionaryConvertible"/> object that have a default constructor.
        /// </typeparam>
        /// <returns>
        /// Returns true if the array was found and deserialized into the given list;
        /// returns false otherwise.
        /// </returns>
        public bool TryGetCustomDataArray<T>(string arrayName, List<T> dataList)
            where T : IDictionaryConvertible, new()
        {
            if (dataList == null)
                throw new ArgumentNullException(nameof(dataList));

            dataList.Clear();

            //Early return for invalid names.
            if (string.IsNullOrEmpty(arrayName)
                || !userDefined.TryGetValue(arrayName, out var rawData))
            {
                Debug.LogWarning($"There is no \"{arrayName}\" entry in this detail.");

                return false;
            }

            //Skip type check if the stored data is null.
            if (rawData == null)
                return true;

            //Early return for non array data.
            if (!(rawData is IList rawList))
            {
                Debug.LogWarning($"The data \"{arrayName}\" can't be converted into an array.");

                return false;
            }

            var genericType = typeof(T);
            var genericTypeName = genericType.FullName;
            var fieldTypeName = FindFieldTypeName(arrayName);

            //Not stored as an array.
            if (!fieldTypeName.EndsWith(arrayTypeNameExtension))
            {
                Debug.LogWarning($"\"{arrayName}\" is stored as a \"{fieldTypeName}\" and can't be deserialized into a an array of \"{genericTypeName}\".");

                return false;
            }

            var rawElementTypeName = fieldTypeName.Remove(fieldTypeName.Length - arrayTypeNameExtension.Length);
            if (!rawElementTypeName.EndsWith(customTypeNameExtension))
            {
                Debug.LogWarning($"\"{arrayName}\" is stored as an array of \"{rawElementTypeName}\" and can't be deserialized into an array of \"{genericTypeName}\".");

                return false;
            }

            var elementTypeName = rawElementTypeName.Remove(rawElementTypeName.Length - customTypeNameExtension.Length);
            if (elementTypeName != genericTypeName)
            {
                Debug.LogWarning($"\"{arrayName}\" is stored as an array of \"{elementTypeName}\" and can't be deserialized into an array of \"{genericTypeName}\".");

                return false;
            }

            foreach (var rawItem in rawList)
            {
                T dataItem = default;
                if (rawItem is Dictionary<string, object> rawDictionary)
                {
                    dataItem = new T();
                    dataItem.FillFromDictionary(rawDictionary);
                }

                dataList.Add(dataItem);
            }

            return true;
        }

        /// <summary>
        /// Try to find the dictionary of data with the given name and deserialize it into the given dictionary.
        /// </summary>
        /// <param name="dictionaryName">
        /// The name of the dictionary to retrieve.
        /// </param>
        /// <param name="dataDictionary">
        /// A dictionary of data to fill with the found data.
        /// It must not be null and it will be cleared even if no dictionary is found.
        /// </param>
        /// <returns>
        /// Returns true if the dictionary was found and deserialized into the given dictionary;
        /// returns false otherwise.
        /// </returns>
        public bool TryGetDataDictionary(string dictionaryName, Dictionary<string, object> dataDictionary)
        {
            if (dataDictionary == null)
                throw new ArgumentNullException(nameof(dataDictionary));

            dataDictionary.Clear();

            //Early return for invalid names.
            if (string.IsNullOrEmpty(dictionaryName)
                || !userDefined.TryGetValue(dictionaryName, out var rawData))
            {
                Debug.LogWarning($"There is no \"{dictionaryName}\" entry in this detail.");

                return false;
            }

            var fieldTypeName = FindFieldTypeName(dictionaryName);
            if (fieldTypeName != dictionaryTypeName)
            {
                Debug.LogWarning($"The given \"{dictionaryName}\" is a \"{fieldTypeName}\" and can't be converted to a dictionary.");

                return false;
            }

            if (rawData is Dictionary<string, object> rawDictionary)
            {
                foreach (var rawEntry in rawDictionary)
                    dataDictionary.Add(rawEntry.Key, rawEntry.Value);
            }

            return true;
        }

        string FindFieldTypeName(string fieldName)
        {
            foreach (var entry in gfTypeMap)
            {
                if (entry.Value.Contains(fieldName))
                {
                    return entry.Key;
                }
            }

            return null;
        }

        static bool TryGetConvertibleData<T>(string fieldTypeName, object rawData, Type genericType, out T data)
        {
            if (!(rawData is IConvertible convertibleData))
            {
                data = default;

                return false;
            }

            var genericTypeName = genericType.FullName;

            if (genericTypeName == typeof(bool).FullName
                && fieldTypeName != typeof(DateTime).FullName)
            {
                data = (T)(object)convertibleData.ToBoolean(CultureInfo.InvariantCulture);

                return true;
            }

            //All integers value types are stored as long so we need to convert them.
            if (IsIntegerTypeName(fieldTypeName)
                && IsIntegerTypeName(genericTypeName))
                return TryConvertToIntegerValue(genericType, convertibleData, out data);

            //All floating value types are stored as double so we need to convert them.
            if (IsDecimalTypeName(fieldTypeName)
                && IsDecimalTypeName(genericTypeName))
                return TryConvertToDecimalValue(genericType, convertibleData, out data);

            //DateTime are serialized as strings
            if (genericType == typeof(DateTime)
                && (fieldTypeName == typeof(DateTime).FullName
                    || fieldTypeName == typeof(string).FullName))
            {
                data = (T)(object)convertibleData.ToDateTime(CultureInfo.InvariantCulture);

                return true;
            }

            if (genericType == typeof(string))
            {
                data = (T)(object)convertibleData.ToString(CultureInfo.InvariantCulture);

                return true;
            }

            data = default;

            return false;
        }

        static bool TryConvertToIntegerValue<T>(Type genericType, IConvertible convertibleData, out T data)
        {
            if (genericType == typeof(byte))
            {
                data = (T)(object)convertibleData.ToByte(null);

                return true;
            }

            if (genericType == typeof(sbyte))
            {
                data = (T)(object)convertibleData.ToSByte(null);

                return true;
            }

            if (genericType == typeof(short))
            {
                data = (T)(object)convertibleData.ToInt16(null);

                return true;
            }

            if (genericType == typeof(ushort))
            {
                data = (T)(object)convertibleData.ToUInt16(null);

                return true;
            }

            if (genericType == typeof(int))
            {
                data = (T)(object)convertibleData.ToInt32(null);

                return true;
            }

            if (genericType == typeof(uint))
            {
                data = (T)(object)convertibleData.ToUInt32(null);

                return true;
            }

            if (genericType == typeof(long))
            {
                data = (T)(object)convertibleData.ToInt64(null);

                return true;
            }

            if (genericType == typeof(ulong))
            {
                data = (T)(object)convertibleData.ToUInt64(null);

                return true;
            }

            data = default;

            return false;
        }

        static bool TryConvertToDecimalValue<T>(Type genericType, IConvertible convertibleData, out T data)
        {
            if (genericType == typeof(float))
            {
                data = (T)(object)convertibleData.ToSingle(null);

                return true;
            }

            if (genericType == typeof(decimal))
            {
                data = (T)(object)convertibleData.ToDecimal(null);

                return true;
            }

            if (genericType == typeof(double))
            {
                data = (T)(object)convertibleData.ToDouble(null);

                return true;
            }

            data = default;

            return false;
        }

        static bool IsIntegerTypeName(string name)
        {
            return name == typeof(byte).FullName
                || name == typeof(sbyte).FullName
                || name == typeof(short).FullName
                || name == typeof(ushort).FullName
                || name == typeof(int).FullName
                || name == typeof(uint).FullName
                || name == typeof(long).FullName
                || name == typeof(ulong).FullName;
        }

        static bool IsDecimalTypeName(string name)
        {
            return name == typeof(float).FullName
                || name == typeof(double).FullName
                || name == typeof(decimal).FullName;
        }

        static bool IsBuiltInTypeName(string name)
        {
            return IsIntegerTypeName(name)
                || IsDecimalTypeName(name)
                || name == typeof(bool).FullName
                || name == typeof(string).FullName
                || name == typeof(DateTime).FullName;
        }

        /// <inheritdoc cref="IDictionaryConvertible.ToDictionary"/>
        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                [nameof(userDefined)] = userDefined,
                [nameof(gfTypeMap)] = gfTypeMap
            };
        }

        /// <inheritdoc cref="IDictionaryConvertible.FillFromDictionary"/>
        public JsonData FillFromDictionary(Dictionary<string, object> rawDictionary)
        {
            userDefined.Clear();
            gfTypeMap.Clear();

            if (rawDictionary.TryGetValue(nameof(userDefined), out var rawUserDefined)
                && rawUserDefined is Dictionary<string, object> castUserDefined)
                userDefined = new Dictionary<string, object>(castUserDefined);

            if (rawDictionary.TryGetValue(nameof(gfTypeMap), out var rawGfTypeMap))
            {
                switch (rawGfTypeMap)
                {
                    case Dictionary<string, List<string>> castGfTypeMap:
                    {
                        gfTypeMap = new Dictionary<string, List<string>>(castGfTypeMap);
                        break;
                    }

                    //Dictionary created from MiniJson isn't hard typed and need to be handled differently.
                    case Dictionary<string, object> halfCastGfTypeMap:
                    {
                        gfTypeMap = new Dictionary<string, List<string>>(halfCastGfTypeMap.Count);
                        if (halfCastGfTypeMap.Count == 0)
                            break;

                        foreach (var halfCastEntry in halfCastGfTypeMap)
                        {
                            //Handle arrays only
                            if (!(halfCastEntry.Value is IList<object> rawFieldNames))
                                continue;

                            var fieldNames = new List<string>(rawFieldNames.Count);
                            foreach (var rawFieldName in rawFieldNames)
                            {
                                fieldNames.Add(rawFieldName.ToString());
                            }

                            gfTypeMap[halfCastEntry.Key] = fieldNames;
                        }

                        break;
                    }
                }
            }

            return this;
        }

        /// <inheritdoc cref="FillFromDictionary"/>
        void IDictionaryConvertible.FillFromDictionary(Dictionary<string, object> rawDictionary)
            => FillFromDictionary(rawDictionary);
    }
}
