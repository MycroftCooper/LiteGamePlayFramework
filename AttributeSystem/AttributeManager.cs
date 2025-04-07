using System;
using System.Collections.Generic;
using UnityEngine;

namespace LitePlayQuickFramework.AttributeSystem {
    public class AttributeManager : MonoBehaviour {
        public string owner;
        public bool initByPrefabConfig;
        public List<AttributePrefabConfig> attributePrefabConfigs = new List<AttributePrefabConfig>();
        
        public virtual string Owner { get; set; }
        public Dictionary<string, Attribute> Attributes = new Dictionary<string, Attribute>();
        
        private void Awake() {
            Init();
        }
        
        public void Init() {
            if (initByPrefabConfig) {
                var list = AttributeFactory.LoadPrefabData(owner, attributePrefabConfigs);
                foreach (var attribute in list) {
                    Attributes.Add(attribute.Name, attribute);
                }
            }
        }

        private void InitByPrefabConfig() {
            
        }

        public void AddAttribute(Attribute attribute) {
            if (!Attributes.TryAdd(attribute.Name, attribute)) {
                Debug.LogError($"Duplicate attribute {attribute.Name}");
            }
        }

        public void AddAttribute(string attributeName, float baseValue) {
            if (Attributes.ContainsKey(attributeName)) {
                Debug.LogError($"Duplicate attribute {attributeName}");
                return;
            }
            Attribute a = new Attribute(attributeName, Owner, baseValue);
            AddAttribute(a);
        }
        
        public void AddAttribute(string attributeName, float baseValue, float minValue, float maxValue) {
            if (Attributes.ContainsKey(attributeName)) {
                Debug.LogError($"Duplicate attribute {attributeName}");
                return;
            }
            Attribute aMin = new Attribute($"Min{attributeName}", Owner, minValue);
            Attribute aMax = new Attribute($"Max{attributeName}", Owner, maxValue);
            Attribute a = new Attribute(attributeName, Owner, baseValue, aMin, aMax);
            Attributes.Add(attributeName, a);
            Attributes[aMin.Name] = aMin;
            Attributes[aMax.Name] = aMax;
        }

        public Attribute GetAttribute(string attributeName) {
            if (Attributes != null && Attributes.Count != 0 && Attributes.TryGetValue(attributeName, out var attribute)) {
                return attribute;
            }
            Debug.LogError($"AttributeManager> {Owner} doesn't has attribute {attributeName}!");
            return null;
        }

        public float GetAttributeFinalValue(string attributeName) {
            var attribute = GetAttribute(attributeName);
            return attribute?.FinalValue ?? 0;
        }

        public int GetAttributeRoundedFinalValue(string attributeName) {
            var attribute = GetAttribute(attributeName);
            return attribute.RoundedFinalValue;
        }

        public void AddModifierToAttribute(string attributeName, ModifierTypes type, string source, float value = 0) {
            var attribute = GetAttribute(attributeName);
            if (attribute == null) {
                return;
            }
            AttributeModifier modifier = new AttributeModifier(type, source, value);
            attribute.AddModifier(modifier);
        }
        
        public void RemoveModifierBySource(string source, string attributeName) {
            var attribute = GetAttribute(attributeName);
            attribute?.RemoveModifiersBySource(source);
        }
        
        public void RemoveModifierBySource(string source) {
            if (Attributes == null || Attributes.Count == 0) {
                return;
            }
            foreach (var a in Attributes.Values) {
                a.RemoveModifiersBySource(source);
            }
        }
        
        public Action<AttributeChangedInfo> OnAttributeChanged;
    }
}