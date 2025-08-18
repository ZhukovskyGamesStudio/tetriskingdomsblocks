using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureCamera : MonoBehaviour {
    private RenderTexture _renderTexture;
    public static Dictionary<string, RenderTexture> RenderTexture { get; private set; } = new Dictionary<string, RenderTexture>();

    [SerializeField]
    private RectTransform _rect;

    [SerializeField]
    private RawImage _rawImage;

    [SerializeField]
    private string _cameraName = "FieldHighlightsCamera";

    private Camera _currentCamera;
    private float _width, _height;

    public static readonly Dictionary<string, int> RenderTextureCount = new Dictionary<string, int>();

    private void UpdateTextureSize() {
        _width = _rect.rect.width;
        _height = _rect.rect.height;

        var cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var cam = cameras.First(o => o.gameObject.name == _cameraName);

        if (_renderTexture != null) {
            if (cam != null && cam.targetTexture == _renderTexture) {
                cam.targetTexture = null;
            }

            _renderTexture.Release();
            Destroy(_renderTexture);
        }

        if (cam != null) {
            if (_currentCamera != null) {
                _currentCamera.targetTexture = null;
            }

            _currentCamera = cam;

            _renderTexture = new RenderTexture(Mathf.RoundToInt(_width), Mathf.RoundToInt(_height), 24, RenderTextureFormat.Default);
            _renderTexture.Create();
            if (_rawImage != null) {
                _rawImage.texture = _renderTexture;
            }

            RenderTexture[_cameraName] = _renderTexture;

            _currentCamera.targetTexture = _renderTexture;
            var cameras2 = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(o => o.gameObject.name == _cameraName);
            foreach (var VARIABLE in cameras2) {
                VARIABLE.targetTexture = _renderTexture;
            }
        } else {
            _currentCamera = null;
            _width = 0;
            _height = 0;
        }
    }

    private void Update() {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Camera cam = cameras.First(o => o.gameObject.name == _cameraName);
        if (cam != _currentCamera) {
            UpdateTextureSize();
            return;
        }

        if (cam.targetTexture != RenderTexture[_cameraName]) {
            UpdateTextureSize();
            return;
        }

        if (!Mathf.Approximately(_width, _rect.rect.width) || !Mathf.Approximately(_height, _rect.rect.height)) {
            UpdateTextureSize();
        }
    }
}