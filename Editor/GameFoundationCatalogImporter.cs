using UnityEditor;

namespace UnityEngine.GameFoundation.CatalogManagement
{
    /// <summary>
    /// This class uses the AssetPostProcessor to verify the GameFoundation's default catalogs.
    /// This is a catch all solution that will verify the contents anytime this file is created or modified.
    /// </summary>
    internal class GameFoundationCatalogImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            VerifyPaths(importedAssets);
            VerifyPaths(movedAssets);
            VerifyPaths(movedFromAssetPaths);
        }

        private static void VerifyPaths(string[] assetPaths)
        {
            foreach (var importedAsset in assetPaths)
            {
                var catalog = AssetDatabase.LoadAssetAtPath<GameFoundationDatabase>(importedAsset);
                
                if (catalog != null)
                {
                    catalog.VerifyDefaultCatalogs();
                }
            }
        }
    }
}
