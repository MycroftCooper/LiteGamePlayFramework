using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LitePlayQuickFramework.AttributeSystem.EditorTools {
    [CustomEditor(typeof(AttributeManager), true)]
    public class AttributeManagerEditor : UnityEditor.Editor {
        private AttributeManager _manager;

        // 折叠控制每个属性
        private readonly Dictionary<string, bool> _attributeFoldouts = new();

        public override void OnInspectorGUI() {
            if (!Application.isPlaying) {
                DrawDefaultInspector();
                return;
            }

            _manager = (AttributeManager)target;
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Owner: {_manager.owner}", EditorStyles.boldLabel);
            DrawAttributeList();
            DrawRemoveModifierBlock();

            if (GUI.changed) {
                EditorUtility.SetDirty(_manager);
            }
        }

        private static float _columnWidth;
        private GUILayoutOption WidthOption => GUILayout.Width(_columnWidth);

        private void DrawAttributeList() {
            if (_manager.Attributes == null || _manager.Attributes.Count == 0) {
                EditorGUILayout.HelpBox("Has no attribute", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"AttributeList ({_manager.Attributes.Count})", EditorStyles.boldLabel);

            _columnWidth = (EditorGUIUtility.currentViewWidth - 60) / 4f; // 减去边距

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name", WidthOption);
            GUILayout.Label("Base", WidthOption);
            GUILayout.Label("Final", WidthOption);
            GUILayout.Label("Clamp", WidthOption);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            foreach (var kvp in _manager.Attributes) {
                DrawAttributeRow(kvp.Value);
            }
        }

        public void DrawAttributeRow(Attribute attribute) {
            if (attribute == null) return;
            _attributeFoldouts.TryAdd(attribute.Name, false);

            EditorGUILayout.BeginHorizontal("box");

            // 属性名 + 折叠箭头
            _attributeFoldouts[attribute.Name] = EditorGUILayout.Foldout(
                _attributeFoldouts[attribute.Name],
                attribute.Name,
                true
            );

            // Base
            GUILayout.Label(attribute.BaseValue.ToString(CultureInfo.CurrentCulture), WidthOption);

            // Final
            GUILayout.Label(attribute.FinalValue.ToString(CultureInfo.CurrentCulture), WidthOption);

            // Clamp（可选）
            if (attribute.HasClamp) {
                string minStr = attribute.MinValue?.FinalValue.ToString(CultureInfo.CurrentCulture) ?? "-∞";
                string maxStr = attribute.MaxValue?.FinalValue.ToString(CultureInfo.CurrentCulture) ?? "+∞";
                GUILayout.Label($"[{minStr}, {maxStr}]", WidthOption);
            } else {
                GUILayout.Label("-", WidthOption);
            }

            EditorGUILayout.EndHorizontal();

            // 折叠展开详细信息
            if (_attributeFoldouts[attribute.Name]) {
                EditorGUI.indentLevel++;
                AttributeEditor.DrawAttribute(attribute);
                EditorGUI.indentLevel--;
            }
        }

        private string _removeSource = "";
        private bool _showRemoveModifierBlock = false;
        private Dictionary<string, bool> _attributeSelection = new();


        private void DrawRemoveModifierBlock() {
            EditorGUILayout.Space(10);
            _showRemoveModifierBlock = EditorGUILayout.Foldout(_showRemoveModifierBlock, "🧹 移除指定来源的 Modifier", true);

            if (!_showRemoveModifierBlock) return;

            EditorGUILayout.BeginVertical("box");

            // 输入来源标识
            _removeSource = EditorGUILayout.TextField("来源标识 (Source)", _removeSource);

            // 构建属性名列表（+ Any 选项）
            if (_manager.Attributes != null && _manager.Attributes.Count > 0) {
                // 初始化选择状态
                foreach (var name in _manager.Attributes.Keys) {
                    if (!_attributeSelection.ContainsKey(name)) {
                        _attributeSelection[name] = false;
                    }
                }

                // 特殊项：Any
                if (!_attributeSelection.ContainsKey("Any")) {
                    _attributeSelection["Any"] = false;
                }

                DrawAttributeMultiSelectDropdown();

                GUILayout.Space(5);

                GUI.enabled = !string.IsNullOrEmpty(_removeSource);
                if (GUILayout.Button("移除修改器")) {
                    if (_attributeSelection.TryGetValue("Any", out var anySelected) && anySelected) {
                        _manager.RemoveModifierBySource(_removeSource);
                    } else {
                        foreach (var kvp in _attributeSelection) {
                            if (kvp.Key == "Any" || !kvp.Value) continue;
                            _manager.RemoveModifierBySource(_removeSource, kvp.Key);
                        }
                    }
                }

                GUI.enabled = true;
            } else {
                EditorGUILayout.HelpBox("无属性可操作", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }
        
        private void DrawAttributeMultiSelectDropdown() {
            if (_manager.Attributes == null || _manager.Attributes.Count == 0) return;

            // 初始化列表
            foreach (var attr in _manager.Attributes.Keys) {
                if (!_attributeSelection.ContainsKey(attr)) _attributeSelection[attr] = false;
            }
            if (!_attributeSelection.ContainsKey("Any")) _attributeSelection["Any"] = false;

            // 构建当前选择展示文本
            List<string> selected = _attributeSelection.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            string label = selected.Count == 0 ? "选择属性..." : string.Join(", ", selected);

            // 绘制按钮样式下拉
            if (GUILayout.Button(label, EditorStyles.popup)) {
                GenericMenu menu = new GenericMenu();

                foreach (var key in _attributeSelection.Keys.ToList()) {
                    bool isOn = _attributeSelection[key];
                    menu.AddItem(new GUIContent(key), isOn, () => {
                        // 如果是Any，清除其他
                        if (key == "Any") {
                            foreach (var k in _attributeSelection.Keys.ToList()) {
                                _attributeSelection[k] = false;
                            }
                            _attributeSelection["Any"] = true;
                        }
                        else {
                            _attributeSelection[key] = !_attributeSelection[key];
                            _attributeSelection["Any"] = false;
                        }
                    });
                }

                menu.ShowAsContext();
            }
        }


    }
}
