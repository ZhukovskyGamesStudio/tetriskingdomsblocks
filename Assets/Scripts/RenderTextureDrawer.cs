using System;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureDrawer : MonoBehaviour {
    [SerializeField]
    private RawImage _rawImage;

    [SerializeField]
    private string _cameraName = "FieldHighlightsCamera";

    private void LateUpdate() {
        _rawImage.texture = RenderTextureCamera.RenderTexture[_cameraName];
    }

    private void Start() {
        if (_rawImage == null) {
            _rawImage = GetComponent<RawImage>();
        }

        if (!RenderTextureCamera.RenderTextureCount.TryAdd(_cameraName, 1)) {
            RenderTextureCamera.RenderTextureCount[_cameraName]++;
        }
    }

    private void OnDestroy() {
        RenderTextureCamera.RenderTextureCount[_cameraName]--;
    }
}