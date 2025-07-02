using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMirrorFinal : MonoBehaviour
{
    [Header("Variables")]
    public OVRHand leftHand; //mano izquierda 
    public OVRHand rightHand; //mano derecha 
    public Transform CameraRig; //Camera Rig

    [Header("Configuración de interacción")]
    //valores por defecto
    public float pinchThreshold = 0.7f; // Umbral para detectar pinza (0-1)
    public float grabDistance = 0.07f; // Distancia máxima para agarrar objetos
    public LayerMask grabbableLayer = -1; // Capa de objetos que se pueden agarrar

    private Transform leftT;
    private Transform rightT;
    
    private Vector3 mirrorNormal; 

    // Diccionarios para mapear los huesos de los dedos.
    //Almacenamos las referencias de los huesos de las manos izquierda y derecha
    private Dictionary<OVRSkeleton.BoneId, Transform> leftBones;
    private Dictionary<OVRSkeleton.BoneId, Transform> rightBones;

    // Referencias a los skeletons
    private OVRSkeleton leftSkeleton;
    private OVRSkeleton rightSkeleton;

    // Variables para interacción con pinza
    private bool isLeftPinching = false;
    private bool isRightPinching = false;
    private GameObject leftGrabbedObject = null;
    private GameObject rightGrabbedObject = null;
    private Vector3 leftGrabOffset; //Offset de posición al agarrar
    private Vector3 rightGrabOffset;
    private Quaternion leftGrabRotationOffset; //Offset de rotación al agarrar
    private Quaternion rightGrabRotationOffset;
    
    // Posiciones de los dedos para calcular pinza
    private Transform leftThumbTip;
    private Transform leftIndexTip;
    private Transform rightThumbTip;
    private Transform rightIndexTip;
    
    void Awake() //almacenamos la referencia de los transform de las manos
    {
        leftT  = leftHand.transform;   // almaceno la referencia una sola vez
        rightT = rightHand.transform;
        mirrorNormal = CameraRig.right.normalized; //vector normal para el espejo, asumiendo que el espejo está en el plano YZ
        
        // Obtenemos los skeletons
        leftSkeleton = leftHand.GetComponent<OVRSkeleton>();
        rightSkeleton = rightHand.GetComponent<OVRSkeleton>();
        
        // Inicializamos los diccionarios
        leftBones = new Dictionary<OVRSkeleton.BoneId, Transform>();
        rightBones = new Dictionary<OVRSkeleton.BoneId, Transform>();

    }

    void Start()
    {
        // Esperamos a que los skeletons estén inicializados
        StartCoroutine(InitializeBones());
    }

    IEnumerator InitializeBones()
    {
        // Esperamos hasta que los skeletons estén listos
        while (!leftSkeleton.IsInitialized || !rightSkeleton.IsInitialized)
        {
            yield return null;
        }

        // Poblamos los diccionarios con los huesos
        foreach (var bone in leftSkeleton.Bones)
        {
            leftBones[bone.Id] = bone.Transform;
        }

        foreach (var bone in rightSkeleton.Bones)
        {
            rightBones[bone.Id] = bone.Transform;
        }
        
        // Obtenemos las referencias a las puntas de los dedos
        if (leftBones.ContainsKey(OVRSkeleton.BoneId.Hand_ThumbTip))
            leftThumbTip = leftBones[OVRSkeleton.BoneId.Hand_ThumbTip];
        if (leftBones.ContainsKey(OVRSkeleton.BoneId.Hand_IndexTip))
            leftIndexTip = leftBones[OVRSkeleton.BoneId.Hand_IndexTip];
        if (rightBones.ContainsKey(OVRSkeleton.BoneId.Hand_ThumbTip))
            rightThumbTip = rightBones[OVRSkeleton.BoneId.Hand_ThumbTip];
        if (rightBones.ContainsKey(OVRSkeleton.BoneId.Hand_IndexTip))
            rightIndexTip = rightBones[OVRSkeleton.BoneId.Hand_IndexTip];
    }

    Vector3 ReflectVector(Vector3 Vector)
    {    
        return Vector3.Reflect(Vector, mirrorNormal); //Reflejamos un vector respecto a un plano definido por mirrorNormal
    }

    public void Mirror(Transform transfOrigen, Transform transfDestino)
    {
        // 1) Determinamos la posición de la mano destino (derecha)
        Vector3 vectorJugadorManoOrigen = transfOrigen.position - CameraRig.position;
        Vector3 vectorJugadorManoDestino = ReflectVector(vectorJugadorManoOrigen);
        transfDestino.position = CameraRig.position + vectorJugadorManoDestino;
        
        // 2) Se determina la rotación de la mano destino (derecha)
        Vector3 forwardVec = ReflectVector(transfOrigen.forward);
        Vector3 upVec = ReflectVector(transfOrigen.up);
        transfDestino.rotation = Quaternion.LookRotation(-forwardVec, -upVec);
    }


    float GetPinchStrength(OVRHand hand)
    {
        // Usamos la función built-in de OVRHand para obtener la fuerza de pinza
        return hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        //GetFingerPinchStrength devuelve un valor entre 0 y 1, donde 0 es sin pinza y 1 es pinza máxima
    }

    Vector3 GetPinchPosition(Transform thumbTip, Transform indexTip)
    {
        // Calculamos el punto medio entre el pulgar y el índice
        if (thumbTip != null && indexTip != null)
        {
            return (thumbTip.position + indexTip.position) / 2f;
        }
        return Vector3.zero;
    }

    //LÓGICA DE AGARRE

    void HandlePinchInteraction()
    {
        // Manejo de pinza para mano izquierda
        HandleHandPinch(leftHand, leftThumbTip, leftIndexTip, ref isLeftPinching, ref leftGrabbedObject, ref leftGrabOffset, ref leftGrabRotationOffset);
        
        // Manejo de pinza para mano derecha
        HandleHandPinch(rightHand, rightThumbTip, rightIndexTip, ref isRightPinching, ref rightGrabbedObject, ref rightGrabOffset, ref rightGrabRotationOffset);
    }

    void HandleHandPinch(OVRHand hand, Transform thumbTip, Transform indexTip, ref bool isPinching, ref GameObject grabbedObject, ref Vector3 grabOffset, ref Quaternion grabRotationOffset)
    {
        float pinchStrength = GetPinchStrength(hand);
        Vector3 pinchPosition = GetPinchPosition(thumbTip, indexTip);
        
        // Detectamos si empezamos a hacer pinza
        if (!isPinching && pinchStrength > pinchThreshold)
        {
            // Buscamos objetos cerca de la posición de pinza
            Collider[] nearbyObjects = Physics.OverlapSphere(pinchPosition, grabDistance, grabbableLayer);
            
            if (nearbyObjects.Length > 0)
            {
                // Tomamos el objeto más cercano
                GameObject closestObject = null;
                float closestDistance = float.MaxValue;
                
                foreach (var collider in nearbyObjects)
                {
                    float distance = Vector3.Distance(pinchPosition, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestObject = collider.gameObject;
                    }
                }
                
                if (closestObject != null)
                {
                    // Iniciamos el agarre
                    isPinching = true;
                    grabbedObject = closestObject;
                    grabOffset = grabbedObject.transform.position - pinchPosition;
                    grabRotationOffset = Quaternion.Inverse(hand.transform.rotation) * grabbedObject.transform.rotation;
                    
                    // Deshabilitamos la física del objeto mientras lo agarramos
                    Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                }
            }
        }
        // Si ya estamos haciendo pinza y soltamos
        else if (isPinching && pinchStrength < pinchThreshold)
        {
            if (grabbedObject != null)
            {
                // Reactivamos la física del objeto
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    // Opcional: añadir velocidad al objeto al soltarlo
                    rb.velocity = hand.transform.GetComponent<Rigidbody>()?.velocity ?? Vector3.zero;
                }
                
                grabbedObject = null;
            }
            isPinching = false;
        }
        
        // Si estamos agarrando un objeto, actualizamos su posición
        if (isPinching && grabbedObject != null)
        {
            grabbedObject.transform.position = pinchPosition + grabOffset;
            grabbedObject.transform.rotation = hand.transform.rotation * grabRotationOffset;
        }
    }

    void LateUpdate()
    {
        Mirror(leftT, rightT); // Reflejamos la mano
        HandlePinchInteraction(); // Manejamos las interacciones de pinza
    }
}
