using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contract for objects providing data to the <see cref="GameItemLookup"/>.
    /// </summary>
    public interface IGameItemLookupDataLayer
    {
        /// <summary>
        /// Get GameItemLookup's serializable data.
        /// </summary>
        GameItemLookupSerializableData GetData();

        /// <summary>
        /// Request to set the last game item id used.
        /// </summary>
        void SetLastGameItemIdUsed(int value, Completer completer);
    }
}
