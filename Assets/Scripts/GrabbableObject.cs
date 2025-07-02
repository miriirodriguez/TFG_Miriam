using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    [Header("Grabbable Settings")]
    public bool canBeGrabbed = true;
    public float grabSmoothness = 10f; // Suavidad del movimiento al agarrar
    
    private bool isBeingGrabbed = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
        // Aseguramos que el objeto tenga un Collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"GrabbableObject en {gameObject.name} no tiene Collider. Añadiendo BoxCollider.");
            gameObject.AddComponent<BoxCollider>();
        }
        
        // Aseguramos que el objeto tenga un Rigidbody
        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }
    }
    
    public void OnGrabbed()
    {
        isBeingGrabbed = true;
        // Aquí puedes añadir efectos visuales o sonoros
    }
    
    public void OnReleased()
    {
        isBeingGrabbed = false;
        // Aquí puedes añadir efectos al soltar el objeto
    }
    
    // Método opcional para movimiento suave
    public void SetTargetTransform(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = rotation;
    }
    
    void Update()
    {
        // Movimiento suave opcional (si prefieres esto al movimiento directo)
        if (isBeingGrabbed && grabSmoothness > 0)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * grabSmoothness);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * grabSmoothness);
        }
    }
}
