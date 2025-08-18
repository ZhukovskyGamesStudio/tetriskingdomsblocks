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
        RenderTextureCamera.RenderTextureCount[_cameraName]++;
    }

    private void OnDestroy() {
        RenderTextureCamera.RenderTextureCount[_cameraName]--;
    }
}