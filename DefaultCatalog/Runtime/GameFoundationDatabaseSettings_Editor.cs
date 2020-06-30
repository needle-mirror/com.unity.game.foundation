#if UNITY_EDITOR

using UnityEditor;

namespace UnityEngine.GameFoundation.DefaultCatalog
{
    public partial class GameFoundationDatabaseSettings
    {
        /// <summary>
        /// The directory name where Unity project assets will be created/stored.
        /// </summary>
        static readonly string kAssetsFolder = "GameFoundation";

        /// <summary>
        /// Creates the database settings asset file of necessary.
        /// </summary>
        static void Editor_TryCreateDatabaseSettings()
        {
            if (s_Instance == null && !Application.isPlaying)
            {
                Debug.Log("No Game Foundation database settings file has been found. " +
                    "Game Foundation code will automatically create one. " +
                    "Database settings file is critical to Game Foundation, " +
                    "if you wish to remove it you will need to " +
                    "remove the entire Game Foundation package.");

                s_Instance = CreateInstance<GameFoundationDatabaseSettings>();

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
        }

        /// <summary>
        /// Creates the database if necessary.
        /// </summary>
        static void Editor_TryCreateDatabase()
        {
            if (s_Instance.m_Database == null)
            {
                string databaseAssetPath = $"Assets/{kAssetsFolder}/GameFoundationDatabase.asset";

                // try to load a database asset by hardcoded path
                s_Instance.m_Database = AssetDatabase.LoadAssetAtPath<GameFoundationDatabase>(databaseAssetPath);

                // if that doesn't work, then create one
                if (s_Instance.m_Database == null)
                {
                    s_Instance.m_Database = CreateInstance<GameFoundationDatabase>();

                    if (!AssetDatabase.IsValidFolder($"Assets/{kAssetsFolder}"))
                    {
                        AssetDatabase.CreateFolder("Assets", kAssetsFolder);
                    }

                    s_Instance.m_Database.Editor_Save(databaseAssetPath);
                    EditorUtility.SetDirty(s_Instance);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}

#endif
