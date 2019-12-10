using UnityEngine;

namespace UnityEditor.GameFoundation
{
#if UNITY_2019
    internal class GameFoundationTemplates : MonoBehaviour
    {
        /// <summary>
        /// This will create a custom detail definition based on the template and the name the user enters.
        /// </summary>
        [MenuItem("Assets/Create/Game Foundation/Custom Detail Definition")]
        static void MenuCreateCustomDetailDefinition()
        {
            string templatePath = @"Packages/com.unity.game.foundation/Editor/ScriptTemplates/CustomDetailDefinition.txt";
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                templatePath = templatePath.Replace('/', '\\');
            }
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "NewCustomDetailDefinition.cs");
        }
    }
#endif
}
