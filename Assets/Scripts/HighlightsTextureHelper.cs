using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HighlightsTextureHelper : MonoBehaviour {
    private RenderTexture _renderTexture;

    [SerializeField]
    private RectTransform _rect;

    [SerializeField]
    private RawImage _rawImage;

    [SerializeField]
    private string _cameraName = "FieldHighlightsCamera";

    private float _width, _height;

    private void UpdateTextureSize() {
        _width = _rect.rect.width;
        _height = _rect.rect.height;

        if (_renderTexture != null) {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }

        _renderTexture = new RenderTexture(Mathf.RoundToInt(_width), Mathf.RoundToInt(_height), 24, RenderTextureFormat.Default);
        _renderTexture.Create();
        _rawImage.texture = _renderTexture;
        var res = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        res.First(o => o.gameObject.name == _cameraName).targetTexture = _renderTexture;
    }

    private void Update() {
        if (!Mathf.Approximately(_width, _rect.rect.width) || !Mathf.Approximately(_height, _rect.rect.height)) {
            UpdateTextureSize();
        }
    }
}