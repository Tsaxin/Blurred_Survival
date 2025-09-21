using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMasterEvent", menuName = "Events/Master Event")]
public class MasterEvent : ScriptableObject
{
    public List<Sprite> NormalIcon;
    public List<GameObject> Survivors;
    public List<GameObject> Vehicles;
    public List<Event> event2survivor = new List<Event>();

    public int MinLevel = 1, MaxLevel = 20;

    public Event GetRandomEvent(SpriteRenderer MapIcon)
    {
        return GetRandomEventFromEventSurvivor(MapIcon);
    }

    public Event GetRandomEventFromEventSurvivor(SpriteRenderer MapIcon)
    {
        MapIcon.sprite = NormalIcon[Random.Range(0, NormalIcon.Count)];

        // Get current scale
        Vector3 scale = MapIcon.transform.localScale;

        // Randomly pick -1 or 1
        scale.x = Random.value < 0.5f ? -1f : 1f;

        MapIcon.transform.localScale = scale;

        Event newEvent = event2survivor[Random.Range(0, event2survivor.Count)];

        List<GameObject> TempSurvivor = new List<GameObject>(Survivors);
        List<GameObject> SelectedSurvivor = new List<GameObject>();

        //this returns the number of survivor
        int RandomNumberOfSurvivor = NumberSettings.GetBiasedRandom(0, 5);
        for (int i = 0; i < 2; i++)
        {
            GameObject obj = TempSurvivor[Random.Range(0, TempSurvivor.Count)];
            obj.GetComponent<CharacterDetail>().SetDetail(GetRandomName(obj.GetComponent<CharacterDetail>()));
            SelectedSurvivor.Add(obj);
            TempSurvivor.Remove(obj);
        }

        newEvent.Survivors = new List<GameObject>(SelectedSurvivor);
        newEvent.NPCLevel = Random.Range(MinLevel, MaxLevel);
        return newEvent;
    }

    #region Name
    [Header("CSV Name Lists")]
    [TextArea] public string MaleName;   // Example: "John,Alex,Michael"
    [TextArea] public string FemaleName; // Example: "Sarah,Emma,Lisa"

    // ✅ Get random male name

    public string GetRandomName(CharacterDetail characterDetail)
    {
        if (characterDetail.gender == CharacterDetail.Gender.Male)
        {
            return GetRandomMaleName();
        }
        else
        {
            return GetRandomFemaleName();
        }
    }
    public string GetRandomMaleName()
    {
        return GetRandomNameFromCSV(MaleName);
    }

    // ✅ Get random female name
    public string GetRandomFemaleName()
    {
        return GetRandomNameFromCSV(FemaleName);
    }

    // ✅ Shared helper function
    private string GetRandomNameFromCSV(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return string.Empty;

        // Split by comma and trim whitespace
        string[] names = csv.Split(',');
        for (int i = 0; i < names.Length; i++)
        {
            names[i] = names[i].Trim();
        }

        // Pick a random name
        int index = Random.Range(0, names.Length);
        return names[index];
    }
    #endregion
}