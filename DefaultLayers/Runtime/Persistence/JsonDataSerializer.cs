using System.IO;
using UnityEngine.GameFoundation.Data;

namespace UnityEngine.GameFoundation.DefaultLayers.Persistence
{
    /// <summary>
    /// DataSerializer to serialize GameFoundation's data to and from Json.
    /// </summary>
    public sealed class JsonDataSerializer : IDataSerializer
    {
        /// <inheritdoc />
        public void Serialize(GameFoundationData data, TextWriter writer)
        {
            var json = JsonUtility.ToJson(data);
            writer.Write(json);
        }

        /// <inheritdoc />
        public GameFoundationData Deserialize(TextReader reader)
        {
            return JsonUtility.FromJson<GameFoundationData>(reader.ReadToEnd());
        }
    }
}
