using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input;         
using Oculus.Interaction;

[RequireComponent(typeof(OVRSkeleton))]
public class Espejo14mayo : MonoBehaviour
{
    [SerializeField] private OVRSkeleton _sourceSkeleton; // mano izquierda
    private OVRSkeleton _targetSkeleton;                  // mano derecha

    void Awake() => _targetSkeleton = GetComponent<OVRSkeleton>();

    void LateUpdate()
    {
        if (!_sourceSkeleton.IsDataValid || !_targetSkeleton.IsDataValid) return;

        var src = _sourceSkeleton.Bones;
        var dst = _targetSkeleton.Bones;
        int n   = Mathf.Min(src.Count, dst.Count);

        for (int i = 0; i < n; i++)
        {
            Pose p       = new Pose(src[i].Transform.position, src[i].Transform.rotation);
            Pose mirrored = HandMirroring.Mirror(p);      // ← ya no necesitas “new”

            dst[i].Transform.SetPositionAndRotation(mirrored.position, mirrored.rotation);
        }
    }
}
