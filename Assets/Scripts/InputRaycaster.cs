using UnityEngine;

public class InputRaycaster {
    private readonly Camera _main;
    private readonly LayerMask _layerMask;
    private readonly LayerMask _additionalContainerMask;

    public InputRaycaster(Camera main, LayerMask layerMask,LayerMask additionalContainerMask) {
        _main = main;
        _layerMask = layerMask;
        _additionalContainerMask = additionalContainerMask;
    }

    public Vector3 InputPos() => Input.touchCount == 0 ? ScreenToWorldPoint(_layerMask) : TouchToWorldPoint(_layerMask);

    public Vector3 InputPosAdditionalContainer() => Input.touchCount == 0
        ? ScreenToWorldPoint(_additionalContainerMask)
        : TouchToWorldPoint(_additionalContainerMask);

    private Vector3 ScreenToWorldPoint(LayerMask needMask) =>
        Raycast(Input.mousePosition, needMask);

    private Vector3 TouchToWorldPoint(LayerMask needMask) =>
        Raycast(Input.GetTouch(0).position, needMask);

    private Vector3 Raycast(Vector2 pos, LayerMask needMask) => Physics.Raycast(
        _main.ScreenPointToRay(pos),
        out RaycastHit hit, Mathf.Infinity, needMask)
        ? hit.point
        : Vector3.zero;
}