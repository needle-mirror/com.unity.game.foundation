namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This is one record in the Stat Manager’s list of current stats at runtime.
    /// </summary>
    public class StatItem<T> where T : new()
    {
        /// <summary>
        /// Constructs a new stat item with desired values, using default value.
        /// </summary>
        /// <param name="gameItemHashId">GameItem's id to apply stat to.</param>
        /// <param name="definitionId">StatDefinition's id for stat to set.</param>
        internal StatItem(int gameItemHashId, string definitionId)
        {
            m_GameItemHashId = gameItemHashId;
            m_DefinitionId = definitionId;
            m_Value = new T();
            m_DefaultValue = new T();
        }

        /// <summary>
        /// Constructs a new stat item with desired values, including initial stat value.
        /// </summary>
        /// <param name="gameItemHashId">GameItem's id to apply stat to.</param>
        /// <param name="definitionId">StatDefinition's id for stat to set.</param>
        /// <param name="value">Current and default value for this stat.</param>
        internal StatItem(int gameItemHashId, string definitionId, T value)
        {
            m_GameItemHashId = gameItemHashId;
            m_DefinitionId = definitionId;
            m_Value = value;
            m_DefaultValue = value;
        }

        /// <summary>
        /// Hash id of gameItem and is the same id as the first key of Dictionary in the StatManager.
        /// </summary>
        /// <returns>Hash id of gameItem and is the same id as the first key of Dictionary in the StatManager.</returns>
        public int gameItemHashId
        {
            get { return m_GameItemHashId; }
        }
        private int m_GameItemHashId;

        /// <summary>
        /// Stat definition id string.
        /// </summary>
        /// <returns>Stat definition id string.</returns>
        public string definitionId
        {
            get { return m_DefinitionId; }
        }
        private string m_DefinitionId;

        /// <summary>
        /// Current stat value.
        /// </summary>
        /// <returns>Current stat value.</returns>
        public T value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }
        private T m_Value;

        /// <summary>
        /// Default (initial) value for this stat to allow resetting as needed.
        /// </summary>
        /// <returns>Default (initial) value for this stat to allow resetting as needed.</returns>
        public T defaultValue
        {
            get { return m_DefaultValue; }
        }
        private T m_DefaultValue;
    }
}
