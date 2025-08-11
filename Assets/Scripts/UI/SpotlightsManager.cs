using UI;
using UnityEngine;

public class SpotlightsManager : MonoBehaviour {
    public static SpotlightsManager Instance;

    [field: SerializeField]
    public Transform FingerTransform { get; private set; }
    
    [field: SerializeField]
    public Transform CenterScreenAnchor { get; private set; }

    [field: SerializeField]
    public SpotlightWithText SpotlightWithText { get; private set; }

    [field: SerializeField]
    public ShadowWithText ShadowWithText { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}