// IconDetailDefinition is deprecated but we don't want a warning about it and we can't remove it from this script
#pragma warning disable 618

using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    [CustomEditor(typeof(IconDetailDefinition))]
    internal class IconDetailDefinitionEditor : BaseDetailDefinitionEditor
    {
    }
}
