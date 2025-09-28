using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDetail : MonoBehaviour
{
    public enum Gender
    {
        Male, Female
    }

    public Gender gender;
    public string UniqueID;

    public void SetDetail(string CharacterName)
    {
        if (GetComponent<CharacterStats>().CharacterName == "")
        {
            GetComponent<CharacterStats>().CharacterName = CharacterName;
        }

        Debug.Log($"Name is {CharacterName}");
    }

    [ContextMenu("Get Unique ID")]

    public void GetUniqueID()
    {
        UniqueID = gameObject.name;
    }
}
