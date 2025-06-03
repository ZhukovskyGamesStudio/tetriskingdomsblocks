using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialogShowHideAnimation : MonoBehaviour {
    private Animation _animation;

    [SerializeField]
    private AnimationClip _showClip, _hideClip;

    private bool _isShown;

    public async UniTask Show() {
        if (_isShown) {
            return;
        }

        _isShown = true;
        EnsureAnimationComponent();
        _animation.Play(_showClip.name);

        await UniTask.WaitWhile(() => _animation.isPlaying);
    }

    public async UniTask Hide() {
        if (!_isShown) {
            return;
        }

        _isShown = false;
        EnsureAnimationComponent();
        _animation.Play(_hideClip.name);

        await UniTask.WaitWhile(() => _animation.isPlaying);
    }

    private void EnsureAnimationComponent() {
        if (_animation == null) {
            _animation = GetComponent<Animation>();
        }
    }
}