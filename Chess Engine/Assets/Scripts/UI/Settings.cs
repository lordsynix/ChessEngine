using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Die Klasse <c>Settings</c> regelt die Settings der Anwendung und kann vernachlaessigt werden.
/// </summary>
public class Settings : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject optionsMenu;

    public Toggle fullscreenToggle;
    public Toggle musicToggle;
    public Toggle sfxToggle;

    public InputField lightCol;
    public InputField darkCol;

    private void OnEnable()
    {
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen") == 1;
        musicToggle.isOn = PlayerPrefs.GetInt("Music") == 1;
        sfxToggle.isOn = PlayerPrefs.GetInt("SFX") == 1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ExitOptions();
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void ToggleMusic(bool music)
    {
        PlayerPrefs.SetInt("Music", music ? 1 : 0);
    }

    public void ToggleSFX(bool sfx)
    {
        PlayerPrefs.SetInt("SFX", sfx ? 1 : 0);
    }

    public void OnApply()
    {
        PlayerPrefs.SetString("lightCol", lightCol.text);
        PlayerPrefs.SetString("darkCol", darkCol.text);

        SceneManager.LoadScene(0);
    }

    public void OnReset()
    {
        Screen.fullScreen = true;

        PlayerPrefs.SetInt("Fullscreen", 1);
        PlayerPrefs.SetInt("Music", 1);
        PlayerPrefs.SetInt("SFX", 1);

        PlayerPrefs.SetString("lightCol", "#DDC39C");
        PlayerPrefs.SetString("darkCol", "#936D4B");

        SceneManager.LoadScene(0);
    }

    public void ExitOptions()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }
}
