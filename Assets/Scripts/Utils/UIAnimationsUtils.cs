using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks.Triggers;

public static class UIAnimationsUtils
{
   public static void FromPointToPointAnimation(Action onComplete, RectTransform uiElement, Vector2 startWorldPos, Vector2 endWorldPos)
    {
        //make this method with "for" for multiply elements and with images pool
        Canvas canvas = uiElement.GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Vector2 startAnchoredPos = WorldToCanvasPosition(canvas, startWorldPos);
        Vector2 endAnchoredPos = WorldToCanvasPosition(canvas, endWorldPos);
        
        uiElement.anchoredPosition = startAnchoredPos;
        
        float randomAngle = UnityEngine.Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        float distance = Vector2.Distance(startAnchoredPos, endAnchoredPos) * 0.3f;
        
        Vector2 dir = (startAnchoredPos - endAnchoredPos).normalized;
        Vector2 deviatedDir = new Vector2(
            dir.x * Mathf.Cos(randomAngle) - dir.y * Mathf.Sin(randomAngle),
            dir.x * Mathf.Sin(randomAngle) + dir.y * Mathf.Cos(randomAngle)
        );
        
        Vector2 midPoint = startAnchoredPos + deviatedDir * distance;
        
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(uiElement.DOAnchorPos(midPoint, 0.3f).SetEase(Ease.OutQuad));
        
        sequence.Append(uiElement.DOAnchorPos(endAnchoredPos, 0.4f).SetEase(Ease.InQuad));
        
        sequence.OnComplete(() => onComplete?.Invoke());
    }

    private static Vector2 WorldToCanvasPosition(Canvas canvas, Vector3 worldPosition)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Camera camera = /*canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null :*/ Camera.main;
        
        Vector2 viewportPosition = camera.WorldToViewportPoint(worldPosition);
        
        return new Vector2(
            (viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
            (viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
        );
    }
}
