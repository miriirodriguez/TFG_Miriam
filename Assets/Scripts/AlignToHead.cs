using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignToHead : MonoBehaviour
{
    public Transform centerEyeAnchor; //crea un campo público para que arrastres manualmente el objeto de la cabeza (CenterEyeAnchor)
    
    void Update() //se ejecuta cada frame
    {
        if (centerEyeAnchor != null) //si el objeto de la cabeza no es nulo
        {
            transform.position = centerEyeAnchor.position; //asigna la posición del objeto a la posición de la cabeza
            Vector3 forward = centerEyeAnchor.forward;
            forward.y = 0; //evita la inclinación
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }

}
