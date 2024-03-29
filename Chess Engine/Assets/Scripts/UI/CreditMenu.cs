using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Klasse <c>CreditMenu</c> regelt UI-Elemente und kann vernachlaessigt werden.
/// </summary>
public class CreditMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject creditsMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ExitCreditScreen();
    }
    public void ExitCreditScreen()
    {
        mainMenu.SetActive(true);
        creditsMenu.SetActive(false);
    }
}
