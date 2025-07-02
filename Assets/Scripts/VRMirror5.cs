using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRMirror5 : MonoBehaviour
{
    public GameObject leftHand; //mano izquierda ([BuildingBlock] Hand Tracking Left)
    public GameObject rightHand; //mano derecha (OVRCustomHandPrefabRight))
    public Transform CameraRig;

    private Transform leftT;
    private Transform rightT;

    void Awake() //almacenamos la referencia de los transform de las manos
    {
    leftT  = leftHand.transform;   // almaceno la referencia una sola vez
    rightT = rightHand.transform;   
    }

    Vector3 ReflectRelativeVector(Vector3 relativeVec)
    {
        Vector3 normal = CameraRig.right;      // eje x del jugador (CameraRig.right)
        return Vector3.Reflect(relativeVec, normal);
    }

    void Update() //Se ejecuta en cada frame del juego
    {
        MirrorFromTo(leftT, rightT); //Llamamos a la funcion MirrorFromTo(origen,destino), para que la mano derecha imite a la izquierda   
    }

    public void MirrorFromTo(Transform transfOrigen, Transform transfDestino) //Esta función se encarga de hacer el espejo desde el transform de origen al transform destino.
    //recibe dos parámetros tipo Transform: sourceTransform es la mano que se va a reflejar; destTransform es la mano que imitará en espejo la otra
    {
        // 1) Determinamos la posición de la mano destino (derecha) --> reflejando la posición de la mano origen (izquierda)
        //CameraRig.position es el centro del jugador (la posición de la cámara).
        Vector3 vectorJugadorManoOrigen = transfOrigen.position - CameraRig.position; //Vector desde el centro del jugador hasta la mano origen.
        //playerToSourceHand = posición de la mano origen (izquierda) - posición del jugador (centro de la cámara).
        //El resultado es un vector que apunta desde el origen de la cámara hacia la mano izquierda. Su módulo es igual a la distancia real entre esos dos puntos.
        Vector3 vectorJugadorManoDestino = ReflectRelativeVector(vectorJugadorManoOrigen); // Invierte  para obtener “el punto simétrico” al otro lado del cuerpo.
        transfDestino.position = CameraRig.position + vectorJugadorManoDestino; //posición de la mano destino (derecha) = posición de la cámara + vector reflejado.
        //Nota: un punto + un desplazamiento = otro punto.
        // punto - punto = vector (desplazamiento).
        
   
        // 2) Se determina la rotación de la mano destino (derecha)
        Vector3 forwardVec = ReflectRelativeVector(transfOrigen.forward); //hacia donde apunta la palma
        Vector3 upVec = ReflectRelativeVector(transfOrigen.up); //hacia los nudillos de la mano.
        transfDestino.rotation = Quaternion.LookRotation(-forwardVec, -upVec) * Quaternion.Euler(0, 180f, 0); // Quaternion.LookRotation() crea una rotación a partir de esos dos vectores.
        // el - invierte los vectores para simular correctamente el espejo.
    }



}
