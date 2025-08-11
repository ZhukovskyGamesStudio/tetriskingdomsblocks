using System;
using Abstract;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace UI {
    public class ShadowWithText : HasAnimationAndCallback {
        [SerializeField]
        protected TextMeshProUGUI _speakText;

        [SerializeField]
        private AnimationClip _showAnimationClip, _hideAnimationClip, _changeStartAnimationClip,_changeEndAnimationClip;
        
        private bool _isHidingAfter;
        private string _hintTextToUpdate;

        public void ShowSpeak(string text, Action onHideEnded = null, bool isHidingAfter = false) {
            gameObject.SetActive(true);
            RecreateToken(); // Создаём новый токен для отмены предыдущего
            
            OnAnimationEnded = onHideEnded;
            _animation.Play(_showAnimationClip.name);
            _isHidingAfter = isHidingAfter;

            TypeText(_speakText, text, _cts.Token).Forget();
        }

        public async void ChangeSpeak(string text, Action onHideEnded = null, bool isHidingAfter = false) {
            RecreateToken();
            _hintTextToUpdate = text;
            gameObject.SetActive(true);
            
            OnAnimationEnded = onHideEnded;
            _animation.Play(_changeStartAnimationClip.name);
            await UniTask.WaitWhile(()=> _animation.isPlaying, cancellationToken: _cts.Token);
            RecreateToken();
            TypeText(_speakText,_hintTextToUpdate, _cts.Token).Forget();
            _animation.Play(_changeEndAnimationClip.name);
            _isHidingAfter = isHidingAfter;
        }
        
        public void HideSpeak() {
            RecreateToken();
            if (_isHidingAfter) {
                _animation.Play(_hideAnimationClip.name);
                WaitForAnimationEnded(_cts.Token).Forget();
            } else {
                OnAnimationEnded?.Invoke();
            }
        }

        public void HideSpeakWithoutCallback() {
            OnAnimationEnded = null;
            _animation.Play(_hideAnimationClip.name);
        }
        

        protected override void OnDestroy() {
            base.OnDestroy();
            _cts?.Cancel();
        }
    }
}