using UnityEngine;

namespace ScriptableObjects.Configs {
    [CreateAssetMenu(fileName = "DragConfig", menuName = "Scriptable Objects/DragConfig", order = 0)]
    public class DragConfig : ScriptableObject {
        public Vector3 DragMouseShift;
        public float LerpSpeed = 20f;
        public float HigherFieldShift = 0.5f;

        public float HeightUnderField;

        public bool IsHighlightedLayer;
        
        
        [Header("Drop animation")]
        public bool IsSquishingOnDrop = false;
        public float MoveBeforeDropAnimationSpeed;
        public AnimationCurve MoveBeforeDropAnimationCurve;
        public float AfterDropPieceAnimationMultiplayer;
        public float _delayBetweenTileDrop = 0.15f, _delayBetweenDecorDrop=0.1f;
        public float _dropLength = 0.3f;
        public AnimationCurve DropPieceAnimationCurve;
        public float _callbackPercent = 0.75f;
        public float smokeVerticalShift = -0.5f;
        
        [Header("Destroy animation")]
        public float DestroyPieceAnimationMultiplayer;
    }
    
}