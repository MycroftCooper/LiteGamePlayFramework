using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LitePlayQuickFramework.AttributeSystem {
    [Serializable]
    public class Attribute {
        public string Owner { get; internal set; }
        public readonly string Name;
        [SerializeField] private float baseValue;
        
        public float BaseValue => baseValue;
        public float FinalValue { get; private set; }
        public int RoundedFinalValue { get; private set; }
        public readonly CalculateTypes CalculateType = CalculateTypes.AddThenMultiply;
        public bool HasClamp => minValue != null || maxValue != null;
        [SerializeField] public Attribute maxValue;
        [SerializeField] public Attribute minValue;
        
        public bool IsLocked => Modifiers.Any(m => m.Type == ModifierTypes.Locked);
        public List<AttributeModifier> Modifiers = new List<AttributeModifier>();
        public Func<AttributeModifier, bool, bool> CanModifierChange;
        public Action<AttributeChangedInfo> OnValueChanged;
        
        public Attribute(string name) {
            Owner = null;
            Name = name;
            if (string.IsNullOrEmpty(name)) {
                Debug.LogWarning("[Attribute] name is empty!");
            }
            CalculateFinalValue();
        }
        
        public Attribute(string name, string owner = null, float baseValue = 0, Attribute min = null, Attribute max = null) {
            Owner = owner;
            Name = name;
            this.baseValue = baseValue;
            maxValue = max;
            minValue = min;
            if (string.IsNullOrEmpty(owner)) {
                Debug.LogWarning("[Attribute] owner is empty!");
            }
            if (string.IsNullOrEmpty(name)) {
                Debug.LogWarning("[Attribute] name is empty!");
            }

            CalculateFinalValue();
        }
        
        public void AddModifier(AttributeModifier modifier) {
            if (IsLocked) {
                return;
            }
            bool canAdd = CanModifierChange == null || CanModifierChange.Invoke(modifier, true);
            if (!canAdd) {
                return;
            }
            Modifiers.Add(modifier);
            
            float oldValue = FinalValue;
            CalculateFinalValue();
            AttributeChangedInfo info = new AttributeChangedInfo {
                OldValue = oldValue,
                NewValue = FinalValue,
                ChangedModifiers = new Dictionary<AttributeModifier, ModifierChangeTypes> 
                    { {modifier, ModifierChangeTypes.Add} },
                Attribute = this,
            };
            OnValueChanged?.Invoke(info);
        }

        public void RemoveModifier(AttributeModifier modifier) {
            if (Modifiers.Any(m => m.Type == ModifierTypes.Locked && m != modifier)) {
                return;
            }
            bool canRemove = CanModifierChange == null || CanModifierChange.Invoke(modifier, false);
            if (!canRemove) {
                return;
            }
            Modifiers.Remove(modifier);
            
            float oldValue = FinalValue;
            CalculateFinalValue();
            AttributeChangedInfo info = new AttributeChangedInfo {
                OldValue = oldValue,
                NewValue = FinalValue,
                ChangedModifiers = new Dictionary<AttributeModifier, ModifierChangeTypes> 
                    { {modifier, ModifierChangeTypes.Remove} },
                Attribute = this
            };
            OnValueChanged?.Invoke(info);
        }

        public void RemoveModifiersBySource(string source) {
            float oldValue = FinalValue;
            if (Modifiers.Any(m => m.Type == ModifierTypes.Locked)) return;
            var targets = Modifiers.FindAll(m => m.Source == source);
            if(targets.Count == 0) return;
            
            var changedModifiers = new Dictionary<AttributeModifier, ModifierChangeTypes>();
            foreach (var m in targets) {
                Modifiers.Remove(m);
                changedModifiers.Add(m, ModifierChangeTypes.Remove);
            }
            CalculateFinalValue();
            
            AttributeChangedInfo info = new AttributeChangedInfo {
                OldValue = oldValue,
                NewValue = FinalValue,
                ChangedModifiers = changedModifiers,
                Attribute = this,
            };
            OnValueChanged?.Invoke(info);
        }

        public void CalculateFinalValue() {
            // 检查Reset类型的Modifier
            if (Modifiers.Any(m => m.Type == ModifierTypes.Reset)) {
                FinalValue = baseValue;
                Modifiers.Clear();
                return;
            }

            // 检查Fixed类型的Modifier
            var fixedModifier = Modifiers.FindLast(m => m.Type == ModifierTypes.Fixed);
            if (fixedModifier != null) {
                FinalValue = fixedModifier.Value;
                return;
            }
            
            FinalValue = AttributeValueCalculator.CalculateFinalValue(this);
            RoundedFinalValue = Mathf.RoundToInt(FinalValue);// 四舍五入
        }

        public override string ToString() {
            var modifiersStr = new StringBuilder();
            foreach (var modifier in Modifiers) {
                modifiersStr.Append(modifier + "\n");
            }
            return $"Attribute> Owner:{Owner} Name:{Name} BaseValue:{baseValue} FinalValue:{FinalValue}\n{modifiersStr}";
        }
    }
}