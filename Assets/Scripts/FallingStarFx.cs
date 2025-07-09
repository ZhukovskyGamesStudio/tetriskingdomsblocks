using UnityEngine;

public class FallingStarFx : MonoBehaviour {
    [SerializeField]
    private ParticleSystem _boomFx;

    public void ShowBoom(Transform fxContainer) {
        Instantiate(_boomFx, transform.position, Quaternion.identity, fxContainer);
    }
}