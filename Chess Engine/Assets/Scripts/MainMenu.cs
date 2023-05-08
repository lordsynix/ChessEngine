using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    private void Start()
    {
        // SFX
        FindObjectOfType<AudioManager>().Play("theme_mainmenu");
    }

    #region Buttons

    public void Play()
    {
        // L�dt die Schachszene
        SceneManager.LoadScene(1);
    }

    public void Options()
    {
        // L�dt die Applikationseinstellungen
    }

    public void Quit()
    {
        // Schliesst die Anwendung
        Application.Quit();
    }

    #endregion

}
