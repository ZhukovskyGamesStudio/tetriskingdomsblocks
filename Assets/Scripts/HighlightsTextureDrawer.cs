using System;
using UnityEngine;
using UnityEngine.UI;

public class HighlightsTextureDrawer : MonoBehaviour {
    [SerializeField]
    private RawImage _rawImage;

    [SerializeField]
    private string _cameraName = "FieldHighlightsCamera";

    private void LateUpdate() {
        _rawImage.texture = HighlightsTextureHelper.RenderTexture[_cameraName];
    }
}