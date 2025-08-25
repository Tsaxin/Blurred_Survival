using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image Sprite;
    public GameObject QuantityHolder;
    public TextMeshProUGUI Quantity;

    public void SetImage(Sprite sprite)
    {
        Sprite.sprite = sprite;
    }
    public void SetText(string Quantity)
    {
        QuantityHolder.SetActive(true);
        this.Quantity.text = Quantity;
    }
}
