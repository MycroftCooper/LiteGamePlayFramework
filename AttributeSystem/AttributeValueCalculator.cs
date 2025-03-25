using System;
using UnityEngine;

namespace LitePlayQuickFramework.AttributeSystem {
    public enum CalculateTypes { AddThenMultiply }

    public static class AttributeValueCalculator {
        public static float CalculateFinalValue(Attribute a) {
            switch (a.CalculateType) {
                case CalculateTypes.AddThenMultiply:
                    return AddThenMultiply(a);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static float AddThenMultiply(Attribute a) {
            float sumAdd = 0f;
            float productMul = 1f;
            var modifiers = a.Modifiers;
            var baseVal = a.BaseValue;

            foreach (var mod in modifiers) {
                switch (mod.Type) {
                    case ModifierTypes.Add:
                        sumAdd += mod.Value;
                        break;
                    case ModifierTypes.Multiply:
                        productMul *= mod.Value;
                        break;
                }
            }
            float final = (baseVal + sumAdd) * productMul;

            if (!a.HasClamp) return final;
            float minVal = a.minValue?.FinalValue ?? float.MinValue;
            float maxVal = a.maxValue?.FinalValue ?? float.MaxValue;
            final = Mathf.Clamp(final, minVal, maxVal);
            return final;
        }
    }
}