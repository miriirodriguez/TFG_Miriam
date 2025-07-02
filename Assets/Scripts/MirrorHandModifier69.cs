using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input;


public class MirrorHandModifier69 : DataModifier<HandDataAsset>
{
    [SerializeField] private Hand _leftHandDataSource;          
    [SerializeField] private bool _reportAsRight = true;

    protected override void Apply(HandDataAsset data)
    {
        // 1) Raíz (muñeca)
        var p = data.Root.position;
        p.x = -p.x;
        var r = data.Root.rotation;
        // espejo en X = invertir Y y Z del quaternion
        r = new Quaternion(r.x, -r.y, -r.z, r.w);
        data.Root = new Pose(p, r);

        // 2) Articulaciones
        for (int i = 0; i < data.Joints.Length; i++)
        {
            var q = data.Joints[i];
            data.Joints[i] = new Quaternion(q.x, -q.y, -q.z, q.w);
        }

        // 3) Reportar como mano derecha
        if (_reportAsRight)
            data.Config.Handedness = Handedness.Right;
    }
}
