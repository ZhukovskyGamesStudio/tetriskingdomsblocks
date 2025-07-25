using UnityEngine;
using UnityEngine.Pool;

public class MetaWorldCanvasView : MonoBehaviour {
    public static MetaWorldCanvasView Instance;

    [field: SerializeField]
    public UnlockFieldCellsView UnlockFieldCellsView { get; private set; }

    [SerializeField]
    private Transform _resourcesMarksContainer;

    [SerializeField]
    private ResourceMarkView _resourceMarkViewPrefab;

    [SerializeField]
    private Vector3 _worldCanvasRotation = new(-45, -90, 90);

    private ObjectPool<ResourceMarkView> _resourcesMarksPool;

    private void Awake() {
        Instance = this;
        InitMarksPool();
    }

    private void InitMarksPool() {
        _resourcesMarksPool = new ObjectPool<ResourceMarkView>(() => Instantiate(_resourceMarkViewPrefab, _resourcesMarksContainer));
    }

    public ResourceMarkView SpawnResourceMark(Vector3 pos, int maxResource, float currentResource, ResourceType resourceType, Color resourceColor,
        int index) {
        var mark = _resourcesMarksPool.Get();
        mark.gameObject.SetActive(true);
        //pos = _mainCamera.WorldToScreenPoint(pos);
        mark.transform.position = new Vector3(pos.x, pos.y + 1, pos.z);
        mark.transform.localRotation = Quaternion.Euler(_worldCanvasRotation);
        mark.SetColor(resourceColor);
        mark.SetResourceMarkInfo(maxResource, currentResource, resourceType, index);
        return mark;
    }

    public void ReleaseResourceMark(ResourceMarkView mark) {
        //mark.gameObject.SetActive(false);
        _resourcesMarksPool.Release(mark);
    }
}