using System;
using System.Collections.Generic;
using UnityEngine;

namespace LitePlayQuickFramework.UI.RedDotSystem {
    public interface IRedDotUI {
        public string RedDotKey { get; }
        public void SetRedDotState(bool active, int dotCount);
    }

    public interface IRedDotDataSource {
        public string RedDotKey { get; }
        public int RedDotCount { get; }
        public Action<string, int> OnRedDotCountChanged {get; set; }
    }
    
    public class RedDotManager: MonoBehaviour {
        public static RedDotManager Instance { get; private set; }
        
        private void Awake() {
            if (Instance != null) return;
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _redDotTree = new RedDotTree();
        }
        
        #region 数据源相关
        private readonly Dictionary<string, IRedDotDataSource> _redDotDataSourceDict = new();
        
        public void BindRedDotDataSource(IRedDotDataSource redDotDataSource) {
            if (redDotDataSource == null) {
                Debug.LogError("RedDotManager>BindRedDotDataSource: redDotDataSource cant be null!");
                return;
            }
            var key = redDotDataSource.RedDotKey;
            if (string.IsNullOrEmpty(key)) {
                Debug.LogError("RedDotManager>BindRedDotDataSource: redDotDataSource.RedDotKey cant be null!");
                return;
            }
            if (!_redDotDataSourceDict.TryAdd(key, redDotDataSource)) {
                Debug.LogError($"RedDotManager>BindRedDotDataSource: RedDotKey[{key}] is already bind! The old one will be replaced!");
                _redDotDataSourceDict[key] = redDotDataSource;
            }
            
            // 如果节点不存在，则创建节点
            if (_redDotTree.Contains(key)) {
                Debug.LogError($"RedDotManager>BindRedDotDataSource: RedDotKey[{key}] is already exist in tree!");
            } else {
                _redDotTree.AddNode(key, redDotDataSource.RedDotCount);
            }
            redDotDataSource.OnRedDotCountChanged += OnRedDotCountChangedHandler;
        }

        public void UnbindRedDotDataSource(string key) {
            if (!_redDotDataSourceDict.TryGetValue(key, out var redDotDataSource)) {
                Debug.LogError($"RedDotManager>UnbindRedDotDataSource:  RedDot[{key}] is not bind, cant found!");
                return;
            }
            redDotDataSource.OnRedDotCountChanged -= OnRedDotCountChangedHandler;
            _redDotDataSourceDict.Remove(key);
        }

        private void OnRedDotCountChangedHandler(string redDotKey, int redDotCount) {
            var changedNodes = _redDotTree.SetDotNodeSelfDotCount(redDotKey, redDotCount);
            if(changedNodes == null || changedNodes.Count == 0) return;
            foreach (var node in changedNodes) {
                RefreshRedDotUI(node);
            }
        }
        #endregion

        #region UI相关
        private readonly Dictionary<string, IRedDotUI> _redDotUIDict = new();
        
        public void BindAllRedDotUI(GameObject rootObject) {
            if (rootObject == null) {
                Debug.LogError("RedDotManager>BindAllRedDotUI: Provided GameObject is null!");
                return;
            }
            // 获取所有实现 IRedDotUI 接口的组件，包含 inactive 的对象
            var redDotComponents = rootObject.GetComponentsInChildren<IRedDotUI>(true);
            foreach (var redDotUI in redDotComponents) {
                BindRedDotUI(redDotUI);
            }
            Debug.Log($"RedDotManager>BindAllRedDotUI: Bound {redDotComponents.Length} IRedDotUI components under {rootObject.name}.");
        }
        
        public void BindRedDotUI(IRedDotUI redDotUI) {
            if (redDotUI == null) {
                Debug.LogError("RedDotManager>BindRedDotUI: IRedDotUI cant be null!");
                return;
            }
            var key = redDotUI.RedDotKey;
            if (string.IsNullOrEmpty(key)) {
                Debug.LogError("RedDotManager>BindRedDotUI: IRedDotUI.RedDotKey cant be null!");
                return;
            }
            if (!_redDotUIDict.TryAdd(key, redDotUI)) {
                Debug.LogError($"RedDotManager>BindRedDotUI: RedDotKey[{key}] is already bind! The old one will be replaced!");
                _redDotUIDict[key] = redDotUI;
            }
            
            if (!_redDotTree.TryGetNode(key, out var node)) {
                Debug.LogError($"RedDotManager>BindRedDotUI: RedDotNode[{key}] cant find! It has no RedDotDataSource!");
                return;
            }
            // 绑定后立即更新 UI 状态
            redDotUI.SetRedDotState(node.IsEnable, node.TotalDotCount);
        }

        public void UnbindRedDotUI(string key) {
            if (!_redDotUIDict.ContainsKey(key)) {
                Debug.LogError($"RedDotManager>UnbindRedDotUI:  RedDot[{key}] is not bind, cant found!");
                return;
            }
            _redDotUIDict.Remove(key);
        }

        private void RefreshRedDotUI(RedDotNode node) {
            if (!_redDotUIDict.TryGetValue(node.Key, out var redDotUI)) {
                Debug.LogError($"RedDotManager>RefreshRedDotUI>RedDot[{node.Key}] didn't bind any redDotUI!");
                return;
            }
            redDotUI.SetRedDotState(node.IsEnable, node.TotalDotCount);
        }
        #endregion
        
        private RedDotTree _redDotTree;
        
        public void RefreshAll() {
            var needUpdate = _redDotTree.Refresh();
            foreach (var node in needUpdate) {
                _redDotUIDict.TryGetValue(node.Key, out var redDotUI);
                redDotUI?.SetRedDotState(node.IsEnable, node.TotalDotCount);
            }
        }

        public bool RemoveRedDot(string key) {
            if (!_redDotTree.Contains(key)) {
                Debug.LogError($"RedDotManager>RemoveRedDot: Node with key [{key}] does not exist in red dot tree!");
                return false;
            }
            
            if (_redDotUIDict.ContainsKey(key)) {
                UnbindRedDotUI(key);
            }
            
            if (_redDotDataSourceDict.ContainsKey(key)) {
                UnbindRedDotDataSource(key);
            }
            
            var removed = _redDotTree.RemoveNode(key);
            RefreshAll();
    
            return removed;
        }

        #region Debug相关
        public void LogTree() {
            Debug.Log(_redDotTree.ToString());
            
            // 遍历所有节点，排除根节点（假设根节点不需要绑定）
            foreach (var node in _redDotTree.GetAllNodesInOrder()) {
                if (node.Key.Equals("Root", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!_redDotUIDict.ContainsKey(node.Key)) {
                    Debug.LogWarning($"RedDotManager>LogTree: Node with key [{node.Key}] has no UI bound!");
                }
                if (!_redDotDataSourceDict.ContainsKey(node.Key)) {
                    Debug.LogWarning($"RedDotManager>LogTree: Node with key [{node.Key}] has no DataSource bound!");
                }
            }
        }
        #endregion
    }
}