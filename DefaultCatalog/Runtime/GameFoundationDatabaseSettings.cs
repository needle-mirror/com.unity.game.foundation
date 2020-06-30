using System;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    /// <summary>
    /// Game Foundation database settings for Game Foundation editor database serialization.
    /// </summary>
    public partial class GameFoundationDatabaseSettings : ScriptableObject
    {
        static GameFoundationDatabaseSettings s_Instance;

        internal static GameFoundationDatabaseSettings singleton
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<GameFoundationDatabaseSettings>("GameFoundationDatabaseSettings");

#if UNITY_EDITOR
                    Editor_TryCreateDatabaseSettings();
#endif
                    if (s_Instance == null)
                    {
                        throw new InvalidOperationException("Unable to find or create a GameFoundationDatabaseSettings resource!");
                    }
                }

#if UNITY_EDITOR
                Editor_TryCreateDatabase();
#endif

                if (s_Instance.m_Database == null)
                {
                    throw new Exception("Game Foundation database reference cannot be null."
                        + "Open one of the Game Foundation windows in the Unity Editor while not in Play Mode to have a database asset created for you automatically.");
                }

                return s_Instance;
            }
        }

        /// <inheritdoc cref="database"/>
        [SerializeField]
        GameFoundationDatabase m_Database;

        /// <summary>
        /// The GameFoundationDatabase in use.
        /// </summary>
        public static GameFoundationDatabase database
        {
            get => singleton.m_Database;
            set => singleton.m_Database = value;
        }
    }
}
