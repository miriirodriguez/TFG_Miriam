using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input;

public class MirrorHandModifier4 : DataModifier<HandDataAsset>
{
    [SerializeField] private Hand      _leftHand;
    [SerializeField] private Transform _mirrorRoot;      // p.ej. TrackingSpace
    [SerializeField] private bool      _reportAsRight = true;

    protected override void Apply(HandDataAsset data)
    {
        // 1) Copia previa (DataModifier ya hace data.CopyFrom)

        // 2) Refleja la Root Pose
        MirrorPose(ref data.Root);

        // 3) Refleja PointerPose si lo usas para raycasts
        MirrorPose(ref data.PointerPose);

        // 4) Refleja todas las articulaciones localmente
        for (int i = 0; i < data.Joints.Length; i++)
        {
            Quaternion q = data.Joints[i];
            // Aquí suele funcionar bien con la negación simple
            data.Joints[i] = new Quaternion(-q.x, q.y, -q.z, q.w).normalized;
        }

        // 5) Opcional: marcar como mano derecha
        if (_reportAsRight)
            data.Config.Handedness = Handedness.Right;

        // 6) Origen sintético
        data.RootPoseOrigin    = PoseOrigin.SyntheticPose;
        data.PointerPoseOrigin = PoseOrigin.SyntheticPose;
    }

    private void MirrorPose(ref Pose pose)
    {
        // plano de espejo: definido por mirrorRoot
        Vector3 origin = _mirrorRoot ? _mirrorRoot.position : Vector3.zero;
        Vector3 normal = _mirrorRoot ? _mirrorRoot.right    : Vector3.right;

        // *** Posición ***
        Vector3 rel = pose.position - origin;
        Vector3 refl = rel - 2f * Vector3.Dot(rel, normal) * normal;
        pose.position = origin + refl;

        // *** Rotación reconstruida ***
        // definimos dos ejes de la pose original
        Vector3 forward = pose.rotation * Vector3.forward; // hacia el dedo índice
        Vector3 up      = pose.rotation * Vector3.up;      // hacia los nudillos

        // los reflejamos
        forward.x = Vector3.Dot(forward, normal) * -2f * normal.x + forward.x;
        forward   = Vector3.Reflect(forward, normal);
        up.x      = Vector3.Dot(up, normal) * -2f * normal.x + up.x;
        up        = Vector3.Reflect(up, normal);

        // y reconstruimos un quaternion válido
        pose.rotation = Quaternion.LookRotation(forward, up);
    }
}
