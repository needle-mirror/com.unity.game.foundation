using UnityEngine.GameFoundation.DataPersistence;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Contract for objects providing data to the <see cref="StatManager"/>.
    /// </summary>
    public interface IStatDataLayer
    {
        /// <summary>
        /// Get StatManager's serializable data.
        /// </summary>
        StatManagerSerializableData GetData();

        /// <summary>
        /// Request to create or update the stat item defined by the given ids.
        /// </summary>
        /// <param name="gameItemId">Game item's id the stat item is connected to.</param>
        /// <param name="statDefinitionId">Definition's Id the stat item is based on.</param>
        /// <param name="value">Value to set to the stat item.</param>
        /// <param name="defaultValue">Value used to reset the stat item.</param>
        /// <param name="completer">The handle to settle the promise with.</param>
        /// <typeparam name="T">
        /// Type of the stat.
        /// Supported types currently are:
        /// - <see cref="int"/>
        /// - <see cref="float"/>
        /// </typeparam>
        void SetStatValue<T>(int gameItemId, string statDefinitionId, T value, T defaultValue, Completer completer);

        /// <summary>
        /// Request to delete the stat item defined by the given ids.
        /// </summary>
        /// <param name="gameItemId">Game item's id the stat item is connected to.</param>
        /// <param name="statDefinitionId">Definition's Id the stat item is based on.</param>
        /// <param name="completer">The handle to settle the promise with.</param>
        /// <typeparam name="T">
        /// Type of the stat.
        /// Supported types currently are:
        /// - <see cref="int"/>
        /// - <see cref="float"/>
        /// </typeparam>
        void DeleteStatValue<T>(int gameItemId, string statDefinitionId, Completer completer);
    }
}
