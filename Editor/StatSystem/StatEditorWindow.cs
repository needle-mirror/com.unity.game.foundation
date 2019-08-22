using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    /// <summary>
    /// Class creates Stat system-specific editor window.
    /// </summary>
    internal class StatEditorWindow : CollectionEditorWindowBase
    {
        private static StatEditorWindow s_Window;

        private static List<ICollectionEditor> m_StatEditors = new List<ICollectionEditor>();
        protected override List<ICollectionEditor> m_Editors
        {
            get
            {
                return m_StatEditors;
            }
        }

        /// <summary>
        /// Basic constructor, sets the collection type name to stat.
        /// </summary>
        public StatEditorWindow()
        {
            m_CollectionTypeName = "Stat";
        }
    
        /// <summary>
        /// Opens the Stat window (and creates one if one doesn't exist already).
        /// </summary>
        public static void ShowWindow()
        {
            s_Window = GetWindow<StatEditorWindow>(false, "Stat", true);
        }

        /// <summary>
        /// Adds the editors for the stat system as tabs in the window.
        /// </summary>
        public override void CreateEditors()
        {
            m_StatEditors.Clear();

           m_StatEditors.Add(new StatDefinitionEditor("Stats", this));
        }

        protected bool IsCatalogSet()
        {
            return GameFoundationSettings.statCatalog != null;
        }

        protected override void SetUpNecessaryCatalogs()
        {
            base.SetUpNecessaryCatalogs();

            if (!IsCatalogSet())
            {
                CreateStatCatalog();
            }
        }

        protected void CreateStatCatalog()
        {
            string createDatabaseFileName = "StatCatalog";
            string statCatalogAssetPath = string.Format("Assets/{0}/Resources/{1}.asset", GameFoundationSettings.kAssetsFolder, createDatabaseFileName);

            SetUpResourcesFolder();

            if (File.Exists(Path.Combine(Application.dataPath, statCatalogAssetPath.Substring(7))))
            {
                statCatalogAssetPath = CollectionEditorTools.CreateUniqueCatalogPath(statCatalogAssetPath);
            }

            StatCatalog newStatCatalog = StatCatalog.Create();
            AssetDatabase.CreateAsset(newStatCatalog, statCatalogAssetPath);
            CollectionEditorTools.AssetDatabaseUpdate();

            GameFoundationSettings.statCatalog = newStatCatalog;
            Debug.LogWarningFormat("Creating new asset file at path {0} and connecting it to StatCatalog in GameFoundationSettings asset.", statCatalogAssetPath);
            ResetEditors();
        }
    }
}
