using System;
using UnityEngine;
using UnityEngine.GameFoundation.CatalogManagement;

namespace UnityEditor.GameFoundation
{
    [CustomPropertyDrawer(typeof(StatUnion))]
    class StatUnionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var unionTypeProperty = property.FindPropertyRelative(nameof(StatUnion.type));
            var valueProperty = property.FindPropertyRelative(nameof(StatUnion.intValue));

            switch (unionTypeProperty.intValue)
            {
                case (int)StatDefinition.StatValueType.Float:
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

                case (int)StatDefinition.StatValueType.Int:
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
