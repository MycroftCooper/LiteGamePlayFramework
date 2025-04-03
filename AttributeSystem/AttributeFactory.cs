using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace LitePlayQuickFramework.AttributeSystem {
    public static class AttributeFactory {
        public static void LoadSaveData(AttributeManager mgr, AttributesSaveData data) {
            mgr.Owner = data.owner;
            mgr.Attributes.Clear();

            // 第一阶段：创建所有属性，但不处理Min/Max和Modifiers，只构造基础属性
            foreach (var asd in data.attributes) {
                var attr = CreateAttribute(asd);
                mgr.AddAttribute(attr);
            }

            // 第二阶段：为每个属性寻找对应的MinValue和MaxValue属性
            foreach (var asd in data.attributes) {
                var attr = mgr.GetAttribute(asd.name);
                if (attr == null) continue;

                bool needCalculate = false;
                if (!string.IsNullOrEmpty(asd.minValueAttributeName)) {
                    var minAttr = mgr.GetAttribute(asd.minValueAttributeName);
                    attr.MinValue = minAttr;
                    needCalculate = true;
                }
                if (!string.IsNullOrEmpty(asd.maxValueAttributeName)) {
                    var maxAttr = mgr.GetAttribute(asd.maxValueAttributeName);
                    attr.MaxValue = maxAttr;
                    needCalculate = true;
                }

                if (needCalculate) {
                    attr.CalculateFinalValue();
                }
            }
        }
        
        public static AttributesSaveData CreateSaveData(AttributeManager mgr) {
            var saveData = new AttributesSaveData {
                owner = mgr.Owner,
                attributes = mgr.Attributes.Values.Select(CreateSaveData).ToList()
            };
            return saveData;
        }

        public static List<Attribute> LoadPrefabData(string owner, List<AttributePrefabConfig> configs) {
            if (string.IsNullOrEmpty(owner)) {
                Debug.LogError("owner cant be null or empty!");
                return null;
            }
            List<Attribute> result = new List<Attribute>();
            if (configs == null || configs.Count == 0) {
                return result;
            }

            foreach (var config in configs) {
                var attr = new Attribute(config.name, owner, config.baseValue);
                result.Add(attr);
                if (config.hasMinClamp) {
                    var minAttr = new Attribute($"Min{config.name}", owner, config.minValue);
                    attr.MinValue = minAttr;
                    result.Add(minAttr);
                }
                if (config.hasMaxClamp) {
                    var maxAttr = new Attribute($"Max{config.name}", owner, config.maxValue);
                    attr.MaxValue = maxAttr;
                    result.Add(maxAttr);
                }
            }
            return result;
        }
        
        private static Attribute CreateAttribute(AttributeSaveData data) {
            float baseVal = ParseValue(data.baseValue);
            var attr = new Attribute(data.name, data.owner, baseVal);
            // MinValue 和 MaxValue 暂时留空，需要在外部根据名称找到对应的Attribute实例再注入
            
            foreach (var modData in data.modifiers) {
                var modifier = CreateModifier(modData);
                attr.Modifiers.Add(modifier);
            }
            attr.CalculateFinalValue();
            return attr;
        }

        private static AttributeSaveData CreateSaveData(Attribute attribute) {
            var saveData = new AttributeSaveData {
                owner = attribute.Owner,
                name = attribute.Name,
                baseValue = ConvertToString(attribute.BaseValue),
                modifiers = attribute.Modifiers.Select(CreateSaveData).ToList(),
                minValueAttributeName = attribute.MinValue?.Name,
                maxValueAttributeName = attribute.MaxValue?.Name
            };
            return saveData;
        }

        private static ModifierSaveData CreateSaveData(AttributeModifier modifier) {
            return new ModifierSaveData {
                type = modifier.Type,
                source = modifier.Source,
                value = ConvertToString(modifier.Value)
            };
        }

        private static AttributeModifier CreateModifier(ModifierSaveData data) {
            float val = ParseValue(data.value);
            return new AttributeModifier(data.type, data.source, val);
        }
        
        private static string ConvertToString(float value) {
            return Convert.ToSingle(value).ToString(CultureInfo.InvariantCulture);
        }

        private static float ParseValue(string str) {
            return float.Parse(str, CultureInfo.InvariantCulture);
        }
    }
}