using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGround : MonoBehaviour
{
    public float OffsetX, OffsetY;

    public void LoadBattleGround(Transform Parent)
    {
        transform.position = new Vector3(Parent.position.x+OffsetX,Parent.position.y+OffsetY,Parent.position.z);
    }
}
