using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtons : MonoBehaviour
{
    public CharacterController CC;
    public GameObject Retreat, Inventory, ActiveSkill, PassiveSkill, Gear, CharacterStat;

    void OnEnable()
    {
        LoadButtonFunction();
    }

    void LoadButtonFunction()
    {
        CharacterUIEvents uiEvents = GetComponent<CharacterUIEvents>();
        Retreat.GetComponent<Button>().onClick.AddListener(uiEvents.Retreat);
        Inventory.GetComponent<Button>().onClick.AddListener(uiEvents.OpenInventory);
        Gear.GetComponent<Button>().onClick.AddListener(uiEvents.OpenGear);
        CharacterStat.GetComponent<Button>().onClick.AddListener(uiEvents.OpenStat);
    }

    public void LoadTriggerText()
    {
        SetSkillTriggerText();
        Retreat.GetComponent<TooltipTrigger>().SetTriggerText("Retreat", Squad.Instance.GetRetreatChance() + "% chance of retreating from the battlefield.\n\n Higher number of survivor in group means less chance of retreating.");
        Inventory.GetComponent<TooltipTrigger>().SetTriggerText("Inventory", "Can view and equip collected items. Each action consumes survivor's turn.");
        Gear.GetComponent<TooltipTrigger>().SetTriggerText("Gear", "View survivor's current outfit and gears.");
        CharacterStat.GetComponent<TooltipTrigger>().SetTriggerText("Survivor's stat", "View and Upgrade survivor's stat. Each level up gives 1 attribute point.");
    }

    void SetSkillTriggerText()
    {
        if (CC.GetComponent<CharacterPassive>().passiveSkills.Count > 0)
        {
            PassiveSkill PS = CC.GetComponent<CharacterPassive>().passiveSkills[0].passiveSkill;
            PassiveSkill.GetComponent<Image>().sprite = PS.icon;

            PassiveSkill.GetComponent<TooltipTrigger>().SetTriggerText(PS.skillName, PS.description);
        }
        else
        {
            PassiveSkill.GetComponent<Button>().interactable = false;
            PassiveSkill.GetComponent<TooltipTrigger>().SetTriggerText( "None", "No effects.");
        }

        //For active skill
        ActiveSkill.GetComponent<TooltipTrigger>().SetTriggerText("None", "No effects.");
    }
}
