using System;

namespace UnityEngine.GameFoundation
{
    public abstract class BaseDataPersistence : IDataPersistence
    {
        public static readonly int k_SaveVersion = 1;

        protected IDataSerializer m_Serializer;

        public BaseDataPersistence(IDataSerializer serializer)
        {
            m_Serializer = serializer;
        }

        /// <inheritdoc />
        public abstract void Load<T>(string identifier, Action<ISerializableData> onLoadCompleted = null, Action<Exception> onLoadFailed = null) where T : ISerializableData;
        
        /// <inheritdoc />
        public abstract void Save(string identifier, ISerializableData content, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null);
    }
}
