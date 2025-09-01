using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionSprite : MonoBehaviour
{
    [ContextMenu("Enable Region Sprite")]
    public void EnableRegionSprite()
    {
        SpriteState(true);
    }
    [ContextMenu("Disable Region Sprite")]
    public void DisableRegionSprite()
    {
        SpriteState(false);
    }
    
    public void SpriteState(bool value)
    {
        foreach (Transform child in transform)
        {
            child.GetComponentInChildren<SpriteRenderer>().enabled = value;
        }
    }
}
