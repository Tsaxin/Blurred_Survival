using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject MainMenuUI, CreditUI;
    public void OnNewGameClick()
    {
       SceneManager.LoadScene(1);
    }

    public void OnCreditClick()
    {
        CreditUI.SetActive(true);
        MainMenuUI.SetActive(false);
    }

    public void OnCreditExit() {
        CreditUI.SetActive(false);
        MainMenuUI.SetActive(true);
    }

    public void OnExitClick()
    {
        Application.Quit();
    }
}
