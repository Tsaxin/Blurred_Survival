using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image FrameSprite,Sprite;
    public GameObject SelectedImage;
    public GameObject QuantityHolder;
    public TextMeshProUGUI Quantity;

    public Color ActiveColor, InActiveColor;

    public void SetImage(Sprite sprite)
    {
        Sprite.sprite = sprite;
    }
    public void SetText(string Quantity)
    {
        QuantityHolder.SetActive(true);
        this.Quantity.text = Quantity;
    }

    public bool SetSelectedImage(bool State, Slot PreviousSelected = null)
    {
        if (PreviousSelected != null)
        {
            PreviousSelected.SetSelectedImage(false);
            if (PreviousSelected == this)
            {
                return false;
            }
        }
        SelectedImage.SetActive(State);
        return true;
    }

    public void SetActiveColor(bool State)
    {
        if (State)
        {
            FrameSprite.color = ActiveColor;
            Sprite.color = ActiveColor;
        }
        else
        {
            FrameSprite.color = InActiveColor;
            Sprite.color = InActiveColor;
        }
    }
}
