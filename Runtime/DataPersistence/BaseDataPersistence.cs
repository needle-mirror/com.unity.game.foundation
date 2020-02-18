using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Base persistence class derived from IDataPersistence
    /// </summary>
    public abstract class BaseDataPersistence : IDataPersistence
    {
        IDataSerializer m_Serializer;

        /// <summary>
        /// The serialization layer used by the processes of this persistence.
        /// </summary>
        protected IDataSerializer serializer
        {
            get { return m_Serializer; }
        }

        /// <summary>
        /// Basic constructor that takes in a data serializer which this will use.
        /// </summary>
        /// <param name="serializer">The data serializer to use.</param>
        public BaseDataPersistence(IDataSerializer serializer)
        {
            m_Serializer = serializer;
        }

        /// <inheritdoc />
        public abstract void Load(Action<GameFoundationSerializableData> onLoadCompleted = null, Action<Exception> onLoadFailed = null);

        /// <inheritdoc />
        public abstract void Save(GameFoundationSerializableData content, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null);
    }
}
