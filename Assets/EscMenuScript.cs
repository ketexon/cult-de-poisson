using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscMenuScript : MonoBehaviour
{
    public static bool Paused = false;
    public GameObject EscBackground;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(Paused)
            {
                Play();
            }
            else
                Stop();
        }
    }

    void Stop()
    {
        EscBackground.SetActive(true);
        Time.timeScale = 0f;
        Paused = true;

    }

    public void Play() //esc key when paused
    {
        EscBackground.SetActive(false);
        Time.timeScale = 1f;
        Paused = false;
    }

    /*
    public void MainMenuButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1)
    }
    */
    // will need for later

    
}
