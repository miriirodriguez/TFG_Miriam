using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMirrorfinal2 : MonoBehaviour
{
    [Header("Variables")]
    public OVRHand leftHand; //mano izquierda 
    public OVRHand rightHand; //mano derecha 
    public Transform CameraRig; //Camera Rig

    [Header("Configuración de interacción")]
    public float pinchThreshold = 0.7f; // Umbral para detectar pinza (0-1)
    public float fistThreshold = 0.8f; // Umbral para detectar puño cerrado (0-1)
    public float grabDistance = 0.07f; // Distancia máxima para agarrar objetos
    public float throwForceMultiplier = 5f; // Multiplicador de fuerza para lanzar objetos
    public LayerMask grabbableLayer = -1; // Capa de objetos que se pueden agarrar

    private Transform leftT;
    private Transform rightT;
    private Vector3 mirrorNormal; 

    // Variables para interacción con pinza
    private bool isLeftPinching = false;
    private bool isRightPinching = false;
    private GameObject leftGrabbedObject = null;
    private GameObject rightGrabbedObject = null;
    private Vector3 leftGrabOffset;
    private Vector3 rightGrabOffset;
    private Quaternion leftGrabRotationOffset;
    private Quaternion rightGrabRotationOffset;
    
    // Variables para interacción con puño
    private bool isLeftFisting = false;
    private bool isRightFisting = false;
    private GameObject leftFistObject = null;
    private GameObject rightFistObject = null;
    private Vector3 leftFistOffset;
    private Vector3 rightFistOffset;
    private Quaternion leftFistRotationOffset;
    private Quaternion rightFistRotationOffset;
    
    // Velocidades para lanzamiento
    private Vector3 leftHandVelocity = Vector3.zero;
    private Vector3 rightHandVelocity = Vector3.zero;
    private Vector3 leftHandPrevPos = Vector3.zero;
    private Vector3 rightHandPrevPos = Vector3.zero;
    
    // Posiciones de los dedos para calcular pinza
    private Transform leftThumbTip;
    private Transform leftIndexTip;
    private Transform rightThumbTip;
    private Transform rightIndexTip;
    
    void Awake() //almacenamos la referencia de los transform de las manos
    {
        leftT = leftHand.transform;   // almaceno la referencia una sola vez
        rightT = rightHand.transform;
        mirrorNormal = CameraRig.right.normalized;
        
        // Inicializamos posiciones previas para calcular velocidad
        leftHandPrevPos = leftT.position;
        rightHandPrevPos = rightT.position;
    }

    void Start()
    {
        // Esperamos a que los skeletons estén inicializados
        StartCoroutine(InitializeBones());
    }

    IEnumerator InitializeBones()
    {
        // Esperamos hasta que los skeletons estén listos
        var leftSkeleton = leftHand.GetComponent<OVRSkeleton>();
        var rightSkeleton = rightHand.GetComponent<OVRSkeleton>();
        
        while (!leftSkeleton.IsInitialized || !rightSkeleton.IsInitialized)
        {
            yield return null;
        }

        // Obtenemos las referencias a las puntas de los dedos directamente
        leftThumbTip = GetBone(leftSkeleton, OVRSkeleton.BoneId.Hand_ThumbTip);
        leftIndexTip = GetBone(leftSkeleton, OVRSkeleton.BoneId.Hand_IndexTip);
        rightThumbTip = GetBone(rightSkeleton, OVRSkeleton.BoneId.Hand_ThumbTip);
        rightIndexTip = GetBone(rightSkeleton, OVRSkeleton.BoneId.Hand_IndexTip);
    }

    // Función auxiliar para obtener huesos específicos
    Transform GetBone(OVRSkeleton skeleton, OVRSkeleton.BoneId boneId)
    {
        foreach (var bone in skeleton.Bones)
        {
            if (bone.Id == boneId) return bone.Transform;
        }
        return null;
    }

    Vector3 ReflectRelativeVector(Vector3 relativeVec)
    {    
        return Vector3.Reflect(relativeVec, mirrorNormal);
    }

    public void MirrorFromTo(Transform transfOrigen, Transform transfDestino)
    {
        // 1) Determinamos la posición de la mano destino (derecha)
        Vector3 vectorJugadorManoOrigen = transfOrigen.position - CameraRig.position;
        Vector3 vectorJugadorManoDestino = ReflectRelativeVector(vectorJugadorManoOrigen);
        transfDestino.position = CameraRig.position + vectorJugadorManoDestino;
        
        // 2) Se determina la rotación de la mano destino (derecha)
        Vector3 forwardVec = ReflectRelativeVector(transfOrigen.forward);
        Vector3 upVec = ReflectRelativeVector(transfOrigen.up);
        transfDestino.rotation = Quaternion.LookRotation(-forwardVec, -upVec);
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

    void HandlePinchInteraction()
    {
        // Manejo de pinza para mano izquierda
        HandleHandPinch(leftHand, leftThumbTip, leftIndexTip, ref isLeftPinching, ref leftGrabbedObject, ref leftGrabOffset, ref leftGrabRotationOffset);
        
        // Manejo de pinza para mano derecha
        HandleHandPinch(rightHand, rightThumbTip, rightIndexTip, ref isRightPinching, ref rightGrabbedObject, ref rightGrabOffset, ref rightGrabRotationOffset);
    }

    void HandleFistInteraction()
    {
        // Manejo de puño para mano izquierda
        HandleHandFist(leftHand, ref isLeftFisting, ref leftFistObject, ref leftFistOffset, ref leftFistRotationOffset, leftHandVelocity);
        
        // Manejo de puño para mano derecha
        HandleHandFist(rightHand, ref isRightFisting, ref rightFistObject, ref rightFistOffset, ref rightFistRotationOffset, rightHandVelocity);
    }

    void HandleHandFist(OVRHand hand, ref bool isFisting, ref GameObject fistObject, ref Vector3 fistOffset, ref Quaternion fistRotationOffset, Vector3 handVelocity)
    {
        // Calculamos la fuerza del puño promediando todos los dedos excepto el pulgar
        float fistStrength = (hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) +
                             hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) +
                             hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) +
                             hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky)) / 4f;
        
        Vector3 handPosition = hand.transform.position;
        
        // Detectamos si empezamos a hacer puño
        if (!isFisting && fistStrength > fistThreshold)
        {
            // Buscamos objetos cerca de la posición de la mano
            Collider[] nearbyObjects = Physics.OverlapSphere(handPosition, grabDistance, grabbableLayer);
            
            if (nearbyObjects.Length > 0)
            {
                // Tomamos el objeto más cercano
                GameObject closestObject = null;
                float closestDistance = float.MaxValue;
                
                foreach (var collider in nearbyObjects)
                {
                    float distance = Vector3.Distance(handPosition, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestObject = collider.gameObject;
                    }
                }
                
                if (closestObject != null)
                {
                    // Iniciamos el agarre con puño
                    isFisting = true;
                    fistObject = closestObject;
                    fistOffset = fistObject.transform.position - handPosition;
                    fistRotationOffset = Quaternion.Inverse(hand.transform.rotation) * fistObject.transform.rotation;
                    
                    // Deshabilitamos la física del objeto mientras lo agarramos
                    Rigidbody rb = fistObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                }
            }
        }
        // Si ya estamos haciendo puño y soltamos - LANZAMOS EL OBJETO
        else if (isFisting && fistStrength < fistThreshold)
        {
            if (fistObject != null)
            {
                // Reactivamos la física del objeto
                Rigidbody rb = fistObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    // Aplicamos la velocidad de la mano multiplicada por la fuerza de lanzamiento
                    rb.velocity = handVelocity * throwForceMultiplier;
                    // Añadimos un poco de torque para rotación realista
                    rb.angularVelocity = new Vector3(
                        Random.Range(-5f, 5f),
                        Random.Range(-5f, 5f),
                        Random.Range(-5f, 5f)
                    );
                }
                
                fistObject = null;
            }
            isFisting = false;
        }
        
        // Si estamos agarrando un objeto con el puño, actualizamos su posición
        if (isFisting && fistObject != null)
        {
            fistObject.transform.position = handPosition + fistOffset;
            fistObject.transform.rotation = hand.transform.rotation * fistRotationOffset;
        }
    }

    void HandleHandPinch(OVRHand hand, Transform thumbTip, Transform indexTip, ref bool isPinching, ref GameObject grabbedObject, ref Vector3 grabOffset, ref Quaternion grabRotationOffset)
    {
        float pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
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
        // Calculamos velocidades de las manos para el lanzamiento
        leftHandVelocity = (leftT.position - leftHandPrevPos) / Time.deltaTime;
        rightHandVelocity = (rightT.position - rightHandPrevPos) / Time.deltaTime;
        
        // Actualizamos posiciones previas
        leftHandPrevPos = leftT.position;
        rightHandPrevPos = rightT.position;
        
        MirrorFromTo(leftT, rightT); // Reflejamos la mano
        HandlePinchInteraction(); // Manejamos las interacciones de pinza
        HandleFistInteraction(); // Manejamos las interacciones de puño
    }
}
