using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{   
    public void LoadScene(string nombre)
    {
        // Cambia a la escena especificada por el nombre
        SceneManager.LoadScene(nombre);
    }
    
}
