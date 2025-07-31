using System;
using UnityEngine;
using UnityEngine.UI;

public class HighlightsTextureDrawer : MonoBehaviour {
    [SerializeField]
    private RawImage _rawImage;

    private void LateUpdate() {
        _rawImage.texture = HighlightsTextureHelper.RenderTexture;
    }
}