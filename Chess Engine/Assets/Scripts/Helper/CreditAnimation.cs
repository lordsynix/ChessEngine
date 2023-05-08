using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditAnimation : MonoBehaviour
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
