using UnityEngine;

public class TutorialObjectHidedAfterTap : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    void Start()
    {
        // Создаем Canvas
        Transform canvasObj = GameUI.Instance.BlackBgContainer;
        
        gameObject.transform.SetParent(canvasObj.transform);
        
        // Настраиваем RectTransform
        _rectTransform.anchorMin = Vector2.zero;
        _rectTransform.anchorMax = Vector2.one;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
        gameObject.transform.localScale = Vector3.one;
    }
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
                Destroy(gameObject);
        }
        
        // Также оставляем поддержку мыши для тестирования в редакторе
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            Destroy(gameObject);
#endif
    }
}
