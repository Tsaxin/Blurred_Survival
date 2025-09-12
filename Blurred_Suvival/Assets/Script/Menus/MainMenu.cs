using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject MainMenuUI, CreditUI;
    public void OnNewGameClick()
    {
        MusicManager.Instance.PlaySelectSound();
        MusicManager.Instance.PlayAmbientMusic();

        SceneManager.LoadScene(1);
    }

    public void OnCreditClick()
    {
        MusicManager.Instance.PlaySelectSound();
        CreditUI.SetActive(true);
        MainMenuUI.SetActive(false);
    }

    public void OnCreditExit() {
        MusicManager.Instance.PlaySelectSound();
        CreditUI.SetActive(false);
        MainMenuUI.SetActive(true);
    }

    public void OnExitClick()
    {
        MusicManager.Instance.PlaySelectSound();
        Application.Quit();
    }
}
