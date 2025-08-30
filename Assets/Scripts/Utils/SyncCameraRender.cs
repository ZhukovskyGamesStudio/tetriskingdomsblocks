using UnityEngine;
using UnityEngine.UI;

public class SyncCameraRender : MonoBehaviour {
    [SerializeField]
    private RawImage _image;

    [SerializeField]
    private Camera _renderCamera;

    private RenderTexture _rt;

    private void OnEnable() {
        SyncSize();
    }

    private void OnDisable() {
        if (_rt != null) {
            _renderCamera.targetTexture = null;
            _image.texture = null;
            _rt.Release();
            Destroy(_rt);
            _rt = null;
        }
    }

    private void SyncSize() {
        int width = Screen.width;
        int height = Screen.height;

        if (_rt != null && (_rt.width != width || _rt.height != height)) {
            _renderCamera.targetTexture = null;
            _image.texture = null;
            _rt.Release();
            Destroy(_rt);
            _rt = null;
        }

        if (_rt == null) {
            _rt = new RenderTexture(width, height, 24);
            _rt.Create();
        }

        _renderCamera.targetTexture = _rt;
        _image.texture = _rt;
        _renderCamera.orthographicSize = width;
    }
}