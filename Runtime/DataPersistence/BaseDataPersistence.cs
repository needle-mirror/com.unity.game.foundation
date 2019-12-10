using System;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Base persistence class derived from IDataPersistence
    /// </summary>
    public abstract class BaseDataPersistence : IDataPersistence
    {
        /// <summary>
        /// Current save version of DataPersistence in use. Value will be incremented each time data persistence is changed to ensure the version number matches the DataSerializer to allow proper serialization.
        /// </summary>
        public static readonly int k_SaveVersion = 1;

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
        public abstract void Load<TSerializableData>(string identifier, Action<TSerializableData> onLoadCompleted = null, Action<Exception> onLoadFailed = null)
            where TSerializableData : ISerializableData;

        /// <inheritdoc />
        public abstract void Save(string identifier, ISerializableData content, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null);
    }
}
