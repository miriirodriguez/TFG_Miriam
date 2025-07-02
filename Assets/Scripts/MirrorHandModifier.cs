using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input; 
             
public class MirrorHandModifier : DataModifier<HandDataAsset>
{
    [SerializeField] private Hand _leftHand;          
    [SerializeField] private bool _reportAsRight = true;

    protected override void Apply(HandDataAsset data)
    {
        // 1) Raíz (muñeca)
        Vector3 pos = data.Root.position;
        pos.x = -pos.x;
        Quaternion rot = data.Root.rotation;
        rot = new Quaternion(rot.x, rot.y, rot.z, rot.w);
        data.Root = new Pose(pos, rot);

        // 2) Articulaciones
        for (int i = 0; i < data.Joints.Length; i++)
        {
            Quaternion q = data.Joints[i];
            data.Joints[i] = new Quaternion(-q.x, q.y, -q.z, q.w);
        }

        // 3) Reportar como mano derecha con Id único
        if (_reportAsRight)
        {
            data.Config.Handedness = Handedness.Right;
        }
    }
}

