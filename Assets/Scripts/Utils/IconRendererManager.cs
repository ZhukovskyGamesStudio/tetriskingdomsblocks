using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering;

public class IconRendererManager : MonoBehaviour {
    [Header("Settings")]
    [SerializeField]
    private int _textureSize = 128;

    [SerializeField]
    private LayerMask _renderLayer;

    [SerializeField]
    private float _renderDelay = 0.1f;

    [SerializeField]
    private Vector3 _spawnRotation = new Vector3(-90, -90, 90);

    [Header("References")]
    [SerializeField]
    private Camera _renderCamera;

    [SerializeField]
    private Light _renderLight;

    [SerializeField]
    private Material _unlitMaterial;

    public static IconRendererManager Instance;

    [SerializeField]
    private RenderTexture _renderTexture;

    private readonly Dictionary<string, Texture2D> _iconCache = new Dictionary<string, Texture2D>();
    private bool _isRendering;
    private float _lastRenderTime;

    private TextureFormat format;

    private void Awake() {
        Instance = this;
    }

    public void InitializeRenderSystem() {
        // Настраиваем камеру
        _renderCamera.orthographic = true;
        _renderCamera.orthographicSize = 1;
        _renderCamera.cullingMask = _renderLayer;
        _renderCamera.clearFlags = CameraClearFlags.SolidColor;
        _renderCamera.backgroundColor = new Color(0, 0, 0, 0);
        _renderCamera.targetTexture = _renderTexture;
        _renderCamera.enabled = false;

        // Настраиваем освещение
        _renderLight.type = LightType.Directional;
        _renderLight.cullingMask = _renderLayer;
        _renderLight.intensity = 1f;

        format = TextureFormat.RGBA32;
    }

    public async UniTask<Sprite> GetIconAsSprite(GameObject prefab) {
        var texture = await GetIcon(prefab);

        if (texture == null) {
            return null;
        }

        // Создаем спрайт из текстуры
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), // Pivot по центру
            100, // Pixels per unit
            0, SpriteMeshType.Tight);

        return sprite;
    }

    private async UniTask<Texture2D> GetIcon(GameObject prefab) {
        if (prefab == null) {
            return null;
        }

        /* string itemId = prefab.name;

         // Проверяем кэш
         if (_iconCache.TryGetValue(itemId, out Texture2D cachedIcon))
         {
             callback?.Invoke(cachedIcon);
             return;
         }*/

        return await RenderIconAsync(prefab);
    }

    private void SetLayerRecursively(GameObject obj, int layer) {
        if (obj == null) {
            return;
        }

        obj.layer = layer;

        foreach (Transform child in obj.transform) {
            if (child != null) {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }

    private async UniTask<Texture2D> RenderIconAsync(GameObject prefab) {
        while (Time.time - _lastRenderTime < _renderDelay) {
            await UniTask.WaitForEndOfFrame();
        }

        while (_isRendering) {
            await UniTask.WaitForEndOfFrame();
        }

        _isRendering = true;
        _lastRenderTime = Time.time;

        SetLayerRecursively(prefab, (int)Mathf.Log(_renderLayer.value, 2));

        //SimplifyObjectMaterials(prefab);

        PositionObjectForRendering(prefab);

        var res = await RenderIconTexture(prefab, prefab.name);
        Destroy(prefab.gameObject);

        _isRendering = false;
        return res;
    }

    private void SimplifyObjectMaterials(GameObject obj) {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            if (renderer.material != null) {
                var mat = _unlitMaterial;
                if (renderer.material.mainTexture != null)
                    mat.mainTexture = renderer.material.mainTexture;
                renderer.material = mat;
            }
        }
    }

    private void PositionObjectForRendering(GameObject obj) {
        obj.transform.localRotation = Quaternion.Euler(_spawnRotation);
        Bounds bounds = CalculateObjectBounds(obj);
        float maxExtent = bounds.extents.magnitude;
        Vector3 center = bounds.center;

        obj.transform.position = _renderCamera.transform.position + _renderCamera.transform.forward * (maxExtent + 0.5f);
        _renderCamera.orthographicSize = maxExtent * 1.2f;
    }

    private Bounds CalculateObjectBounds(GameObject obj) {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) {
            return new Bounds(obj.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) {
            bounds.Encapsulate(r.bounds);
        }

        return bounds;
    }

    private async UniTask<Texture2D> RenderIconTexture(GameObject target, string itemId) {
        _renderCamera.Render();

        var tmp = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        Texture2D icon = new Texture2D(_renderTexture.width, _renderTexture.height, format, false);
        icon.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
        icon.Apply();
        RenderTexture.active = tmp;

        return icon;
    }

    public void ClearCache() {
        foreach (var texture in _iconCache.Values)
            Destroy(texture);
        _iconCache.Clear();
    }

    private void OnDestroy() {
        if (_renderTexture != null)
            _renderTexture.Release();

        ClearCache();
    }
}