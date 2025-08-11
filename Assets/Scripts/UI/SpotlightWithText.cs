using System;
using Abstract;
using Cysharp.Threading.Tasks;
//using Localization;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class SpotlightWithText : HasAnimationAndCallback {
        [SerializeField]
        protected TextMeshProUGUI _hintText;

        [SerializeField]
        private CanvasGroup _centerImageCanvasGroup;

        [SerializeField]
        protected RectTransform _shadowTransform, _shadowCenter, _headShift, _textBubble, _headFinPosAnchor;

        [SerializeField]
        protected CanvasGroup _textBubbleCanvas;

        private const string SHOW = "SpotlightShow";
        private const string HIDE = "SpotlightHide";

        private Button _target;
        private Action _onButtonPressed;
        private bool _isHidingByAnyTap;
        private bool _isShown;
        private bool _isHidingAfter;

        public async void ShowSpotlightOnButton(Button target, SpotlightAnimConfig animDataConfig, Action onButtonPressed = null,
            bool isHidingAfter = false) {
            _isHidingAfter = isHidingAfter;
            gameObject.SetActive(true);
            if (_isShown) {
                JumpSpotlight(target.transform.position, animDataConfig);
            } else {
                ShowSpotlight(target.transform.position, animDataConfig);
            }

            _target = target;
            target.onClick.AddListener(OnTargetButtonPressed);
            OnAnimationEnded = onButtonPressed;
            _isHidingByAnyTap = false;
            ChangeCenterBlockRaycast(_isHidingByAnyTap);
        }

        public async void ShowSpotlight(Transform target, SpotlightAnimConfig animDataConfig, Action onHideEnded = null,
            bool isHidingByAnyTap = true, bool isHidingAfter = false) {
            _isHidingAfter = isHidingAfter;
            gameObject.SetActive(true);

            if (_isShown) {
                JumpSpotlight(target.transform.position, animDataConfig);
            } else {
                ShowSpotlight(target.transform.position, animDataConfig);
            }

            OnAnimationEnded = onHideEnded;
            _isHidingByAnyTap = isHidingByAnyTap;
            ChangeCenterBlockRaycast(_isHidingByAnyTap);
        }

        public async void MoveHead(Transform target, SpotlightAnimConfig animDataConfig, Action onHideEnded = null,
            bool isHidingByAnyTap = true, bool isHidingAfter = false) {
            _isHidingAfter = isHidingAfter;
            gameObject.SetActive(true);

            if (_isShown) {
                MoveHeadOnly(animDataConfig);
            } else {
                ShowSpotlight(target.transform.position, animDataConfig);
            }

            OnAnimationEnded = onHideEnded;
            _isHidingByAnyTap = isHidingByAnyTap;
            ChangeCenterBlockRaycast(_isHidingByAnyTap);
        }

        public void JumpSpotlightFromVeryBigOnButton(Button target, SpotlightAnimConfig animDataConfig, Action onButtonPressed = null,
            bool isHidingAfter = false) {
            _isHidingAfter = isHidingAfter;
            gameObject.SetActive(true);

            JumpSpotlightFromVeryBig(target.transform, animDataConfig);

            _target = target;
            target.onClick.AddListener(OnTargetButtonPressed);
            OnAnimationEnded = onButtonPressed;
            _isHidingByAnyTap = false;
            ChangeCenterBlockRaycast(_isHidingByAnyTap);
        }

        private async void JumpSpotlightFromVeryBig(Transform target, SpotlightAnimConfig config) {
            RecreateToken();
            _headFinPosAnchor.position = target.transform.position;
            _shadowTransform.position = target.transform.position;
            _hintText.maxVisibleCharacters = 0;

            ResizeBubbleForText(_hintText, config.GetLocalizedText);
            await JumpSpotlightEnd(config);
            TypeText(_hintText, config.GetLocalizedText, _cts.Token).Forget();
        }

        private void OnTargetButtonPressed() {
            if (_isHidingAfter) {
                HideSpotlight();
            } else {
                OnAnimationEnded?.Invoke();
            }

            _target.onClick.RemoveListener(OnTargetButtonPressed);
            _target = null;
        }

        private void ShowSpotlight(Vector3 targetPos, SpotlightAnimConfig config) {
            _textBubbleCanvas.alpha = 1;
            _shadowTransform.position = targetPos;
            _headFinPosAnchor.position = targetPos;
            _headShift.anchoredPosition = _shadowTransform.anchoredPosition + config.HeadShift;
            _shadowCenter.sizeDelta = config.SpotlightSize;
            RecreateToken();
            TypeText(_hintText, config.GetLocalizedText, _cts.Token).Forget();
            _animation.Play(SHOW);
            _isShown = true;
        }

        private void ChangeCenterBlockRaycast(bool isBlock) {
            _centerImageCanvasGroup.blocksRaycasts = isBlock;
        }

        public void Hide() {
            if (_isHidingAfter) {
                HideSpotlight();
            } else {
                OnAnimationEnded?.Invoke();
            }
        }

        public void HideButton() {
            if (_animation.isPlaying) {
                return;
            }

            if (_isHidingByAnyTap) {
                if (_isHidingAfter) {
                    HideSpotlight();
                } else {
                    OnAnimationEnded?.Invoke();
                }
            }
        }

        public void HideSpotlight() {
            _animation.Play(HIDE);
            WaitForAnimationEnded(_cts.Token).Forget();
            _isShown = false;
        }

        private async UniTask MoveSpotlight(Vector3 targetPos, SpotlightAnimConfig config) {
            RecreateToken();
            TypeText(_hintText, config.GetLocalizedText, _cts.Token).Forget();
            Vector3 startpos = _shadowTransform.position;
            Vector2 headPos = _headShift.anchoredPosition;
            Vector2 shadowSize = _shadowCenter.sizeDelta;

            float curTime = 0;
            float maxTime = 0.5f;
            do {
                float percent = curTime / maxTime;
                _shadowTransform.position = Vector3.Slerp(startpos, targetPos, Mathf.SmoothStep(0, 1, percent / 2));
                _headShift.anchoredPosition = Vector2.Lerp(headPos, config.HeadShift, Mathf.SmoothStep(0, 1, percent / 2));

                _shadowCenter.sizeDelta = Vector2.Lerp(shadowSize, Vector2.zero, Mathf.SmoothStep(0, 1, percent));
                _headShift.localScale = Vector3.Slerp(Vector3.one, Vector3.one * 0.9f, Mathf.SmoothStep(0, 1, percent));
                curTime += Time.deltaTime;
                await UniTask.WaitForEndOfFrame();
            } while (curTime <= maxTime);

            curTime = 0;
            do {
                float percent = curTime / maxTime;
                float globalPercent = 0.5f + percent / 2;
                _shadowTransform.position = Vector3.Slerp(startpos, targetPos, Mathf.SmoothStep(0, 1, globalPercent));
                _headShift.anchoredPosition = Vector2.Lerp(headPos, config.HeadShift, Mathf.SmoothStep(0, 1, globalPercent));

                _shadowCenter.sizeDelta = Vector2.Lerp(Vector2.zero, config.SpotlightSize, Mathf.SmoothStep(0, 1, percent));
                _headShift.localScale = Vector3.Slerp(Vector3.one * 0.9f, Vector3.one, Mathf.SmoothStep(0, 1, percent));

                curTime += Time.deltaTime;
                await UniTask.WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }

        private async UniTask JumpSpotlight(Vector3 targetPos, SpotlightAnimConfig config) {
            _headFinPosAnchor.position = targetPos;

            MoveHeadPos(config);
            await JumpSpotlightStart(config);
            RecreateToken();
            _shadowTransform.position = targetPos;
            _hintText.maxVisibleCharacters = 0;
            ResizeBubbleForText(_hintText, config.GetLocalizedText);
            await JumpSpotlightEnd(config);
            TypeText(_hintText, config.GetLocalizedText, _cts.Token).Forget();
        }

        private async UniTask JumpSpotlightStart(SpotlightAnimConfig config) {
            Vector2 headPos = _headShift.anchoredPosition;
            Vector2 shadowSize = _shadowCenter.sizeDelta;
            float curTime = 0;
            float maxTime = 0.5f;
            do {
                float percent = curTime / maxTime;
                //_shadowTransform.position = Vector3.Slerp(startpos, targetPos, Mathf.SmoothStep(0, 1, percent / 2));
                // _headShift.anchoredPosition =
                //   Vector2.Lerp(headPos, _headFinPosAnchor.anchoredPosition + config.HeadShift, Mathf.SmoothStep(0, 1, percent / 2));

                _shadowCenter.sizeDelta = Vector2.Lerp(shadowSize, Vector2.zero, Mathf.SmoothStep(0, 1, percent));
                _headShift.localScale = Vector3.Slerp(Vector3.one, Vector3.one * 0.9f, Mathf.SmoothStep(0, 1, percent));
                _textBubble.localScale = Vector3.Slerp(Vector3.one, Vector3.zero, Mathf.SmoothStep(0, 1, percent));
                _textBubbleCanvas.alpha = Mathf.SmoothStep(1, 0, percent * 2f);

                curTime += Time.deltaTime;
                await UniTask.WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }

        private async UniTask JumpSpotlightEnd(SpotlightAnimConfig config) {
            Vector2 startShadowSize = _shadowCenter.sizeDelta;
            Vector2 headPos = _headShift.anchoredPosition;
            float maxTime = 0.5f;
            float curTime = 0;
            do {
                float percent = curTime / maxTime;

                _shadowCenter.sizeDelta = Vector2.Lerp(startShadowSize, config.SpotlightSize, Mathf.SmoothStep(0, 1, percent));

                //_headShift.anchoredPosition = Vector2.Lerp(headPos, _headFinPosAnchor.anchoredPosition + config.HeadShift, Mathf.SmoothStep(0, 1, 0.5f +percent/2));
                _headShift.localScale = Vector3.Slerp(Vector3.one * 0.9f, Vector3.one, Mathf.SmoothStep(0, 1, percent));
                _textBubble.localScale = Vector3.Slerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, percent));
                _textBubbleCanvas.alpha = Mathf.SmoothStep(0, 1, (percent - 0.5f) * 2f);

                curTime += Time.deltaTime;
                await UniTask.WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }

        private async UniTask MoveHeadOnly(SpotlightAnimConfig config) {
            RecreateToken();
            Vector2 headPos = _headShift.anchoredPosition;
            float curTime = 0;
            float maxTime = 0.5f;
            do {
                float percent = curTime / maxTime;

                _headShift.localScale = Vector3.Slerp(Vector3.one, Vector3.one * 0.9f, Mathf.SmoothStep(0, 1, percent));
                _textBubble.localScale = Vector3.Slerp(Vector3.one, Vector3.zero, Mathf.SmoothStep(0, 1, percent));
                _textBubbleCanvas.alpha = Mathf.SmoothStep(1, 0, percent * 2f);

                curTime += Time.deltaTime;
                await UniTask.WaitForEndOfFrame();
            } while (curTime <= maxTime);

            _hintText.maxVisibleCharacters = 0;
            ResizeBubbleForText(_hintText, config.GetLocalizedText);
            await MoveHeadEnd(config);
            TypeText(_hintText, config.GetLocalizedText, _cts.Token).Forget();
        }

        private async UniTask MoveHeadEnd(SpotlightAnimConfig config) {
            Vector2 headPos = _headShift.anchoredPosition;
            float maxTime = 0.5f;
            float curTime = 0;
            do {
                float percent = curTime / maxTime;
                _headShift.anchoredPosition = Vector2.Lerp(headPos, _headFinPosAnchor.anchoredPosition + config.HeadShift,
                    Mathf.SmoothStep(0, 1, 0.5f + percent / 2));

                _headShift.localScale = Vector3.Slerp(Vector3.one * 0.9f, Vector3.one, Mathf.SmoothStep(0, 1, percent));
                _textBubble.localScale = Vector3.Slerp(Vector3.zero, Vector3.one, Mathf.SmoothStep(0, 1, percent));
                _textBubbleCanvas.alpha = Mathf.SmoothStep(0, 1, (percent - 0.5f) * 2f);

                curTime += Time.deltaTime;
                await UniTask.WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }

        private async UniTask MoveHeadPos(SpotlightAnimConfig config) {
            Vector2 headPos = _headShift.anchoredPosition;
            float maxTime = 1f;
            float curTime = 0;
            do {
                float percent = curTime / maxTime;
                _headShift.anchoredPosition = Vector2.Lerp(headPos, _headFinPosAnchor.anchoredPosition + config.HeadShift,
                    Mathf.SmoothStep(0, 1, percent));
                curTime += Time.deltaTime;
                await UniTask.WaitForEndOfFrame();
            } while (curTime <= maxTime);
        }
    }
}