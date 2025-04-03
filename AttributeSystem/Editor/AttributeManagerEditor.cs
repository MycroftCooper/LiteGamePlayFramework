using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LitePlayQuickFramework.AttributeSystem.EditorTools {
    [CustomEditor(typeof(AttributeManager), true)]
    public class AttributeManagerEditor : UnityEditor.Editor {
        private AttributeManager manager;

        // 折叠控制每个属性
        private readonly Dictionary<string, bool> attributeFoldouts = new();

        // 添加属性表单数据
        private string newAttributeName = "";
        private float newBaseValue;
        private bool addClamp;
        private float minClamp;
        private float maxClamp = 100;

        public override void OnInspectorGUI() {
            if (!Application.isPlaying) {
                DrawDefaultInspector();
                return;
            }

            manager = (AttributeManager)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("属性管理器调试工具", EditorStyles.boldLabel);

            DrawAttributeList();
            DrawAddAttributeForm();

            if (GUI.changed) {
                EditorUtility.SetDirty(manager);
            }
        }

        private void DrawAttributeList() {
            if (manager.Attributes == null || manager.Attributes.Count == 0) {
                EditorGUILayout.HelpBox("当前没有任何属性", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("属性列表", EditorStyles.boldLabel);

            foreach (var kvp in manager.Attributes) {
                var attr = kvp.Value;

                attributeFoldouts.TryAdd(attr.Name, false);

                attributeFoldouts[attr.Name] = EditorGUILayout.Foldout(attributeFoldouts[attr.Name], attr.Name);

                if (attributeFoldouts[attr.Name]) {
                    EditorGUI.indentLevel++;
                    AttributeEditor.DrawAttribute(attr);
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawAddAttributeForm() {
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("➕ 添加新属性", EditorStyles.boldLabel);

            newAttributeName = EditorGUILayout.TextField("属性名", newAttributeName);
            newBaseValue = EditorGUILayout.FloatField("基础值", newBaseValue);

            addClamp = EditorGUILayout.Toggle("添加范围限制 (Clamp)", addClamp);
            if (addClamp) {
                minClamp = EditorGUILayout.FloatField("最小值", minClamp);
                maxClamp = EditorGUILayout.FloatField("最大值", maxClamp);
            }

            GUI.enabled = !string.IsNullOrEmpty(newAttributeName);
            if (GUILayout.Button("添加属性")) {
                if (addClamp) {
                    manager.AddAttribute(newAttributeName, newBaseValue, minClamp, maxClamp);
                }
                else {
                    manager.AddAttribute(newAttributeName, newBaseValue);
                }

                // 重置表单
                newAttributeName = "";
                newBaseValue = 0;
                minClamp = 0;
                maxClamp = 100;
                addClamp = false;

                // 标记脏
                EditorUtility.SetDirty(manager);
            }

            GUI.enabled = true;
        }
    }
}
