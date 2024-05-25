using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchSceneOnLoad : MonoBehaviour
{
    [SerializeField] string scene;

    void Start()
    {
        SceneManager.LoadScene(scene);
    }
}
