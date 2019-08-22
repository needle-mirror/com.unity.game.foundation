#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Game Foundation settings for all of Game Foundation implemenation and serialization.
    /// </summary>
    public class GameFoundationSettings : ScriptableObject
    {
        /// <summary>
        /// The directory name where Unity project assets will be created/stored.
        /// </summary>
        public static readonly string kAssetsFolder = "GameFoundation";

        /// <summary>
        /// The directory name where runtime data files are stored (in PersistentDataPath).
        /// </summary>
        public static readonly string kDataFolder = "GameFoundation";

        private static GameFoundationSettings s_Instance;
        private static GameFoundationSettings singleton
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");

#if UNITY_EDITOR
                    if (s_Instance == null && !Application.isPlaying)
                    {
                        s_Instance = CreateInstance<GameFoundationSettings>();

                        if (!AssetDatabase.IsValidFolder(string.Format("Assets/{0}", kAssetsFolder)))
                        {
                            AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                        }

                        if (!AssetDatabase.IsValidFolder(string.Format("Assets/{0}/Resources", kAssetsFolder)))
                        {
                            AssetDatabase.CreateFolder(string.Format("Assets/{0}", kAssetsFolder), "Resources");
                        }

                        AssetDatabase.CreateAsset(s_Instance, string.Format("Assets/{0}/Resources/GameFoundationSettings.asset", kAssetsFolder));
                        
                        AssetDatabase.Refresh();

                        s_Instance = Resources.Load<GameFoundationSettings>("GameFoundationSettings");
                    }
#endif

                    if (s_Instance == null)
                    {
                        throw new System.InvalidOperationException("Unable to find or create a GameFoundationSettings resource!");
                    }
                }
                return s_Instance;
            }
        }
        
        [SerializeField]
        private InventoryCatalog m_InventoryCatalog;

        /// <summary>
        /// A reference to an InventoryCatalog database asset file.
        /// </summary>
        /// <returns>A reference to an InventoryCatalog database asset file.</returns>
        public static InventoryCatalog inventoryCatalog
        {
            get { return singleton.m_InventoryCatalog; }
            set {
                singleton.m_InventoryCatalog = value;
#if UNITY_EDITOR
                SetInstanceDirty();
#endif
            }
        }

        [SerializeField]
        private StatCatalog m_StatCatalog = null;

        /// <summary>
        /// A reference to a stat catalog
        /// </summary>
        public static StatCatalog statCatalog
        {
            get { return singleton.m_StatCatalog; }
            set {
                singleton.m_StatCatalog = value;
#if UNITY_EDITOR
                SetInstanceDirty();
#endif
            }
        }

        [SerializeField]
        private GameItemCatalog m_GameItemCatalog;

        /// <summary>
        /// A reference to a GameItemCatalog database asset file.
        /// </summary>
        /// <returns>A reference to a GameItemCatalog database asset file.</returns>
        public static GameItemCatalog gameItemCatalog
        {
            get { return singleton.m_GameItemCatalog; }
            set {
                singleton.m_GameItemCatalog = value;
#if UNITY_EDITOR
                SetInstanceDirty();
#endif
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
                SetInstanceDirty();
#endif
            }
        }

        [SerializeField]
        private bool m_EnableEditorModeAnalytics = true;

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
                SetInstanceDirty();
#endif
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Set GameFoundationSettings asset file.
        /// </summary>
        [MenuItem("Window/Game Foundation/Settings", false, 2000)]
        public static void SelectGameFoundationSettingsAssetFile()
        {
            Selection.SetActiveObjectWithContext(singleton, null);
        }
#endif
        
#if UNITY_EDITOR
        /// <summary>
        /// Sets the GameFoundationSettings dirty so they will be serialized.
        /// </summary>
        static void SetInstanceDirty()
        {
            EditorUtility.SetDirty(s_Instance);
        }
#endif
    }
}
