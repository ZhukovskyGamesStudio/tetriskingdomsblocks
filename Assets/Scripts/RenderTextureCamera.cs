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

    private void Awake() {
        _currentCamera = GetComponent<Camera>();
    }

    private void OnEnable() {
        UpdateTextureSize();
    }

    private void UpdateTextureSize() {
        _width = _rect.rect.width;
        _height = _rect.rect.height;

        if (_renderTexture != null) {
            _currentCamera.targetTexture = null;

            _renderTexture.Release();
            Destroy(_renderTexture);
        }

        if (_currentCamera != null) {
            _currentCamera.targetTexture = null;
        }

        _renderTexture = new RenderTexture(Mathf.RoundToInt(_width), Mathf.RoundToInt(_height), 24, RenderTextureFormat.Default);
        _renderTexture.Create();
        if (_rawImage != null) {
            _rawImage.texture = _renderTexture;
        }

        RenderTexture[_cameraName] = _renderTexture;

        _currentCamera.targetTexture = _renderTexture;
    }

    private void Update() {
        if (_currentCamera.targetTexture != RenderTexture[_cameraName]) {
            UpdateTextureSize();
            return;
        }

        if (!Mathf.Approximately(_width, _rect.rect.width) || !Mathf.Approximately(_height, _rect.rect.height)) {
            UpdateTextureSize();
        }
    }
}