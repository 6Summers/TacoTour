using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject square1;
    [SerializeField] private  GameObject square2;
    [SerializeField] private int backGroundOffset = 20;

    bool IsBackgroundOutsideCamera(GameObject background)
    {
        // obtenemos  coordenadas  de la cámara
        Vector3 camPos = playerCamera.WorldToViewportPoint(background.transform.position/2);

        // verificamos si el fondo está fuera del perímetro de la cámara
        return camPos.x < 0 || camPos.x > 1 || camPos.y < 0 || camPos.y > 1;
    }

    void MoveBackground(GameObject backgroundToMove, GameObject referenceBackground)
    {
        // calculamos la nueva posición para el fondo que se moverá
        Vector2 newPosition = referenceBackground.transform.position + new Vector3(backGroundOffset, 0, 0);

        // movemosel fondo a la nueva posición
        backgroundToMove.transform.position = newPosition;
    }

    void Update()
    {
        // verificamos si los fondos están fuera del perímetro de la cámara
        if (IsBackgroundOutsideCamera(square1))
        {
            MoveBackground(square1, square2);
        }
        if (IsBackgroundOutsideCamera(square2))
        {
            MoveBackground(square2, square1);
        }
    }
}