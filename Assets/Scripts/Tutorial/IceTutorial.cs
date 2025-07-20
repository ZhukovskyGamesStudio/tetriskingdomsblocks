using System;
using UnityEngine;

public class IceTutorial : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransformIceMark;

    private void Start() {
       var mainCamera = Camera.main;
        for (int i = 0; i < GameFieldManager.Instance._field.GetLength(0); i++) {
            for (int j = 0; j < GameFieldManager.Instance._field.GetLength(1); j++) {
                if (GameFieldManager.Instance._field[i, j] == CellType.Ice) {
                    var iceHoleUI = Instantiate(rectTransformIceMark, GameUI.Instance.HolesForBgContainer);
                    iceHoleUI.transform.position = (Vector2)mainCamera.WorldToScreenPoint(new Vector3(i,0,j));
                }
            }
        }

       
    }
}
