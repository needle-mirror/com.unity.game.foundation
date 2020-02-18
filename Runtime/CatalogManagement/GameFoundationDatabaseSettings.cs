#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// Game Foundation database settings for Game Foundation editor database serialization.
    /// </summary>
    public class GameFoundationDatabaseSettings : ScriptableObject
    {
        /// <summary>
        /// The directory name where Unity project assets will be created/stored.
        /// </summary>
        private static readonly string kAssetsFolder = "GameFoundation";

        private static GameFoundationDatabaseSettings s_Instance;
        internal static GameFoundationDatabaseSettings singleton
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<GameFoundationDatabaseSettings>("GameFoundationDatabaseSettings");

#if UNITY_EDITOR
                    if (s_Instance == null && !Application.isPlaying)
                    {
                        Debug.Log("No Game Foundation database settings file has been found. " +
                                  "Game Foundation code will automatically create one. " +
                                  "Database settings file is critical to Game Foundation, " +
                                  "if you wish to remove it you will need to " +
                                  "remove the entire Game Foundation package.");

                        s_Instance = ScriptableObject.CreateInstance<GameFoundationDatabaseSettings>();

                        if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}"))
                        {
                            AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                        }

                        if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}/Resources"))
                        {
                            AssetDatabase.CreateFolder($"Assets/{kAssetsFolder}", "Resources");
                        }

                        AssetDatabase.CreateAsset(s_Instance, $"Assets/{kAssetsFolder}/Resources/GameFoundationDatabaseSettings.asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        s_Instance = Resources.Load<GameFoundationDatabaseSettings>("GameFoundationDatabaseSettings");
                    }
#endif

                    if (s_Instance == null)
                    {
                        throw new System.InvalidOperationException("Unable to find or create a GameFoundationDatabaseSettings resource!");
                    }
                }

#if UNITY_EDITOR
                if (s_Instance.m_Database == null)
                {
                    Tools.ThrowIfPlayMode("Game Foundation database reference cannot be null while in play mode. "
                        + "Open one of the Game Foundation windows in the Unity Editor while not in Play Mode to have a database asset created for you automatically.");

                    string databaseAssetPath = $"Assets/{kAssetsFolder}/GameFoundationDatabase.asset";

                    // try to load a database asset by hardcoded path
                    s_Instance.m_Database = AssetDatabase.LoadAssetAtPath<GameFoundationDatabase>(databaseAssetPath);

                    // if that doesn't work, then create one
                    if (s_Instance.m_Database == null)
                    {
                        s_Instance.m_Database = ScriptableObject.CreateInstance<GameFoundationDatabase>();

                        if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}"))
                        {
                            AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                        }

                        AssetDatabase.CreateAsset(s_Instance.m_Database, databaseAssetPath);
                        EditorUtility.SetDirty(s_Instance);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
#else
                if (s_Instance.m_Database == null)
                {
                    throw new System.Exception("Game Foundation database reference cannot be null."
                        + "Open one of the Game Foundation windows in the Unity Editor while not in Play Mode to have a database asset created for you automatically.");
                }
#endif

                return s_Instance;
            }
        }

        [SerializeField]
        private GameFoundationDatabase m_Database;

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
