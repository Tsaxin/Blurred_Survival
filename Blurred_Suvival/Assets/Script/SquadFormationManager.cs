using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SquadFormationManager : MonoBehaviour
{
    public static SquadFormationManager Instance { get; private set; }

    // Maps character name to their current tile
    public Dictionary<string, TileData> savedFormation = new Dictionary<string, TileData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Call this from the button
    public void SaveFormation()
    {
        savedFormation.Clear();

        foreach (Transform child in transform)
        {
            CharacterController controller = child.GetComponent<CharacterController>();
            if (controller != null && controller.currentTileData != null)
            {
                savedFormation[child.name] = controller.currentTileData;
                Debug.Log($"Saved {child.name} at tile {controller.currentTileData.name}");
            }
        }
    }

    public bool CheckBornLeader(List<GameObject> Characters)
    {
        bool hasBornLeader = Characters.Any(character =>
            {
                var cp = character.GetComponent<CharacterPassive>();
                if (cp == null) return false;

                return cp.passiveSkills.Any(passive => passive != null && passive.passiveSkill.skillName == "Born Leader");
            });

        return hasBornLeader;
    }
}
