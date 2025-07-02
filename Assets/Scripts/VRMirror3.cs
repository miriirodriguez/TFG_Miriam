using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMirror3 : MonoBehaviour
{
    public OVRHand leftHand; //mano izquierda (OVRHandPrefabLeft)
    public GameObject rightHand; //mano derecha (OVRHandPrefabRight)
    public Transform CameraRig;

    private Transform leftT;
    private Transform rightT;
    
    private Vector3 mirrorNormal;

    // Diccionarios para mapear los huesos de los dedos
    private Dictionary<OVRSkeleton.BoneId, Transform> leftBones;
    private Dictionary<OVRSkeleton.BoneId, Transform> rightBones;
    
    // Referencias a los skeletons
    private OVRSkeleton leftSkeleton;
    private OVRSkeleton rightSkeleton;

    void Awake() //almacenamos la referencia de los transform de las manos
    {
        leftT  = leftHand.transform;   // almaceno la referencia una sola vez
        rightT = rightHand.transform;
        mirrorNormal = CameraRig.right.normalized;
        
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
    }

    Vector3 ReflectRelativeVector(Vector3 relativeVec)
    {    
        return Vector3.Reflect(relativeVec, mirrorNormal);
    }

    public void MirrorFromTo(Transform transfOrigen, Transform transfDestino) //Esta función se encarga de hacer el espejo desde el transform de origen al transform destino.
    //recibe dos parámetros tipo Transform: sourceTransform es la mano que se va a reflejar; destTransform es la mano que imitará en espejo la otra
    {
        // 1) Determinamos la posición de la mano destino (derecha) --> reflejando la posición de la mano origen (izquierda)
        //CameraRig.position es el centro del jugador (la posición de la cámara).
        Vector3 vectorJugadorManoOrigen = transfOrigen.position - CameraRig.position; //Vector desde el centro del jugador hasta la mano origen.
        //playerToSourceHand = posición de la mano origen (izquierda) - posición del jugador (centro de la cámara).
        //El resultado es un vector que apunta desde el origen de la cámara hacia la mano izquierda. Su módulo es igual a la distancia real entre esos dos puntos.
        Vector3 vectorJugadorManoDestino = ReflectRelativeVector(vectorJugadorManoOrigen); // Invierte  para obtener "el punto simétrico" al otro lado del cuerpo.
        transfDestino.position = CameraRig.position + vectorJugadorManoDestino; //posición de la mano destino (derecha) = posición de la cámara + vector reflejado.
        //Nota: un punto + un desplazamiento = otro punto.
        // punto - punto = vector (desplazamiento).
        
   
        // 2) Se determina la rotación de la mano destino (derecha)
        Vector3 forwardVec = ReflectRelativeVector(transfOrigen.forward); //hacia donde apunta la palma
        Vector3 upVec = ReflectRelativeVector(transfOrigen.up); //hacia los nudillos de la mano.
        transfDestino.rotation = Quaternion.LookRotation(-forwardVec, -upVec); // Quaternion.LookRotation() crea una rotación a partir de esos dos vectores.
        // el - invierte los vectores para simular correctamente el espejo.
    }

    void MirrorFingers()
    {
        // Si los diccionarios no están inicializados, no hacemos nada
        if (leftBones.Count == 0 || rightBones.Count == 0) return;

        // Lista de todos los huesos de los dedos que queremos reflejar
        OVRSkeleton.BoneId[] fingerBones = {
            // Pulgar
            OVRSkeleton.BoneId.Hand_Thumb1,
            OVRSkeleton.BoneId.Hand_Thumb2,
            OVRSkeleton.BoneId.Hand_Thumb3,
            
            // Índice
            OVRSkeleton.BoneId.Hand_Index1,
            OVRSkeleton.BoneId.Hand_Index2,
            OVRSkeleton.BoneId.Hand_Index3,
            
            // Medio
            OVRSkeleton.BoneId.Hand_Middle1,
            OVRSkeleton.BoneId.Hand_Middle2,
            OVRSkeleton.BoneId.Hand_Middle3,
            
            // Anular
            OVRSkeleton.BoneId.Hand_Ring1,
            OVRSkeleton.BoneId.Hand_Ring2,
            OVRSkeleton.BoneId.Hand_Ring3,
            
            // Meñique
            OVRSkeleton.BoneId.Hand_Pinky1,
            OVRSkeleton.BoneId.Hand_Pinky2,
            OVRSkeleton.BoneId.Hand_Pinky3
        };

        // Reflejamos cada hueso
        foreach (var boneId in fingerBones)
        {
            if (leftBones.ContainsKey(boneId) && rightBones.ContainsKey(boneId))
            {
                Transform leftBone = leftBones[boneId];
                Transform rightBone = rightBones[boneId];

                // Calculamos la rotación reflejada
                Vector3 leftForward = leftBone.forward;
                Vector3 leftUp = leftBone.up;
                
                // Reflejamos los vectores direccionales
                Vector3 mirroredForward = ReflectRelativeVector(leftForward);
                Vector3 mirroredUp = ReflectRelativeVector(leftUp);
                
                // Aplicamos la rotación reflejada
                rightBone.rotation = Quaternion.LookRotation(-mirroredForward, -mirroredUp);
            }
        }
    }

    void LateUpdate() //Se ejecuta en cada frame del juego
    {
        MirrorFromTo(leftT, rightT); //Llamamos a la funcion MirrorFromTo(origen,destino), para que la mano derecha imite a la izquierda
        MirrorFingers(); // Reflejamos también los dedos
    }

}
