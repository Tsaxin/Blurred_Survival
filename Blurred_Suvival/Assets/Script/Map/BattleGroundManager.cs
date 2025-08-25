using UnityEngine;

public class BattleGroundManager : MonoBehaviour
{
    public static BattleGroundManager Instance;

    public Transform BattleGroundHolder, BattleGroundPool;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void LoadMap(GameObject map)
    {
        map.transform.SetParent(BattleGroundHolder);
        map.GetComponent<BattleGround>().LoadBattleGround(BattleGroundHolder);
    }

    public void RemoveMap()
    {
        if(BattleGroundHolder.childCount>0)
            BattleGroundHolder.GetChild(0).transform.SetParent(BattleGroundPool);
    }
}
