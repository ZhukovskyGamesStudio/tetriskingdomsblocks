using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Configs {
    [CreateAssetMenu(fileName = "CellsList", menuName = "Scriptable Objects/CellsList", order = 0)]
    public class CellsList : ScriptableObject {
        public List<CellTypeInfo> CellsConfigs;
        public List<MetaCellTypeInfo> MetaCellsConfigs;
        public List<CoreCellTypeInfo> CoreCellsConfigs;
        private List<CellTypeInfo> _combined = new List<CellTypeInfo>();
        public List<CellTypeInfo> Combined() {
            if (_combined.Count == 0) {
                _combined.AddRange(CellsConfigs);
                _combined.AddRange(MetaCellsConfigs);
                _combined.AddRange(CoreCellsConfigs);
            }
            Debug.Log(_combined.Count);
            foreach (var type in _combined) {
                Debug.Log(type.CellType);
            }
            return _combined;
        }
    }
}