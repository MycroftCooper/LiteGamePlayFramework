using LitePlayQuickFramework.AttributeSystem;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

namespace LiteGamePlayFramework.AttributeSystem.Editor {
    // todo:优化编辑器，移除对Odin的依赖
    
    [CustomEditor(typeof(AttributeManager), true)]
    public class AttributeManagerEditor : UnityEditor.Editor {
        private AttributeManager mgr;
        private Vector2 scroll;

        private string newAttrName = "Health";
        private float newAttrBase = 100f;
        private float newAttrMin = 0f;
        private float newAttrMax = 999f;

        private string removeAttrName = "Health";
        private string removeSource = "TestSource";

        private void OnEnable() {
            mgr = (AttributeManager)target;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector(); // 保留原有字段

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Attribute Debug Panel", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawAttributesSection();

            EditorGUILayout.Space(10);
            DrawAddAttributeSection();
            EditorGUILayout.Space(10);
            DrawRemoveAttributeSection();
            EditorGUILayout.Space(10);
            DrawModifierUtilitySection();
            EditorGUILayout.Space(10);
            DrawSaveLoadSection();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAttributesSection() {
            if (mgr.Attributes == null || mgr.Attributes.Count == 0) {
                EditorGUILayout.LabelField("No attributes found.");
                return;
            }

            foreach (var attr in mgr.Attributes.Values.ToList()) {
                EditorGUILayout.BeginVertical("helpBox");
                EditorGUILayout.LabelField($"[ {attr.Name} ]", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Base: {attr.BaseValue}, Final: {attr.FinalValue:F2}, Clamp: [{attr.minValue?.FinalValue}, {attr.maxValue?.FinalValue}]");

                if (attr.Modifiers.Count > 0) {
                    EditorGUILayout.LabelField("Modifiers:");
                    foreach (var mod in attr.Modifiers.ToList()) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"[{mod.Type}] {mod.Source} = {mod.Value}", GUILayout.MaxWidth(300));
                        if (GUILayout.Button("X", GUILayout.Width(25))) {
                            attr.RemoveModifier(mod);
                            GUIUtility.ExitGUI(); // 立即退出当前 GUI 循环避免异常
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Add Modifier:");
                EditorGUILayout.BeginHorizontal();
                var type = (ModifierTypes)EditorGUILayout.EnumPopup(ModifierTypes.Add, GUILayout.Width(80));
                string source = EditorGUILayout.TextField("Source", "", GUILayout.Width(150));
                float value = EditorGUILayout.FloatField("Value", 0f, GUILayout.Width(100));
                if (GUILayout.Button("Add", GUILayout.Width(50))) {
                    attr.AddModifier(new AttributeModifier(type, source, value));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4);
                if (GUILayout.Button("Remove Attribute", GUILayout.Width(150))) {
                    mgr.Attributes.Remove(attr.Name);
                    mgr.Attributes.Remove("Min" + attr.Name);
                    mgr.Attributes.Remove("Max" + attr.Name);
                    GUIUtility.ExitGUI(); // 防止集合变动导致遍历异常
                }

                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawAddAttributeSection() {
            EditorGUILayout.LabelField("Add New Attribute", EditorStyles.boldLabel);
            newAttrName = EditorGUILayout.TextField("Name", newAttrName);
            newAttrBase = EditorGUILayout.FloatField("Base Value", newAttrBase);
            newAttrMin = EditorGUILayout.FloatField("Min Value", newAttrMin);
            newAttrMax = EditorGUILayout.FloatField("Max Value", newAttrMax);
            if (GUILayout.Button("Add Attribute")) {
                mgr.AddAttribute(newAttrName, newAttrBase, newAttrMin, newAttrMax);
            }
        }

        private void DrawRemoveAttributeSection() {
            EditorGUILayout.LabelField("Remove Attribute (By Name)", EditorStyles.boldLabel);
            removeAttrName = EditorGUILayout.TextField("Name", removeAttrName);
            if (GUILayout.Button("Remove Attribute")) {
                mgr.Attributes.Remove(removeAttrName);
                mgr.Attributes.Remove("Min" + removeAttrName);
                mgr.Attributes.Remove("Max" + removeAttrName);
            }
        }
        
        private void DrawModifierUtilitySection() {
            EditorGUILayout.LabelField("Remove Modifiers by Source", EditorStyles.boldLabel);
            removeSource = EditorGUILayout.TextField("Source", removeSource);
            if (GUILayout.Button("Remove All Modifiers from Source")) {
                mgr.RemoveModifierBySource(removeSource);
            }
        }

        private void DrawSaveLoadSection() {
            EditorGUILayout.LabelField("Save / Load", EditorStyles.boldLabel);
            if (GUILayout.Button("Export SaveData (Log)")) {
                var json = JsonUtility.ToJson(mgr.ToSaveData(), true);
                Debug.Log("[AttributeManager] Exported SaveData:\n" + json);
            }

            if (GUILayout.Button("Import SaveData (From Clipboard)")) {
                string json = EditorGUIUtility.systemCopyBuffer;
                try {
                    var saveData = JsonUtility.FromJson<AttributesSaveData>(json);
                    mgr.LoadSaveData(saveData);
                    Debug.Log("[AttributeManager] Imported save data.");
                }
                catch (Exception e) {
                    Debug.LogError($"Import failed: {e}");
                }
            }
        }
    }
}