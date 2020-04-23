using System;
using UnityEngine;
using UnityEngine.GameFoundation;

namespace UnityEditor.GameFoundation
{
    [CustomPropertyDrawer(typeof(StatValue))]
    class StatValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var unionTypeProperty = property.FindPropertyRelative(nameof(StatValue.m_Type));
            var valueProperty = property.FindPropertyRelative(nameof(StatValue.m_IntValue));

            switch (unionTypeProperty.intValue)
            {
                case (int)StatValueType.Float:
                {
                    var floatValue = BitConverter.ToSingle(
                        BitConverter.GetBytes(valueProperty.intValue),
                        0);

                    floatValue = EditorGUI.FloatField(position, label, floatValue);

                    valueProperty.intValue = BitConverter.ToInt32(
                        BitConverter.GetBytes(floatValue),
                        0);

                    break;
                }

                case (int)StatValueType.Int:
                {
                    valueProperty.intValue = EditorGUI.IntField(position, label, valueProperty.intValue);

                    break;
                }

                default:
                    throw new Exception("Invalid StatValueType.");
            }
        }
    }
}
