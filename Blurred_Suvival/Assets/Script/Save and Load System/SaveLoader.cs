using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SaveLoader : MonoBehaviour
{
    [Header("Cut Scene Manager")]
    public CutsceneManager cutsceneManager;

    [Header("Squad")]
    public Transform SquadMover;
    public Transform SquadParent;

    [Header("Event Manager")]
    public Transform EventParent;
    [Header("Hunger Manager")]
    public HungerManager hungerManager;

    [Header("Player Inventory")]
    public PlayerInventory playerInventory;

    [Header("Object Holder")]
    public MasterEvent masterEvent;
    public LootMaster lootMaster;
    public static SaveLoader Instance;
    public bool IsGameLoaded;
    // Start is called before the first frame update
    void Start()
    {
        IsGameLoaded = false;
        if (Instance == null)
        {
            Instance = this;
        }
        if (GameModeTracker.Instance == null)
        {
            return;
        }

        if (CheckData() && GameModeTracker.Instance.GameMode == 1)
        {
            cutsceneManager.cutsceneID = "0";
            LoadData();
            IsGameLoaded = true;
        }
        else
        {
            IsGameLoaded = true;
            cutsceneManager.PlayCutsceneByID();
            //Show cutscene
        }

    }
    bool CheckData()
    {
        if (SaveSystem.LoadPlayerLocation() != null)
            return true;
        return false;
    }

    void LoadData()
    {
        LoadPlayerLocation();
        LoadEventData();
        LoadHungerData();
        LoadPlayerData();
        LoadInventoryData();
    }

    void LoadPlayerLocation()
    {
        if (SaveSystem.LoadPlayerLocation() != null)
        {
            SquadMover.transform.position = new Vector3(SaveSystem.LoadPlayerLocation().LocationX
                , SaveSystem.LoadPlayerLocation().LocationY
                    , SquadMover.transform.position.z);
        }
    }

    void LoadEventData()
    {
        EventData eventData = SaveSystem.LoadEventData();

        if (eventData == null) return;

        for (int i = EventParent.childCount - 1; i >= 0; i--)
        {
            bool found = false;
            for (int j = eventData.baseEventDatas.Count - 1; j >= 0; j--)
            {
                if (eventData.baseEventDatas[j].Name == EventParent.GetChild(i).name)
                {
                    EventParent.GetChild(i).GetComponent<EventTrigger>().EventCompleted = eventData.baseEventDatas[j].IsCompleted;
                    EventParent.GetChild(i).gameObject.SetActive(eventData.baseEventDatas[j].IsActive);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Destroy(EventParent.GetChild(i).gameObject);
            }
        }
    }

    void LoadHungerData()
    {
        if (SaveSystem.LoadHungerData() == null) return;
        hungerManager.currentHunger = SaveSystem.LoadHungerData().currentHunger;
    }

    void LoadPlayerData()
    {
        if (SaveSystem.LoadPlayerData() == null) return;

        PlayerData playerData = SaveSystem.LoadPlayerData();

        for (int i = SquadParent.childCount - 1; i >= 0; i--)
        {
            Destroy(SquadParent.GetChild(i).gameObject);
        }

        foreach (CharacterData data in playerData.characterData)
        {
            bool found = false;
            foreach (GameObject obj in masterEvent.Survivors)
            {
                if (data.UniqueID == obj.GetComponent<CharacterDetail>().UniqueID)
                {
                    LoadOneCharacter(obj, data);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                foreach (GameObject obj in masterEvent.EventSurvivors)
                {
                    if (data.UniqueID == obj.GetComponent<CharacterDetail>().UniqueID)
                    {
                        LoadOneCharacter(obj, data);
                        found = true;
                        break;
                    }
                }
            }
        }
    }

    void LoadOneCharacter(GameObject character, CharacterData characterData)
    {
        CharacterStatsData characterStatsData = characterData.characterStatsData;
        GameObject obj = Instantiate(character, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(SquadParent, false); // keep local position intact

        CharacterStats stats = obj.GetComponent<CharacterStats>();
        if (stats != null)
        {
            // Apply all main values
            stats.CharacterName = characterStatsData.CharacterName;

            stats.MainMaxHealth = characterStatsData.MainMaxHealth;
            stats.MainAttack = characterStatsData.MainAttack;
            stats.MainRange = characterStatsData.MainRange;
            stats.MainDefense = characterStatsData.MainDefense;
            stats.MainAttackCount = characterStatsData.MainAttackCount;
            stats.MainCriticalChance = characterStatsData.MainCriticalChance;
            stats.MainEvasionChance = characterStatsData.MainEvasionChance;
            stats.MainMovementRange = characterStatsData.MainMovementRange;

            // Runtime values
            stats.Level = characterStatsData.Level;
            stats.experience = characterStatsData.experience;
            stats.statPoints = characterStatsData.statPoints;
        }

        LoadGear(obj, characterData.gearData);
        stats.Initialize();
    }

    void LoadGear(GameObject character, GearData gearData)
    {
        if (gearData == null) return;

        GearEquipper gearEquipper = character.GetComponent<GearEquipper>();
        gearEquipper.equippedHelmet = SearchItem(gearData.HelmetName)?.loot.GetComponent<ItemPickUp>().itemData as WeaponData;
        gearEquipper.equippedWeapon = SearchItem(gearData.WeaponName)?.loot.GetComponent<ItemPickUp>().itemData as WeaponData;
        gearEquipper.equippedVest = SearchItem(gearData.ShirtName)?.loot.GetComponent<ItemPickUp>().itemData as WeaponData;
        gearEquipper.equippedTrouser = SearchItem(gearData.PantName)?.loot.GetComponent<ItemPickUp>().itemData as WeaponData;
        gearEquipper.equippedShoe = SearchItem(gearData.ShoeName)?.loot.GetComponent<ItemPickUp>().itemData as WeaponData;
        gearEquipper.LoadWeaponSprite();
    }

    LootEntry SearchItem(string ItemName)
    {
        return lootMaster.GetLootByName(ItemName);
    }

    void LoadInventoryData()
    {
        InventoryData inventoryData = SaveSystem.LoadInventoryData();
        if (inventoryData == null) return;

        foreach (InventoryItem inventoryItem in inventoryData.inventoryItems)
        {
            LootEntry result = SearchItem(inventoryItem.ItemName);
            if (result != null)
            {
                playerInventory.collectedItems.Add(new ItemInstance(result.loot.GetComponent<ItemPickUp>().itemData,inventoryItem.Quantity));
            }
        }
    }

    #region Save
    public void Save()
    {
        SaveSystem.Save(SquadMover, EventParent, hungerManager, SquadParent,playerInventory);
    }
    #endregion
}
