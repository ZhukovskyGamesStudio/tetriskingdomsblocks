using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Configs {
    [CreateAssetMenu(fileName = "CellsList", menuName = "Scriptable Objects/CellsList", order = 0)]
    public class CellsList : ScriptableObject {
        public List<CellTypeInfo> CellsConfigs;
    }
}