using UnityEngine;

namespace UnityEditor.GameFoundation
{
    internal static class CategoryPickerStyles
    {
        public const int categoryAddButtonWidth = 75;
        public const int categoryItemMargin = 6;
        public const int categoryRemoveButtonSpaceWidth = 15;

        public static GUIStyle searchSuggestAreaStyle { get; } = new GUIStyle(GUI.skin.textArea);

        public static GUIStyle categoryListItemStyle { get; } =
            new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(8, 7, 6, 6),
                wordWrap = false,
                clipping = TextClipping.Overflow,
                alignment = TextAnchor.MiddleLeft
            };

        public static GUIStyle categorySuggestItemStyle { get; } =
            new GUIStyle
            {
                padding = new RectOffset(5, 4, 3, 3),
                normal =
                {
                    textColor = GUI.skin.label.normal.textColor
                }
            };

        public static GUIStyle categorySuggestItemStyleSelected { get; } =
            new GUIStyle(categorySuggestItemStyle)
            {
                normal =
                {
                    background = EditorGUIUtility.IconContent("selected").image as Texture2D,
                    textColor = Color.white
                }
            };
    }
}
