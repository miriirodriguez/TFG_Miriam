using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;                //  Grabbable
using Oculus.Interaction.HandGrab;       //  HandGrabInteractable

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]

public class CubeHandGrabSetup : MonoBehaviour
{
    /* Se llama al añadir el componente */
    private void Reset() => Configure();

#if UNITY_EDITOR
    /* Se llama cada vez que cambias algo en el Inspector */
    private void OnValidate() => Configure();
#endif

    private void Configure()
    {
        /* Rigidbody ------------------------------------------------------------------- */
        var rb = GetComponent<Rigidbody>();
        rb.mass = 1f;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        /* Collider -------------------------------------------------------------------- */
        var col = GetComponent<BoxCollider>();
        col.isTrigger = false;

        /* Interaction components ------------------------------------------------------ */
        if (!TryGetComponent(out Grabbable grabbable))
            grabbable = gameObject.AddComponent<Grabbable>();

        if (!TryGetComponent(out HandGrabInteractable hgi))
            hgi = gameObject.AddComponent<HandGrabInteractable>();

#if UNITY_EDITOR
        /*  En la mayoría de versiones no hace falta, pero por si acaso:
            solo tocamos el SerializedObject si encontramos las propiedades. */
        var so = new UnityEditor.SerializedObject(hgi);

        var rbProp  = so.FindProperty("_rigidbody");
        if (rbProp != null) rbProp.objectReferenceValue = rb;

        var grProp  = so.FindProperty("_grabbable");
        if (grProp != null) grProp.objectReferenceValue = grabbable;

        so.ApplyModifiedPropertiesWithoutUndo();
#endif

        /* Pose por defecto ------------------------------------------------------------ */
        if (hgi.HandGrabPoses.Count == 0)
        {
            var poseGO = new GameObject("AutoPose");
            poseGO.transform.SetParent(transform, false);
            var pose = poseGO.AddComponent<HandGrabPose>();
            hgi.HandGrabPoses.Add(pose);
        }
    }
}