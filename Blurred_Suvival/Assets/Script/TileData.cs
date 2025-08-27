using UnityEngine;
using System.Collections.Generic;

public class TileData : MonoBehaviour
{
    public GameObject occupant;
    public bool IsOccupied => occupant != null;

    private SpriteRenderer sr;
    private Color originalColor;

    public Color possibleMoveColor = Color.cyan;
    public Color rangeColor;

    public float MaxLootOffsetX = 0.05f, MinLootOffsetX = 0.04f, MaxLootOffsetY = 0.04f, MinLootOffsetY = -0.07f;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
        }

        DestroyAllLoot();
    }

    public void AssignOccupant(GameObject character)
    {
        occupant = character;
    }

    public void ClearOccupant()
    {
        occupant = null;
    }

    public void ShowAsPossibleMove()
    {
        if (sr != null)
            sr.color = possibleMoveColor;
    }

    public void ShowRangeColor()
    {
        if (sr != null)
            sr.color = rangeColor;
    }

    public void ResetColor()
    {
        if (sr != null)
            sr.color = originalColor;
    }

    // ------------------ ✅ MULTIPLE LOOT SUPPORT ------------------

    [Header("Loot")]
    public List<GameObject> lootOnTile = new List<GameObject>();

    public bool HasLoot => lootOnTile.Count > 0;

    public void PlaceLoot(GameObject loot)
    {
        lootOnTile.Add(loot);
        loot.transform.SetParent(transform);

        // Optional: Apply slight random offset for visual separation
        float offsetX = Random.Range(MinLootOffsetX, MaxLootOffsetX);
        float offsetY = Random.Range(MinLootOffsetY, MaxLootOffsetY);
        loot.transform.localPosition = new Vector3(offsetX, offsetY, 0);
    }

    public void TryCollectLoot()
    {
        // Use ToArray to avoid modifying list while iterating
        foreach (var loot in lootOnTile.ToArray())
        {
            ItemPickUp pickup = loot.GetComponent<ItemPickUp>();
            if (pickup != null)
            {
                bool added = PlayerInventory.Instance.AddItem(pickup.itemData);
                if (added)
                {
                    lootOnTile.Remove(loot); // remove from tile
                    Destroy(loot);           // cleanup
                }
            }
        }
    }

    public void DestroyAllLoot()
    {
        foreach (GameObject loot in lootOnTile)
        {
            if (loot != null)
                Destroy(loot);
        }
        lootOnTile.Clear();
    }
}
