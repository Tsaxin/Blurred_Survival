using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconToolTip : MonoBehaviour
{
    public GameObject CampIcon, SquadIcon;

    public Squad squad;
    public Transform SquadHolder;

    // Update is called once per frame
    void Update()
    {
        CampIcon.GetComponent<TooltipTrigger>().SetTriggerText(
                "Camp",
                $"Survivor can create camp at their current location to organize themselves."
            );
        SquadIcon.GetComponent<TooltipTrigger>().SetTriggerText(
                "Survivors",
                $"Survivor Count: {SquadHolder.childCount}/{squad.MaxSurvivorCountInGroup}"
            );

    }
}
