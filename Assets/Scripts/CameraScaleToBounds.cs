using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraScaleToBounds : MonoBehaviour {
    [SerializeField] private CinemachineCamera _virtualCamera;
    [SerializeField] private Transform _targetArea; // The object or root that should always be visible
    [SerializeField] private float padding = 1.1f;  // Extra zoom out factor

    private void Update() {
        FitCameraToTarget();
    }

    public void FitCameraToTarget() {
        if (_virtualCamera == null || _targetArea == null) { return; }

        var bounds = CalculateBounds(_targetArea);
        var framing = _virtualCamera.GetComponent<CinemachinePositionComposer>();

        if (framing == null) { return; }

        float aspect = (float)Screen.width / Screen.height;
        float fovRad = _virtualCamera.Lens.FieldOfView * Mathf.Deg2Rad;

        float width = bounds.size.x;
        float height = bounds.size.y;

        // distance needed to fit vertically
        float vertDistance = (height / 2f) / Mathf.Tan(fovRad / 2f);

        // distance needed to fit horizontally
        float horizFovRad = 2f * Mathf.Atan(Mathf.Tan(fovRad / 2f) * aspect);
        float horizDistance = (width / 2f) / Mathf.Tan(horizFovRad / 2f);

        // pick the greater distance (whichever would otherwise crop)
        float targetDistance = Mathf.Max(vertDistance, horizDistance) * padding;

        framing.CameraDistance = targetDistance;
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
