using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Die Klasse <c>MainMenu</c> dient der Ermittlung des Nutzernamen und Verwaltung von 
/// UI-Elementen. Die Klasse hat keinen Einfluss auf die Arbeit und kann vernachlaessigt werden.
/// </summary>
public class MainMenu : MonoBehaviour
{
    public GameObject usernameMenu;
    public InputField usernameInputField;

    private string username;

    private void Start()
    {
        username = ArgumentsParser.instance.GetArg("-username");

        // Uebernimmt den Benutzername, falls das Spiel mit dem Launcher gestartet wurde.
        if (!string.IsNullOrEmpty(username))
        {
            PlayerPrefs.SetString("username", username);
        }

        Username();

        // Startet die Hintergrundmusik des Hauptmenues.
        bool music = PlayerPrefs.GetInt("Music") == 1;
        if (music) FindObjectOfType<AudioManager>().Play("theme_mainmenu");
    }

    private void Username()
    {
        username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(username))
        {
            usernameMenu.SetActive(true);
        }


        if (!PlayerPrefs.HasKey("lightCol")) PlayerPrefs.SetString("lightCol", "DDC39C");
        if (!PlayerPrefs.HasKey("darkCol")) PlayerPrefs.SetString("darkCol", "936D4B");

        if (!PlayerPrefs.HasKey("Fullscreen")) PlayerPrefs.SetInt("Fullscreen", 1);
        if (!PlayerPrefs.HasKey("Music")) PlayerPrefs.SetInt("Music", 1);
        if (!PlayerPrefs.HasKey("SFX")) PlayerPrefs.SetInt("SFX", 1);
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
