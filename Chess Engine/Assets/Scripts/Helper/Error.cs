using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Error : MonoBehaviour
{
    [Header("Windows")]
    public GameObject errorWindow;
    
    public static Error instance;

    private void Awake()
    {
        instance = this;
    }

    public void OnError()
    {
        errorWindow.SetActive(true);
    }

    public void Close()
    {
        errorWindow.SetActive(false);
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}
