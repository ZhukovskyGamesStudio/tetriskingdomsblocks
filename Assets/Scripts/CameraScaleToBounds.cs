using System;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class CameraScaleToBounds : MonoBehaviour {
    [SerializeField]
    private CinemachineCamera _virtualCamera;
    
    private CinemachinePositionComposer _framingComposer; // Optional, if you want to use a composer for camera framing
    
    [SerializeField]
    private Transform _targetArea; // The object or root that should always be visible

    [SerializeField]
    private float padding = 1.1f; // Extra zoom out factor

    private float _aspectRatio = -1;

    private void Awake() {
        _framingComposer= _virtualCamera.GetComponent<CinemachinePositionComposer>();
    }

    private void Update() {
        float curAspect = (float)Screen.width / Screen.height;
        if (!Mathf.Approximately(curAspect, _aspectRatio)) {
            _aspectRatio = curAspect;
            FitCameraToTarget(_aspectRatio);
        }
    }

    private void FitCameraToTarget(float aspectRatio) {
        if (_virtualCamera == null || _targetArea == null) {
            return;
        }

        var bounds = CalculateBounds(_targetArea);
        float fovRad = _virtualCamera.Lens.FieldOfView * Mathf.Deg2Rad;

        float width = bounds.size.x;
        float height = bounds.size.z;

        // distance needed to fit vertically
        float vertDistance = (height / 2f) / Mathf.Tan(fovRad / 2f);

        // distance needed to fit horizontally
        float horizFovRad = 2f * Mathf.Atan(Mathf.Tan(fovRad / 2f) * aspectRatio);
        float horizDistance = (width / 2f) / Mathf.Tan(horizFovRad / 2f);

        // pick the greater distance (whichever would otherwise crop)
        float targetDistance = Mathf.Max(vertDistance, horizDistance) * padding;
        _framingComposer.enabled = true;
        _framingComposer.CameraDistance = targetDistance;
        DisableComposer().Forget();
    }

    private async UniTask DisableComposer() {
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        if (_framingComposer) {
            _framingComposer.enabled = false;
        }
    }

    private Bounds CalculateBounds(Transform root) {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) {
            return new Bounds(root.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
}