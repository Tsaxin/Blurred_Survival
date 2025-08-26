using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    public GameObject Indicator;

    public void SetIndicator(bool Value)
    {
        Indicator.SetActive(Value);
    }
}
