using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorHand1 : MonoBehaviour
{
    [Header("Referencia al skeleton de cada mano")]
    public OVRSkeleton leftSkeleton;
    public OVRSkeleton rightSkeleton;

    // Se asume que ambos skeletons tienen los mismos huesos en el mismo orden
    void LateUpdate()
    {
        var leftBones  = leftSkeleton.Bones;
        var rightBones = rightSkeleton.Bones;
        int count = Mathf.Min(leftBones.Count, rightBones.Count);

        for(int i = 0; i < count; i++)
        {
            // quaternion que viene de la mano izquierda
            Quaternion L = leftBones[i].Transform.localRotation;
            // reflect X axis & W for mirror
            Quaternion mirrored = new Quaternion(-L.x, L.y, L.z, -L.w);
            rightBones[i].Transform.localRotation = mirrored;
        }
    }
}
