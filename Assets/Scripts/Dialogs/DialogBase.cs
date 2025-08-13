using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DialogBase : MonoBehaviour {
    [SerializeField]
    private DialogShowHideAnimation _showHideAnimation;

    private Action _onClose;
    private bool _isHiding;

    public bool ForceOverlay;

    public virtual async UniTask Show(Action onClose) {
        _onClose = onClose;
        await _showHideAnimation.Show();
    }

    public virtual void SetData(object data) { }

    public async UniTask HideAnimation() {
        if (_isHiding) {
            return;
        }

        _isHiding = true;
        await _showHideAnimation.Hide();
    }
    
    public async UniTask Hide() {
        await HideAnimation();
        
        _onClose?.Invoke();
    }

    public void HideByButton() {
        Hide().Forget();
    }
}