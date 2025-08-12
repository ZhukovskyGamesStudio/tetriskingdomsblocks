using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Abstract {
    public class HasAnimationAndCallback : MonoBehaviour {
        [SerializeField]
        protected Animation _animation;
        [SerializeField]
        KnoledgeCanAnimated _knoledgeCanAnimated;
        
        private readonly int _letterSpeed = 15; // скорость появления букв в мс

        protected Action OnAnimationEnded;
        protected CancellationTokenSource _cts = new CancellationTokenSource();
        protected async UniTask WaitForAnimationEnded(CancellationToken cancellationToken) {
            await UniTask.WaitWhile(()=> _animation.isPlaying, cancellationToken: cancellationToken);
            gameObject.SetActive(false);
            OnAnimationEnded?.Invoke();
        }
        
        protected void RecreateToken() {
            _cts?.Cancel();
            _cts = new CancellationTokenSource(); // Создаём новый токен для отмены предыдущего
        }
        
        protected async UniTask TypeText(TextMeshProUGUI t,string text, CancellationToken cancellationToken) {
            ResizeBubbleForText(t, text);
            int type = Random.Range(0, 3);
            _knoledgeCanAnimated.SetAnimationState(type, true);
            await UniTask.Delay(_letterSpeed*3, cancellationToken: cancellationToken);
            for (int i = 1; i <= text.Length; i++) {
                t.maxVisibleCharacters = i;
                await UniTask.Delay(_letterSpeed, cancellationToken: cancellationToken); // скорость появления букв
            }
            _knoledgeCanAnimated.SetAnimationState(type, false);
        }

        protected static void ResizeBubbleForText(TextMeshProUGUI t, string text) {
            t.text = text; // Устанавливаем полный текст для расчёта размера
            t.maxVisibleCharacters = 0; // Скрываем текст
            LayoutRebuilder.ForceRebuildLayoutImmediate(t.rectTransform); // Принудительно обновляем размер
        }

        protected virtual void OnDestroy() {
            OnAnimationEnded = null;
            _cts?.Cancel();
        }
    }
}