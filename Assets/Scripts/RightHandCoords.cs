using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RightHandCoords : MonoBehaviour
{
    public Transform rightHand;          // Transform de la mano derecha

    public TextMeshProUGUI coordText;    // Campo de texto para mostrar las coords

    private Vector3 currentPos;          // Guarda la última posición leída

    void Update()
    {
        if (rightHand == null) return;   // Seguridad por si se te olvida asignarlo

        currentPos = rightHand.position;

        // 1) Mostrar en la consola (ventana Console)
        Debug.Log($"Right hand pos: {currentPos}");

        // 2) Mostrar en UI (si asignaste un TextMeshProUGUI)
        if (coordText != null)
        {
            coordText.text = $"Right Hand XYZ:\n" +
                             $"{currentPos.x:F3}, {currentPos.y:F3}, {currentPos.z:F3}";
        }
    }

    // 3) Mostrar con GUI.Label por si no usas UI Canvas
    void OnGUI()
    {
        // (Descomenta si quieres usarlo)
        // GUI.Label(new Rect(10, 10, 250, 40),
        //           $"Right Hand XYZ: {currentPos.x:F3}, {currentPos.y:F3}, {currentPos.z:F3}");
    }
}
