using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input;  

public class MirrorHandModifier6 : DataModifier<HandDataAsset>
{
    [SerializeField] private Hand _leftHand;      // mano real izquierda
    [SerializeField] private Transform _rightAnchor;   // punto donde quieres que aparezca

    protected override void Apply(HandDataAsset dst)
    {
        // 0) copiar todo el asset
        dst.CopyFrom(_leftHand.GetData());

        // 1) raíz: reflejar en plano X=0 y llevarla al RightAnchor
        Pose srcRoot = dst.Root;
        Vector3 p = srcRoot.position;
        p.x = -p.x;                       // espejo en X
        p += _rightAnchor.position - _leftHand.transform.position; // offset al otro lado

        Quaternion r = srcRoot.rotation;
        r = new Quaternion(-r.x, r.y, -r.z, r.w);      // invertir rot

        dst.Root = new Pose(p, r);

        // 2) reflejar cada articulación
        for (int i = 0; i < dst.Joints.Length; i++)
        {
            Quaternion q = dst.Joints[i];
            dst.Joints[i] = new Quaternion(-q.x, q.y, -q.z, q.w);
        }
    }
}
