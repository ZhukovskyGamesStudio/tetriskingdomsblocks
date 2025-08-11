using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;

public static class UIAnimationsUtils
{
    public static async void FromPointToPointAnimation(int needCount, ResourceType resourceType,Vector2 startWorldPos,
        Vector2 endWorldPos, Action<ResourceType, float> changeTextAction,  float startCount, bool isRemoveResources, float interval = 0.1f)
    {
        float currentCount = needCount;
       
if(needCount > 30)
    needCount = 30;

float addedCountToText = currentCount / needCount;
        Debug.Log($"{needCount} count {startWorldPos} to {endWorldPos}");
        float angleStep = 70f / needCount;
        for (int i = 0; i < needCount; i++) {
            var uiElement = MetaUI.Instance.ShowFloatingImage();
            uiElement.sprite = SpritesManager.Instance.ResourcesSprites[resourceType];
            uiElement.transform.position = startWorldPos;
            float randomAngle =  (i - (float)needCount / 2)*angleStep * Mathf.Deg2Rad;
            //UnityEngine.Random.Range(-30f, 30f)
            float distance = Vector2.Distance(startWorldPos, endWorldPos) * 0.2f;

            Vector2 dir = (startWorldPos - endWorldPos).normalized;
            Vector2 deviatedDir = new Vector2(dir.x * Mathf.Cos(randomAngle) - dir.y * Mathf.Sin(randomAngle),
                dir.x * Mathf.Sin(randomAngle) + dir.y * Mathf.Cos(randomAngle));

            Vector2 midPoint = startWorldPos + deviatedDir * distance;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(uiElement.transform.DOMove(midPoint, 0.8f).SetEase(Ease.OutQuad));

            sequence.Append(uiElement.transform.DOMove(endWorldPos, 0.5f).SetEase(Ease.InQuad));

            if (isRemoveResources)
            {
              
                 startCount -= addedCountToText;
              
            }

            else
            {
                startCount += addedCountToText;  
                float newCount = startCount;
                                                                changeTextAction?.Invoke(resourceType, newCount);
            }
            
            sequence.OnComplete(() =>
            {
                float newStartCount = startCount;
                MetaUI.Instance.ReleaseFloatingImage(uiElement);
                if(isRemoveResources)
                changeTextAction?.Invoke(resourceType, newStartCount);
            });
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
        }
    }
}
