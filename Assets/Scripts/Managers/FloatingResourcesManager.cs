using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FloatingResourcesManager : MonoBehaviour {
    public static FloatingResourcesManager Instance;
    private ObjectPool<Image> _floatingImagePool;

    [SerializeField]
    private const float _interval = 0.1f;

    [SerializeField]
    private Image _floatingImagePrefab;

    [SerializeField]
    private Transform _floatingTextContainer;

    public Action<ResourceType> OnAnimationEnd;
    private bool _isAnimationActive = false;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        _floatingImagePool = new ObjectPool<Image>(() => Instantiate(_floatingImagePrefab, _floatingTextContainer));
        OnAnimationEnd += type => _isAnimationActive = false;
    }

    public Image ShowFloatingImage() {
        var floatingImage = _floatingImagePool.Get();
        floatingImage.gameObject.SetActive(true);
        return floatingImage;
    }

    public void ReleaseFloatingImage(Image needTextObject) {
        needTextObject.gameObject.SetActive(false);
        _floatingImagePool.Release(needTextObject);
    }

    public async UniTask OnAnimationEndAsync() {
        await UniTask.WaitWhile(() => _isAnimationActive);
    } 

    public async UniTask FromPointToPointAnimation(int needCount, ResourceType resourceType, Vector2 startWorldPos, Vector2 endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, bool ActionAfterEndAnimaation,
        float interval = _interval) {
        float currentCount = needCount;
        _isAnimationActive = true;
        if (needCount > 30)
            needCount = 30;

        float addedCountToText = currentCount / needCount;
        if (isRemoveResources)
            addedCountToText = -addedCountToText;

        List<UniTask> tasks = new List<UniTask>();
        for (int i = 0; i < needCount; i++) {
            tasks.Add(ImageAnimation(resourceType, startWorldPos, endWorldPos, changeTextAction, startCount, isRemoveResources, i,
                addedCountToText, ActionAfterEndAnimaation));

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }

        await UniTask.WhenAll(tasks);
        OnAnimationEnd?.Invoke(resourceType);
    }

    public async UniTask FromSomePointsToPointAnimation(ResourceType resourceType, List<Vector2> startWorldPos, Vector2 endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, bool ActionAfterEndAnimaation,
        float interval = _interval) {
        _isAnimationActive = true;
        float currentCount = startWorldPos.Count;

        float addedCountToText = currentCount / startWorldPos.Count;
        if (isRemoveResources)
            addedCountToText = -addedCountToText;

        List<UniTask> tasks = new List<UniTask>();
        for (int i = 0; i < startWorldPos.Count; i++) {
            tasks.Add(ImageAnimation(resourceType, startWorldPos[i], endWorldPos, changeTextAction, startCount, isRemoveResources, i,
                addedCountToText, ActionAfterEndAnimaation));

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }

        await UniTask.WhenAll(tasks);
        OnAnimationEnd?.Invoke(resourceType);
    }

    public async UniTask FromPointToSomePointsAnimation(ResourceType resourceType, Vector2 startWorldPos, List<Vector2> endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, bool ActionAfterEndAnimaation,
        float interval = _interval) {
        _isAnimationActive = true;
        float currentCount = endWorldPos.Count;

        float addedCountToText = currentCount / endWorldPos.Count;
        if (isRemoveResources)
            addedCountToText = -addedCountToText;

        List<UniTask> tasks = new List<UniTask>();
        for (int i = 0; i < endWorldPos.Count; i++) {
            tasks.Add(ImageAnimation(resourceType, startWorldPos, endWorldPos[i], changeTextAction, startCount, isRemoveResources, i,
                addedCountToText, ActionAfterEndAnimaation));

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }

        await UniTask.WhenAll(tasks);
        OnAnimationEnd?.Invoke(resourceType);
    }

    public async UniTask FromSomePointsToPointMultiplyResourcesAnimation(List<ResourceType> resourceType, List<Vector2> startWorldPos,
        Vector2 endWorldPos, Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources,
        bool ActionAfterEndAnimaation, float interval = _interval) {
        _isAnimationActive = true;
        float currentCount = startWorldPos.Count;

        float addedCountToText = currentCount / startWorldPos.Count;
        if (isRemoveResources)
            addedCountToText = -addedCountToText;

        List<UniTask> tasks = new List<UniTask>();
        for (int i = 0; i < startWorldPos.Count; i++) {
            tasks.Add(ImageAnimation(resourceType[i], startWorldPos[i], endWorldPos, changeTextAction, startCount, isRemoveResources, i,
                addedCountToText, ActionAfterEndAnimaation));

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }

        await UniTask.WhenAll(tasks);
        OnAnimationEnd?.Invoke(resourceType[0]);
    }

    private async UniTask ImageAnimation(ResourceType resourceType, Vector2 startWorldPos, Vector2 endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, int i, float addedCountToText,
        bool ActionAfterEndAnimaation) {
        Vector2 midPoint = (startWorldPos + endWorldPos) / 2;

        Vector2 direction = (endWorldPos - startWorldPos).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        float randomSign = Random.Range(0, 2) * 2 - 1;
        float randomFactor = Random.Range(0.5f, 1f) * 0.5f * randomSign;

        Vector2 controlPoint = midPoint + Vector2.up * 150 + perpendicular * randomFactor * (endWorldPos - startWorldPos).magnitude * 0.3f;
        Vector3[] path = { startWorldPos, controlPoint, endWorldPos };

        var uiElement = ShowFloatingImage();
        uiElement.sprite = SpritesManager.Instance.GetSprite(resourceType);
        uiElement.transform.position = startWorldPos;

        if (!ActionAfterEndAnimaation)
            changeTextAction?.Invoke(resourceType, addedCountToText);

        await uiElement.rectTransform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.OutQuad).OnComplete(() => {
            ReleaseFloatingImage(uiElement);
            if (ActionAfterEndAnimaation)
                changeTextAction?.Invoke(resourceType, addedCountToText);
        }).AsyncWaitForCompletion();
    }
}