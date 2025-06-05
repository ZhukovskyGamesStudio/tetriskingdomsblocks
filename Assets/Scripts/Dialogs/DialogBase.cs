using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialogBase : MonoBehaviour {
    [SerializeField]
    private DialogShowHideAnimation _showHideAnimation;

    private Action _onClose;
    private bool _isHiding;

    public virtual async UniTask Show(Action onClose) {
        _onClose = onClose;
        await _showHideAnimation.Show();
    }

    public async UniTask Hide() {
        if (_isHiding) {
            return;
        }

        _isHiding = true;
        await _showHideAnimation.Hide();
        _onClose?.Invoke();
    }
}