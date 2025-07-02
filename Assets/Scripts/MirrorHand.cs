using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorHand : MonoBehaviour
{
    public Transform rightHandReal;
    public Transform leftHandVirtual;
    public Transform mirrorPlane; 

    void Update()
    {
        if (rightHandReal != null && leftHandVirtual != null && mirrorPlane != null)
        {
            // Obtener posición relativa al plano
            Vector3 localRight = mirrorPlane.InverseTransformPoint(rightHandReal.position);

            // Reflejar en el eje X (simetría en torno al plano vertical)
            localRight.x *= -1;

            // Aplicar posición reflejada
            leftHandVirtual.position = mirrorPlane.TransformPoint(localRight);

            // Reflejar rotación (en espejo)
            Quaternion localRot = Quaternion.Inverse(mirrorPlane.rotation) * rightHandReal.rotation;
            Vector3 mirroredEuler = localRot.eulerAngles;
            mirroredEuler.y = -mirroredEuler.y;
            mirroredEuler.z = -mirroredEuler.z;

            leftHandVirtual.rotation = mirrorPlane.rotation * Quaternion.Euler(mirroredEuler);
        }
    }
}
