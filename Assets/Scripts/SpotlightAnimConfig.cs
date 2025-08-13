using UnityEngine;

namespace ScriptableObjects {
    [CreateAssetMenu(fileName = "SpotlightAnim", menuName = "Scriptable Objects/SpotlightAnimConfig", order = 7)]
    public class SpotlightAnimConfig : ScriptableObject {
        [HideInInspector]
        //в тетрисе мы это не используем, т.к. у нас подсветки работают через слои, а не дырки
        public Vector2 SpotlightSize;
        public Vector2 HeadShift;
        [TextArea]
        public string HintText;
        
        //[LocalizationKey("Ftue")]
        //public string HintTextLoc;

        public string GetLocalizedText => HintText; //LocalizationUtils.L(config.HintTextLoc);
    }
}