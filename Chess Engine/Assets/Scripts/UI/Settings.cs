using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>Settings</c> regelt die Settings der Anwendung und kann vernachlaessigt werden.
/// </summary>
public class Settings : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ExitOptions();
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void ExitOptions()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }
}
