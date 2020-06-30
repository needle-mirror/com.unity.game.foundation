using System;
using System.Collections.Generic;
using UnityEngine.GameFoundation.MiniJson;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Use this detail to store JSON like data.
    /// </summary>
    [Obsolete("Prefer using Static Properties when possible.")]
    public sealed class JsonDetail : BaseDetail
    {
        JsonData m_Data;

        /// <summary>
        /// Build a JsonDetailDefinition.
        /// </summary>
        /// <param name="json">
        /// The json data representing the detail.
        /// </param>
        internal JsonDetail(string json)
        {
            //Null json data is considered as empty data.
            if (string.IsNullOrEmpty(json))
            {
                m_Data = new JsonData();

                return;
            }

            //Assert json is valid.
            var rawDeserializedValue = Json.Deserialize(json);
            if (!(rawDeserializedValue is Dictionary<string, object> rawDictionary))
                throw new ArgumentException("The given json isn't properly formatted.", nameof(json));

            m_Data = new JsonData()
                .FillFromDictionary(rawDictionary);
        }

        /// <inheritdoc cref="JsonData.TryGetBuiltInData{T}"/>
        public bool TryGetBuiltInData<T>(string fieldName, out T data)
            where T : IComparable, IConvertible, IComparable<T>, IEquatable<T>
        {
            return m_Data.TryGetBuiltInData(fieldName, out data);
        }

        /// <inheritdoc cref="JsonData.TryGetBuiltInDataArray{T}(string,out T[])"/>
        public bool TryGetBuiltInDataArray<T>(string arrayName, out T[] dataArray)
            where T : IComparable, IConvertible, IComparable<T>, IEquatable<T>
        {
            return m_Data.TryGetBuiltInDataArray(arrayName, out dataArray);
        }

        /// <inheritdoc cref="JsonData.TryGetBuiltInDataArray{T}(string,List{T})"/>
        public bool TryGetBuiltInDataArray<T>(string arrayName, List<T> dataList)
            where T : IComparable, IConvertible, IComparable<T>, IEquatable<T>
        {
            return m_Data.TryGetBuiltInDataArray(arrayName, dataList);
        }

        /// <inheritdoc cref="JsonData.TryGetCustomData{T}"/>
        public bool TryGetCustomData<T>(string fieldName, out T data)
            where T : IDictionaryConvertible, new()
        {
            return m_Data.TryGetCustomData(fieldName, out data);
        }

        /// <inheritdoc cref="JsonData.TryGetCustomDataArray{T}(string,out T[])"/>
        public bool TryGetCustomDataArray<T>(string arrayName, out T[] dataArray)
            where T : IDictionaryConvertible, new()
        {
            return m_Data.TryGetCustomDataArray(arrayName, out dataArray);
        }

        /// <inheritdoc cref="JsonData.TryGetCustomDataArray{T}(string,List{T})"/>
        public bool TryGetCustomDataArray<T>(string arrayName, List<T> dataList)
            where T : IDictionaryConvertible, new()
        {
            return m_Data.TryGetCustomDataArray(arrayName, dataList);
        }

        /// <inheritdoc cref="JsonData.TryGetDataDictionary"/>
        public bool TryGetDataDictionary(string dictionaryName, Dictionary<string, object> dataDictionary)
        {
            return m_Data.TryGetDataDictionary(dictionaryName, dataDictionary);
        }
    }
}
