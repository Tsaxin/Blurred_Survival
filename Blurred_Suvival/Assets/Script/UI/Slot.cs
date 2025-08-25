using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public Image Sprite;
    public GameObject QuantityHolder;
    public TextMeshProUGUI Quantity;

    public void SetText(string Quantity)
    {
        QuantityHolder.SetActive(true);
        this.Quantity.text = Quantity;
    }
}
