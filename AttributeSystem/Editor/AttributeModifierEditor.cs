using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace LitePlayQuickFramework.AttributeSystem.EditorTools {
    public static class AttributeModifierEditor {
        public static void DrawModifier(AttributeModifier modifier, System.Action onRemove = null) {
            if (modifier == null) return;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            float labelWidth = 50f;
            float fieldWidth = 80f;

            // Type
            GUILayout.Label("Type:", GUILayout.Width(labelWidth));
            GUILayout.Label(modifier.Type.ToString(), GUILayout.Width(fieldWidth));

            // Source
            GUILayout.Label("Source:", GUILayout.Width(labelWidth));
            GUILayout.Label(modifier.Source, GUILayout.Width(fieldWidth + 40f));

            // Value
            GUILayout.Label("Value:", GUILayout.Width(labelWidth));
            GUILayout.Label(modifier.Value.ToString(CultureInfo.CurrentCulture), GUILayout.Width(fieldWidth));

            // 删除按钮在末尾
            if (onRemove != null) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", GUILayout.Width(60))) {
                    onRemove.Invoke();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}