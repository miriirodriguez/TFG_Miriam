using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input;

public class MirrorHandModifier2 : DataModifier<HandDataAsset>
{
    [SerializeField] private Hand      _leftHand;
    [SerializeField] private Transform _mirrorRoot;      // normalmente TrackingSpace o CameraRig
    [SerializeField] private bool      _reportAsRight = true;

    protected override void Apply(HandDataAsset data)
    {
        /* El asset ya es una copia de la izquierda ----------------------- */

        // 1) reflejamos la raíz y el pointer pose
        MirrorPose(ref data.Root);
        MirrorPose(ref data.PointerPose);

        // 2) reflejamos todos los quaterniones de las articulaciones
        for (int i = 0; i < data.Joints.Length; i++)
        {
            Quaternion q = data.Joints[i];
            data.Joints[i] = new Quaternion(-q.x, q.y, -q.z, q.w).normalized;
        }

        // 3) opcional: marcarla como mano derecha
        if (_reportAsRight)
        {
            data.Config.Handedness = Handedness.Right;
        }

        // 4) (recomendado) indicar que la pose es sintética
        data.RootPoseOrigin    = PoseOrigin.SyntheticPose;
        data.PointerPoseOrigin = PoseOrigin.SyntheticPose;
    }

    /* ---------- Helper que refleja una Pose en el plano X=0
       pero expresado en el espacio local de _mirrorRoot ---------- */
    private void MirrorPose(ref Pose pose)
    {
        Transform root = _mirrorRoot ? _mirrorRoot : null;

        // origen del plano: posición del TrackingSpace / CameraRig
        Vector3 origin = root ? root.position : Vector3.zero;

        // normal del plano: +X local del TrackingSpace
        Vector3 n = root ? root.right : Vector3.right;    // ya normalizado

        /* --- posición --- */
        Vector3 rel = pose.position - origin;             // a espacio local del plano
        Vector3 refl = rel - 2f * Vector3.Dot(rel, n) * n;/* reflexión p' = p - 2(p·n)n */
        pose.position = origin + refl;

        /* --- rotación --- */
        Quaternion r = pose.rotation;
        // reflejo de quaternion respecto al plano con normal en +X:
        pose.rotation = new Quaternion(-r.x, r.y, -r.z, r.w);
    }
}
