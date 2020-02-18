#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Game Foundation settings for runtime implementation and serialization.
    /// </summary>
    public class GameFoundationSettings : ScriptableObject
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
                if (s_Instance == null)
                {
                    CreateGameFoundationSettingsIfNecessary();
                }

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
        {
            get { return singleton.m_EnablePlayModeAnalytics; }
            set {
                singleton.m_EnablePlayModeAnalytics = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(s_Instance);
#endif
            }
        }

        [SerializeField]
        private bool m_EnableEditorModeAnalytics = false;

        /// <summary>
        /// Indicates whether analytic events should be fired while in Editor Mode.
        /// </summary>
        /// <returns>True if analytic events should be fired while in Editor Mode.</returns>
        public static bool enableEditorModeAnalytics
        {
            get { return singleton.m_EnableEditorModeAnalytics; }
            set {
                singleton.m_EnableEditorModeAnalytics = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(s_Instance);
#endif
            }
        }

        internal static void CreateGameFoundationSettingsIfNecessary()
        {
            if (s_Instance == null)
            {
                s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");

#if UNITY_EDITOR
                if (s_Instance == null && !Application.isPlaying)
                {
                    Debug.Log("No Game Foundation settings file has been found. " +
                              "Game Foundation code will automatically create one. " +
                              "The Settings file is critical to Game Foundation, " +
                              "if you wish to remove it you will need to " +
                              "remove the entire Game Foundation package.");

                    s_Instance = ScriptableObject.CreateInstance<GameFoundationSettings>();

                    if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}"))
                    {
                        AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                    }

                    if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}/Resources"))
                    {
                        AssetDatabase.CreateFolder($"Assets/{kAssetsFolder}", "Resources");
                    }

                    AssetDatabase.CreateAsset(s_Instance, $"Assets/{kAssetsFolder}/Resources/GameFoundationSettings.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");
                }
#endif

                if (s_Instance == null)
                {
                    throw new System.InvalidOperationException("Unable to find or create a GameFoundationSettings resource!");
                }
            }
        }
    }
}
