using System;
using System.Collections.Generic;

namespace LitePlayQuickFramework.AttributeSystem {
    #region PrefabData
    [Serializable]
    public class AttributePrefabConfig {
        public string name;
        public float baseValue;
        public bool hasMinClamp;
        public float minValue = float.MinValue;
        public bool hasMaxClamp;
        public float maxValue = float.MaxValue;
    }
    #endregion

    #region SaveData
    [Serializable]
    public class AttributesSaveData {
        public string owner;
        public List<AttributeSaveData> attributes = new List<AttributeSaveData>();
    }

    [Serializable]
    public class AttributeSaveData {
        public string owner;
        public string name;
        public string baseValue;
        public List<ModifierSaveData> modifiers = new List<ModifierSaveData>();
        public string minValueAttributeName;
        public string maxValueAttributeName;
    }

    [Serializable]
    public class ModifierSaveData {
        public ModifierTypes type;
        public string source;
        public string value;
    }
    #endregion
    
    #region NetworkData
    // todo: 实现网络同步数据
    #endregion
}