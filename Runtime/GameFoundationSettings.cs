namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Game Foundation settings for runtime implementation and serialization.
    /// </summary>
    public partial class GameFoundationSettings : ScriptableObject
    {
        /// <summary>
        /// The directory name where Unity project assets will be created/stored.
        /// </summary>
        private static readonly string kAssetsFolder = "GameFoundation";

        private static GameFoundationSettings s_Instance;
        internal static GameFoundationSettings singleton
        {
            get
            {
#if UNITY_EDITOR
                Editor_CreateGameFoundationSettingsIfNecessary();
#else
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");
                }
                if (s_Instance == null)
                {
                    throw new System.InvalidOperationException("Unable to find a GameFoundationSettings resource!");
                }
#endif

                return s_Instance;
            }
        }

        [SerializeField]
        private bool m_EnablePlayModeAnalytics = true;

        /// <summary>
        /// Indicates whether analytics events should be fired while in Play Mode.
        /// </summary>
        /// <returns>True if analytics events should be fired while in Play Mode.</returns>
        public static bool enablePlayModeAnalytics
            => singleton.m_EnablePlayModeAnalytics;

        [SerializeField]
        private bool m_EnableEditorModeAnalytics = false;

        /// <summary>
        /// Indicates whether analytic events should be fired while in Editor Mode.
        /// </summary>
        /// <returns>True if analytic events should be fired while in Editor Mode.</returns>
        public static bool enableEditorModeAnalytics
            => singleton.m_EnableEditorModeAnalytics;

        /// <summary>
        /// Indicates whether Game Foundation With IAP is enabled.
        /// </summary>
        /// <returns>True if UNITY_PURCHASING_FOR_GAME_FOUNDATION has been enabled for the project.</returns>
        public static bool purchasingEnabled
        {
            get
            {
#if UNITY_PURCHASING && UNITY_PURCHASING_FOR_GAME_FOUNDATION
                return true;
#else
                return false;
#endif
            }
        }
    }
}
