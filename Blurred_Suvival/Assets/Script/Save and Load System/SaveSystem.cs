using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    private static string PlayerLocationFile = Path.Combine(Application.persistentDataPath, "PlayerLocation.dat");
    private static string EventFile = Path.Combine(Application.persistentDataPath, "Event.dat");
    private static string HungerFile = Path.Combine(Application.persistentDataPath, "Hunger.dat");
    private static string PlayerDataFile = Path.Combine(Application.persistentDataPath, "PlayerData.dat");
    private static string InventoryDataFile = Path.Combine(Application.persistentDataPath, "InventoryData.dat");

    // === MASTER SAVE ===
    public static void Save(Transform player, Transform eventParent, HungerManager hungerManager, Transform PlayerParent,PlayerInventory playerInventory)
    {
        SavePlayerLocation(player);
        SaveEventData(eventParent);
        SaveHungerData(hungerManager);
        SavePlayerData(PlayerParent);
        SaveInventoryData(playerInventory);

        Debug.Log("All game data saved!");
    }

    // === INDIVIDUAL SAVE FUNCTIONS ===
    public static void SavePlayerLocation(Transform player)
    {
        PlayerLocation data = new PlayerLocation(player);
        SaveGeneric(data, PlayerLocationFile);
    }

    public static void SaveEventData(Transform eventParent)
    {
        EventData data = new EventData(eventParent);
        SaveGeneric(data, EventFile);
    }

    public static void SaveHungerData(HungerManager hungerManager)
    {
        HungerData data = new HungerData(hungerManager);
        SaveGeneric(data, HungerFile);
    }

    public static void SavePlayerData(Transform Parent)
    {
        PlayerData data = new PlayerData(Parent);
        SaveGeneric(data, PlayerDataFile);
    }

    public static void SaveInventoryData(PlayerInventory playerInventory)
    {
        InventoryData data = new InventoryData(playerInventory);
        SaveGeneric(data,InventoryDataFile);
    }

    private static void SaveGeneric<T>(T data, string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            formatter.Serialize(stream, data);
        }

        Debug.Log(typeof(T).Name + " saved to " + filePath);
    }

    public static PlayerLocation LoadPlayerLocation()
    {
        return LoadData<PlayerLocation>(PlayerLocationFile);
    }
    public static EventData LoadEventData()
    {
        return LoadData<EventData>(EventFile);
    }

    public static HungerData LoadHungerData()
    {
        return LoadData<HungerData>(HungerFile);
    }
    public static PlayerData LoadPlayerData()
    {
        return LoadData<PlayerData>(PlayerDataFile);
    }

    public static InventoryData LoadInventoryData()
    {
        return LoadData<InventoryData>(InventoryDataFile);
    }

    public static T LoadData<T>(string filePath) where T : class
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                T data = formatter.Deserialize(stream) as T;
                return data;
            }
        }
        else
        {
            Debug.LogWarning("Save file not found in " + filePath);
            return null;
        }
    }

    public static void DeleteSaveData()
    {
        if (File.Exists(PlayerLocationFile))
        {
            File.Delete(PlayerLocationFile);
            Debug.Log("Save file deleted at " + PlayerLocationFile);
        }
        else
        {
            Debug.LogWarning("No save file found to delete.");
        }
    }
}

#region PlayerLocation
[System.Serializable]
public class PlayerLocation
{
    public float LocationX, LocationY;

    public PlayerLocation(Transform playerLocation)
    {
        LocationX = playerLocation.position.x;
        LocationY = playerLocation.position.y;
    }
}
#endregion

[System.Serializable]
public class EventData
{
    public List<BaseEventData> baseEventDatas = new List<BaseEventData>();

    public EventData(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            baseEventDatas.Add(new BaseEventData(parent.GetChild(i).GetComponent<EventTrigger>()));
        }
    }
}

[System.Serializable]
public class BaseEventData
{
    public string Name;
    public bool IsCompleted;
    public bool IsActive;

    public BaseEventData(EventTrigger eventTrigger)
    {
        Name = eventTrigger.gameObject.name;
        IsCompleted = eventTrigger.EventCompleted;
        IsActive = eventTrigger.gameObject.activeSelf;
    }
}

[System.Serializable]
public class HungerData
{
    public float currentHunger;

    public HungerData(HungerManager hungerManager)
    {
        this.currentHunger = hungerManager.currentHunger;
    }
}

[System.Serializable]
public class PlayerData
{
    public List<CharacterData> characterData=new List<CharacterData>();

    public PlayerData(Transform parent)
    {
        foreach (Transform child in parent)
        {
            characterData.Add(new CharacterData(child));
        }
    }
}

[System.Serializable]
public class CharacterData
{
    public string UniqueID;
    public CharacterStatsData characterStatsData;

    public GearData gearData;

    public CharacterData(Transform child)
    {
        UniqueID = child.GetComponent<CharacterDetail>().UniqueID;
        characterStatsData = new CharacterStatsData(child.GetComponent<CharacterStats>());
        gearData = new GearData(child.GetComponent<GearEquipper>());
    }
}

[System.Serializable]
public class CharacterStatsData
{
    public string CharacterName;

    // Base stats
    public int MainMaxHealth;
    public int MainAttack;
    public int MainRange;
    public float MainDefense;
    public int MainAttackCount;
    public float MainCriticalChance;
    public float MainEvasionChance;
    public int MainMovementRange;

    // Level & XP
    public int Level;
    public int experience;
    public int xpToLevelUp;
    public int statPoints;

    // Runtime
    public int currentHealth;
    public bool isDead;
    public bool isZombie;

    public CharacterStatsData(CharacterStats stats)
    {
        CharacterName = stats.CharacterName;
        MainMaxHealth = stats.MainMaxHealth;
        MainAttack = stats.MainAttack;
        MainRange = stats.MainRange;
        MainDefense = stats.MainDefense;
        MainAttackCount = stats.MainAttackCount;
        MainCriticalChance = stats.MainCriticalChance;
        MainEvasionChance = stats.MainEvasionChance;
        MainMovementRange = stats.MainMovementRange;

        Level = stats.Level;
        experience = stats.experience;
        xpToLevelUp = stats.xpToLevelUp;
        statPoints = stats.statPoints;

        currentHealth = stats.currentHealth;
        isDead = stats.IsDead;
        isZombie = stats.isZombie;
    }
}

[System.Serializable]
public class GearData
{
    public string WeaponName, HelmetName, ShirtName, PantName, ShoeName;

    public GearData(GearEquipper gearEquipper)
    {
        HelmetName = gearEquipper.equippedHelmet?.itemName;
        WeaponName = gearEquipper.equippedWeapon?.itemName;
        ShirtName = gearEquipper.equippedVest?.itemName;
        PantName = gearEquipper.equippedTrouser?.itemName;
        ShoeName = gearEquipper.equippedShoe?.itemName;
    }
}

[System.Serializable]
public class InventoryData
{
    public List<InventoryItem> inventoryItems = new List<InventoryItem>();

    public InventoryData(PlayerInventory playerInventory)
    {
        foreach (ItemInstance itemInstance in playerInventory.collectedItems)
        {
            inventoryItems.Add(new InventoryItem(itemInstance));
        }
    }
}

[System.Serializable]
public class InventoryItem
{
    public string ItemName;
    public int Quantity;

    public InventoryItem(ItemInstance itemInstance)
    {
        ItemName = itemInstance.data.itemName;
        Quantity = itemInstance.quantity;
    }
}