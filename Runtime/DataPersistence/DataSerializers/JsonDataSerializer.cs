using System.IO;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// DataSerializer to serialize GameFoundation's data to and from Json.
    /// </summary>
    public sealed class JsonDataSerializer : IDataSerializer
    {
        /// <inheritdoc />
        public void Serialize(GameFoundationSerializableData data, TextWriter writer)
        {
            var json = JsonUtility.ToJson(data);
            writer.Write(json);
        }

        /// <inheritdoc />
        public GameFoundationSerializableData Deserialize(TextReader reader)
        {
            return JsonUtility.FromJson<GameFoundationSerializableData>(reader.ReadToEnd());
        }
    }
}
