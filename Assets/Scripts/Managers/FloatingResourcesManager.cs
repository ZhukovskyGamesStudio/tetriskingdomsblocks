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

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        _floatingImagePool = new ObjectPool<Image>(() => Instantiate(_floatingImagePrefab, _floatingTextContainer));
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

    public async UniTask FromPointToPointAnimation(int needCount, ResourceType resourceType, Vector2 startWorldPos, Vector2 endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, bool ActionAfterEndAnimaation,
        float interval = _interval) {
        float currentCount = needCount;

        if (needCount > 30)
            needCount = 30;

        float addedCountToText = currentCount / needCount;
        if (isRemoveResources)
            addedCountToText = -addedCountToText;

        for (int i = 0; i < needCount; i++) {
            startCount = ImageAnimation(resourceType, startWorldPos, endWorldPos, changeTextAction, startCount, isRemoveResources, i,
                addedCountToText, ActionAfterEndAnimaation);

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }

    public async UniTask FromSomePointsToPointAnimation(ResourceType resourceType, List<Vector2> startWorldPos, Vector2 endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, bool ActionAfterEndAnimaation,
        float interval = _interval) {
        float currentCount = startWorldPos.Count;

        float addedCountToText = currentCount / startWorldPos.Count;
        if (isRemoveResources)
            addedCountToText = -addedCountToText;
        for (int i = 0; i < startWorldPos.Count; i++) {
            startCount = ImageAnimation(resourceType, startWorldPos[i], endWorldPos, changeTextAction, startCount, isRemoveResources, i,
                addedCountToText, ActionAfterEndAnimaation);

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }

    public async UniTask FromPointToSomePointsAnimation(ResourceType resourceType, Vector2 startWorldPos, List<Vector2> endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, bool ActionAfterEndAnimaation,
        float interval = _interval) {
        float currentCount = endWorldPos.Count;

        float addedCountToText = currentCount / endWorldPos.Count;
        if (isRemoveResources)
            addedCountToText = -addedCountToText;
        for (int i = 0; i < endWorldPos.Count; i++) {
            startCount = ImageAnimation(resourceType, startWorldPos, endWorldPos[i], changeTextAction, startCount, isRemoveResources, i,
                addedCountToText, ActionAfterEndAnimaation);

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }

    public async UniTask FromSomePointsToPointMultiplyResourcesAnimation(List<ResourceType> resourceType, List<Vector2> startWorldPos,
        Vector2 endWorldPos, Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources,
        bool ActionAfterEndAnimaation, float interval = _interval) {
        float currentCount = startWorldPos.Count;

        float addedCountToText = currentCount / startWorldPos.Count;
        if (isRemoveResources)
            addedCountToText = -addedCountToText;
        for (int i = 0; i < startWorldPos.Count; i++) {
            startCount = ImageAnimation(resourceType[i], startWorldPos[i], endWorldPos, changeTextAction, startCount, isRemoveResources, i,
                addedCountToText, ActionAfterEndAnimaation);

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }

    private float ImageAnimation(ResourceType resourceType, Vector2 startWorldPos, Vector2 endWorldPos,
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
        float newCount;

        if (!ActionAfterEndAnimaation)
            changeTextAction?.Invoke(resourceType, addedCountToText);

        uiElement.rectTransform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.OutQuad).OnComplete(() => {
            ReleaseFloatingImage(uiElement);
            if (ActionAfterEndAnimaation)
                changeTextAction?.Invoke(resourceType, addedCountToText);
        });
        return startCount;
    }
}