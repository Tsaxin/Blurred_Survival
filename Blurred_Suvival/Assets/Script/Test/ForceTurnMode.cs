using UnityEngine;

public class ForceTurnMode : MonoBehaviour
{
    public Squad squad;

    public bool TestMode = false;

    public static ForceTurnMode Instance;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        Application.targetFrameRate = 60;
        squad.RequestBattle(false, null, null, false);;
    }
}
