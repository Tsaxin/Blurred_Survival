using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUIEvents : MonoBehaviour
{
    public void OpenInventory()
    {
        InventoryUIManager.Instance.OpenInventory();
    }

    public void OpenGear()
    {
        GearUI.Instance.OnShowGear(GetComponent<GearEquipper>());
    }

    public void OpenStat()
    {
        CharacterStatsUI.Instance.OpenStatPanel(GetComponent<CharacterStats>());
    }
}
