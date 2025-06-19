using UnityEngine;

namespace ScriptableObjects.Configs {
    [CreateAssetMenu(fileName = "DragConfig", menuName = "Scriptable Objects/DragConfig", order = 0)]
    public class DragConfig : ScriptableObject {
        public Vector3 DragMouseShift;
        public Vector3 AdditionalOffset;
        public Vector3 OnFieldOffset;
    }
    
}