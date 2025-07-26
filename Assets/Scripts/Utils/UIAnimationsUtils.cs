using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks.Triggers;

public static class UIAnimationsUtils
{
    public static void FromPointToPointAnimation(int needCount, ResourceType resourceType,Vector2 startWorldPos,
        Vector2 endWorldPos) {

        Debug.Log($"{needCount} count {startWorldPos} to {endWorldPos}");
        float angleStep = 70f / needCount;
        for (int i = 0; i < needCount; i++) {
            var uiElement = MetaUI.Instance.ShowFloatingText();
            uiElement.text = $"<sprite name={resourceType}>";
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

            sequence.OnComplete(() => MetaUI.Instance.ReleaseFloatingText(uiElement));
        }
    }
}
