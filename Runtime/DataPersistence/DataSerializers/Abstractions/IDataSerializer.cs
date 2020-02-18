using System.IO;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// Contract for objects serializing GameFoundation's data.
    /// </summary>
    public interface IDataSerializer
    {
        /// <summary>
        /// Serialize the given data with the given writer.
        /// </summary>
        /// <param name="data">Serializable data of GameFoundation.</param>
        /// <param name="writer">The writer through which the serialization is done.</param>
        void Serialize(GameFoundationSerializableData data, TextWriter writer);

        /// <summary>
        /// Deserialize GameFoundation's data from the given reader.
        /// </summary>
        /// <param name="reader">The reader to deserialize from.</param>
        /// <returns>The deserialized GameFoundation's data.</returns>
        GameFoundationSerializableData Deserialize(TextReader reader);
    }
}
