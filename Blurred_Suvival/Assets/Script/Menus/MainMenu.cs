using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject MainMenuUI, CreditUI;
    public Button LoadButton;

    void Start()
    {
        if (SaveSystem.LoadPlayerLocation() != null)
        {
            LoadButton.AddComponent<UIHoverScale>();
            LoadButton.interactable = true;
            return;
        }
        else
        {
            Debug.Log("No data found");
            LoadButton.interactable = false;
        }
    }
    public void OnNewGameClick()
    {
        DeleteData();
        MusicManager.Instance.PlaySelectSound();
        MusicManager.Instance.PlayAmbientMusic();

        GameModeTracker.Instance.GameMode = 0;
        LoadingManager.Instance.LoadScene(1);
    }

    public void OnLoadGameClick()
    {
        MusicManager.Instance.PlaySelectSound();
        MusicManager.Instance.PlayAmbientMusic();
        GameModeTracker.Instance.GameMode = 1;
        LoadingManager.Instance.LoadScene(1);
    }

    public void OnCreditClick()
    {
        MusicManager.Instance.PlaySelectSound();
        CreditUI.SetActive(true);
        MainMenuUI.SetActive(false);
    }

    public void OnCreditExit()
    {
        MusicManager.Instance.PlaySelectSound();
        CreditUI.SetActive(false);
        MainMenuUI.SetActive(true);
    }

    public void OnExitClick()
    {
        MusicManager.Instance.PlaySelectSound();
        Application.Quit();
    }

    [ContextMenu("Delete Data")]
    public void DeleteData()
    {
        SaveSystem.DeleteSaveData();
    }
}
