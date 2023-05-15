using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    private void Start()
    {
        // Startet die Hintergrundmusik des Hauptmen�s.
        FindObjectOfType<AudioManager>().Play("theme_mainmenu");
    }

    #region Buttons

    public void Play()
    {
        // L�dt die Schachszene (Hauptszene)
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        // Schliesst die Anwendung
        Application.Quit();
    }

    #endregion

}
