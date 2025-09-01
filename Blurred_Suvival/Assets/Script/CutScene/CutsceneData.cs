using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CutsceneEntry
{
    public Sprite image;       // The cutscene image
    [TextArea(2, 4)]
    public string text;        // Text to display with the image
}

[System.Serializable]
public class Cutscene
{
    public string cutsceneID;             // Unique ID (e.g. "Intro", "BossFight")
    public List<CutsceneEntry> entries;   // Each step of the cutscene
}

[CreateAssetMenu(fileName = "CutsceneData", menuName = "Cutscenes/Cutscene Database")]
public class CutsceneData : ScriptableObject
{
    public List<Cutscene> cutscenes;

    public Cutscene GetCutsceneByID(string id)
    {
        return cutscenes.Find(c => c.cutsceneID == id);
    }
}
