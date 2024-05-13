using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeResolution : MonoBehaviour
{
    TMP_Dropdown dropdown;

    Vector2Int screenSize;

    void Awake()
    {
        screenSize = new(Display.main.systemWidth, Display.main.systemHeight);
        
        PlayerPrefs.SetInt("fullscreen", PlayerPrefs.GetInt("fullscreen", 0));

        dropdown = GetComponent<TMP_Dropdown>();

        if (PlayerPrefs.GetInt("fullscreen", 0) == 0)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
            dropdown.value = 0;
        }
        else
        {
            Screen.SetResolution(screenSize.x, screenSize.y, true);
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;

            LockCursor.PushLockState(CursorLockMode.Locked);
            dropdown.value = 1;
        }

        dropdown.onValueChanged.AddListener(OnValueChange);
    }

    void OnValueChange(int index)
    {
        if(dropdown.options[index].text == "Fullscreen")
        {
            Screen.SetResolution(screenSize.x, screenSize.y, true);
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;

            PlayerPrefs.SetInt("fullscreen", 1);
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;

            PlayerPrefs.SetInt("fullscreen", 0);
        }
    }
}
