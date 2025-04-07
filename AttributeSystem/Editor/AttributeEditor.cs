using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace LitePlayQuickFramework.AttributeSystem.EditorTools {
    [CustomPropertyDrawer(typeof(AttributePrefabConfig))]
    public class AttributePrefabConfigDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 2 + 6; // padding
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var nameProp = property.FindPropertyRelative("name");
            var baseValueProp = property.FindPropertyRelative("baseValue");
            var hasMinProp = property.FindPropertyRelative("hasMinClamp");
            var minValProp = property.FindPropertyRelative("minValue");
            var hasMaxProp = property.FindPropertyRelative("hasMaxClamp");
            var maxValProp = property.FindPropertyRelative("maxValue");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = 2f;
            float labelWidth = 90f;
            float fieldWidth = (position.width / 2f) - 4;

            Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

            // --- Line 1: Name + BaseValue ---
            Rect nameLabel = new Rect(rect.x, rect.y, labelWidth, lineHeight);
            Rect nameField = new Rect(nameLabel.xMax, rect.y, fieldWidth - labelWidth, lineHeight);
            EditorGUI.LabelField(nameLabel, "AttributeName:");
            EditorGUI.PropertyField(nameField, nameProp, GUIContent.none);

            Rect baseLabel = new Rect(nameField.xMax + 8, rect.y, labelWidth, lineHeight);
            Rect baseField = new Rect(baseLabel.xMax, rect.y, fieldWidth - labelWidth, lineHeight);
            EditorGUI.LabelField(baseLabel, "Base Value:");
            EditorGUI.PropertyField(baseField, baseValueProp, GUIContent.none);

            // --- Line 2: MinClamp + MaxClamp ---
            rect.y += lineHeight + spacing;

            // Min Clamp
            Rect minToggle = new Rect(rect.x, rect.y, 16, lineHeight);
            Rect minLabel = new Rect(minToggle.xMax + 2, rect.y, 26, lineHeight);
            Rect minField = new Rect(minLabel.xMax + 2, rect.y, fieldWidth - 16 - 26 - 4, lineHeight);
            EditorGUI.PropertyField(minToggle, hasMinProp, GUIContent.none);
            EditorGUI.LabelField(minLabel, "Min:");
            EditorGUI.BeginDisabledGroup(!hasMinProp.boolValue);
            EditorGUI.PropertyField(minField, minValProp, GUIContent.none);
            EditorGUI.EndDisabledGroup();

            // Max Clamp
            Rect maxToggle = new Rect(minField.xMax + 8, rect.y, 16, lineHeight);
            Rect maxLabel = new Rect(maxToggle.xMax + 2, rect.y, 26, lineHeight);
            Rect maxField = new Rect(maxLabel.xMax + 2, rect.y, fieldWidth - 16 - 26 - 4, lineHeight);
            EditorGUI.PropertyField(maxToggle, hasMaxProp, GUIContent.none);
            EditorGUI.LabelField(maxLabel, "Max:");
            EditorGUI.BeginDisabledGroup(!hasMaxProp.boolValue);
            EditorGUI.PropertyField(maxField, maxValProp, GUIContent.none);
            EditorGUI.EndDisabledGroup();
            
            rect.y += lineHeight + spacing;
            
            Rect lineRect = new Rect(position.x, rect.y, position.width, 1);
            EditorGUI.DrawRect(lineRect, new Color(0.4f, 0.4f, 0.4f)); // 深灰分割线
            
            EditorGUI.EndProperty();
        }
    }

    public static class AttributeEditor {
        private static bool _showModifiers = true;
        
        public static void DrawAttribute(Attribute attribute) {
            if (attribute == null) return;

            EditorGUILayout.BeginVertical("box");
            _showModifiers = EditorGUILayout.Foldout(_showModifiers, $"AttributeModifierList ({attribute.Modifiers.Count})");
            if (_showModifiers) {
                DrawAttributeModifierList(attribute);
            }

            EditorGUILayout.EndVertical();
        }

        private static float _columnWidth;
        private static GUILayoutOption WidthOption => GUILayout.Width(_columnWidth);
        
        private static ModifierTypes _newType = ModifierTypes.Add;
        private static string _newSource = "";
        private static float _newValue;
        
        public static void DrawAttributeModifierList(Attribute attribute) {
            List<AttributeModifier> modifiers = attribute.Modifiers;
            if (modifiers == null) return;
            
            _columnWidth = (EditorGUIUtility.currentViewWidth - 60) / 4f; // 减去边距

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Type", WidthOption);
            GUILayout.Label("Source", WidthOption);
            GUILayout.Label("Value", WidthOption);
            GUILayout.Label("Remove", WidthOption);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            foreach (var mod in modifiers) {
                DrawModifier(mod, () => { attribute.RemoveModifier(mod); });
            }

            GUILayout.Space(8);
            EditorGUILayout.LabelField("AddNewModifier:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _newType = (ModifierTypes)EditorGUILayout.EnumPopup(GUIContent.none, _newType, WidthOption);
            _newSource = EditorGUILayout.TextField(GUIContent.none, _newSource, WidthOption);
            _newValue = EditorGUILayout.FloatField(GUIContent.none, _newValue, WidthOption);
            GUI.enabled = !string.IsNullOrEmpty(_newSource);
            if (GUILayout.Button("Add", WidthOption)) {
                attribute.AddModifier(new AttributeModifier(_newType, _newSource, _newValue));
                _newSource = "";
                _newValue = 0;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        
        public static void DrawModifier(AttributeModifier modifier, System.Action onRemove = null) {
            if (modifier == null) return;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(modifier.Type.ToString(), WidthOption);
            GUILayout.Label(modifier.Source, WidthOption);
            GUILayout.Label(modifier.Value.ToString(CultureInfo.CurrentCulture), WidthOption);

            // 删除按钮在末尾
            if (onRemove != null) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", WidthOption)) {
                    onRemove.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}
