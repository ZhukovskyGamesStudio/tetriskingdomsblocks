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

    public async void FromPointToPointAnimation(int needCount, ResourceType resourceType, Vector2 startWorldPos, Vector2 endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, float interval = 0.1f) {
        float currentCount = needCount;

        if (needCount > 30)
            needCount = 30;

        float addedCountToText = currentCount / needCount;
        Vector2 midPoint = (startWorldPos + endWorldPos) / 2;

        Vector2 direction = (endWorldPos - startWorldPos).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        float randomSign = Random.Range(0, 2) * 2 - 1;
        float randomFactor = Random.Range(0.5f, 1f) * 0.5f * randomSign;

        Vector2 controlPoint = midPoint + Vector2.up * 150 + perpendicular * randomFactor * (endWorldPos - startWorldPos).magnitude * 0.3f;

        Vector3[] path = { startWorldPos, controlPoint, endWorldPos };

        for (int i = 0; i < needCount; i++) {
            var uiElement = ShowFloatingImage();
            uiElement.sprite = SpritesManager.Instance.GetSprite(resourceType);
            uiElement.transform.position = startWorldPos;

            if (isRemoveResources) {
                startCount -= addedCountToText;
                float newCount = startCount;
                changeTextAction?.Invoke(resourceType, newCount);
            } else
                startCount += addedCountToText;

            float newStartCount = startCount;

            uiElement.rectTransform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.OutQuad).OnComplete(() => {
                ReleaseFloatingImage(uiElement);
                if (!isRemoveResources)
                    changeTextAction?.Invoke(resourceType, newStartCount);
            });

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }

    public async void FromSomePointsToPointAnimation(ResourceType resourceType, List<Vector2> startWorldPos, Vector2 endWorldPos,
        Action<ResourceType, float> changeTextAction, float startCount, bool isRemoveResources, float interval = 0.1f) {
        float currentCount = startWorldPos.Count;

        float addedCountToText = currentCount / startWorldPos.Count;

        for (int i = 0; i < startWorldPos.Count; i++) {
            Vector2 midPoint = (startWorldPos[0] + endWorldPos) / 2;

            Vector2 direction = (endWorldPos - startWorldPos[0]).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            float randomSign = Random.Range(0, 2) * 2 - 1;
            float randomFactor = Random.Range(0.5f, 1f) * 0.5f * randomSign;

            Vector2 controlPoint = midPoint + Vector2.up * 150 +
                                   perpendicular * randomFactor * (endWorldPos - startWorldPos[0]).magnitude * 0.3f;
            Vector3[] path = { startWorldPos[i], controlPoint, endWorldPos };

            var uiElement = ShowFloatingImage();
            uiElement.sprite = SpritesManager.Instance.GetSprite(resourceType);
            uiElement.transform.position = startWorldPos[i];

            if (isRemoveResources) {
                startCount -= addedCountToText;
                float newCount = startCount;
                changeTextAction?.Invoke(resourceType, newCount);
            } else
                startCount += addedCountToText;

            float newStartCount = startCount;

            uiElement.rectTransform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.OutQuad).OnComplete(() => {
                ReleaseFloatingImage(uiElement);
                if (!isRemoveResources)
                    changeTextAction?.Invoke(resourceType, newStartCount);
            });

            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }
}