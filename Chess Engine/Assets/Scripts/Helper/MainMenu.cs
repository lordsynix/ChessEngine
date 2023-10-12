using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject usernameMenu;
    public InputField usernameInputField;

    private string username;

    private void Start()
    {
        // Startet die Hintergrundmusik des Hauptmenues.
        FindObjectOfType<AudioManager>().Play("theme_mainmenu");

        username = ArgumentsParser.instance.GetArg("-username");

        // Uebernimmt den Benutzername, falls das Spiel mit dem Launcher gestartet wurde.
        if (!string.IsNullOrEmpty(username))
        {
            PlayerPrefs.SetString("username", username);
        }

        Username();
    }

    private void Username()
    {
        username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(username))
        {
            usernameMenu.SetActive(true);
        }
    }

    #region Buttons

    public void Play(string color)
    {
        // Uebergibt die Farbe des Spielers an das Brett.
        if (color.Length > 1) Debug.LogError("Please enter the color as a char");
        Board.SetPlayerColor(color[0]); 

        // Laedt die Schachszene (Hauptszene)
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        // Schliesst die Anwendung
        Application.Quit();
    }

    public void SubmitUsername()
    {
        username = usernameInputField.text;

        if (username.Length <= 3 || username.Length >= 26) return;

        PlayerPrefs.SetString("username", username);

        usernameMenu.SetActive(false);
    }

    #endregion

}
