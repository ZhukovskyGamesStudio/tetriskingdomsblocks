using UnityEngine;

public class InputRaycaster {
    private readonly Camera _main;
    private readonly LayerMask _layerMask;

    public InputRaycaster(Camera main, LayerMask layerMask) {
        _main = main;
        _layerMask = layerMask;
    }

    public Vector3 InputPos() => Input.touchCount == 0 ? ScreenToWorldPoint : TouchToWorldPoint;

    private Vector3 ScreenToWorldPoint => Raycast(Input.mousePosition);

    private Vector3 TouchToWorldPoint => Raycast(Input.GetTouch(0).position);

    private Vector3 Raycast(Vector2 pos) => Physics.Raycast(_main.ScreenPointToRay(pos), out RaycastHit hit, Mathf.Infinity, _layerMask)
        ? hit.point
        : Vector3.zero;
}