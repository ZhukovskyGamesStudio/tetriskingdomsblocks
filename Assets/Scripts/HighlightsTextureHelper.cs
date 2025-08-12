using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HighlightsTextureHelper : MonoBehaviour {
    private RenderTexture _renderTexture;
    public static RenderTexture RenderTexture { get; private set; }

    [SerializeField]
    private RectTransform _rect;

    [SerializeField]
    private RawImage _rawImage;

    [SerializeField]
    private string _cameraName = "FieldHighlightsCamera";
    private Camera _currentCamera;
    private float _width, _height;

    private void UpdateTextureSize() {
        _width = _rect.rect.width;
        _height = _rect.rect.height;

        if (_renderTexture != null) {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }

      
        var cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var cam = cameras.First(o => o.gameObject.name == _cameraName);
        if (cam != null) {
            _currentCamera = cam;
            _renderTexture = new RenderTexture(Mathf.RoundToInt(_width), Mathf.RoundToInt(_height), 24, RenderTextureFormat.Default);
            _renderTexture.Create();
            _rawImage.texture = _renderTexture;
            RenderTexture = _renderTexture;
            
            cam.targetTexture = _renderTexture;
        } else {
            _currentCamera = null;
            _width = 0;
            _height = 0;
        }
    }

    private void Update() {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Camera cam = cameras.First(o => o.gameObject.name == _cameraName);
       
        if (!Mathf.Approximately(_width, _rect.rect.width) || !Mathf.Approximately(_height, _rect.rect.height) || cam != _currentCamera) {
            UpdateTextureSize();
        }
    }
}