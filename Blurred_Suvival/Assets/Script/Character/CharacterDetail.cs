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

    public void SetDetail(string CharacterName)
    {
        GetComponent<CharacterStats>().CharacterName = CharacterName;
    }
}
