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

        // 临时添加用的表单数据
        private static ModifierTypes _newModifierType = ModifierTypes.Add;
        private static float _newModifierValue;
        private static string _newModifierSource = "";

        public static void DrawAttribute(Attribute attribute) {
            if (attribute == null) return;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"属性名: {attribute.Name}", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("基础值", attribute.BaseValue.ToString(CultureInfo.CurrentCulture));
            EditorGUILayout.LabelField("最终值", attribute.FinalValue.ToString(CultureInfo.CurrentCulture));
            if (attribute.HasClamp) {
                EditorGUILayout.LabelField("范围",
                    $"[{attribute.MinValue?.FinalValue} - {attribute.MaxValue?.FinalValue}]");
            }

            GUILayout.Space(5);
            _showModifiers = EditorGUILayout.Foldout(_showModifiers, $"修改器列表 ({attribute.Modifiers.Count})");
            if (_showModifiers) {
                for (int i = 0; i < attribute.Modifiers.Count; i++) {
                    var modifier = attribute.Modifiers[i];
                    AttributeModifierEditor.DrawModifier(modifier, () => { attribute.RemoveModifier(modifier); });
                }

                GUILayout.Space(8);
                EditorGUILayout.LabelField("➕ 添加修改器", EditorStyles.boldLabel);

                _newModifierType = (ModifierTypes)EditorGUILayout.EnumPopup("类型", _newModifierType);
                _newModifierValue = EditorGUILayout.FloatField("数值", _newModifierValue);
                _newModifierSource = EditorGUILayout.TextField("来源", _newModifierSource);

                if (GUILayout.Button("添加修改器")) {
                    if (!string.IsNullOrEmpty(_newModifierSource)) {
                        var newMod = new AttributeModifier(_newModifierType, _newModifierSource, _newModifierValue);
                        attribute.AddModifier(newMod);
                        // 重置临时表单
                        _newModifierValue = 0;
                        _newModifierSource = "";
                    }else {
                        Debug.LogWarning("添加失败：修改器来源不能为空！");
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
}
