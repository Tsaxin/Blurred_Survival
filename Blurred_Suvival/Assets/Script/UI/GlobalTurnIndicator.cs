using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalTurnIndicator : MonoBehaviour
{
    public GameObject TurnPanel;
    public TextMeshProUGUI TurnIndicatorText;

    public void GlobalTurnIndicatorState(bool State, string text = null)
    {
        if(text!=null && TurnIndicatorText!=null)
            TurnIndicatorText.text = text;
        if (TurnPanel != null)
        {
            TurnPanel.SetActive(State);
        }
    }
    public void SetTurnIndicatorText(string text)
    {
        if (TurnIndicatorText != null)
        {
            TurnIndicatorText.text = text;
        }
    }
}
