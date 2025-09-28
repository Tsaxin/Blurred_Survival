using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeTracker : MonoBehaviour
{
    public static GameModeTracker Instance;
    public int GameMode = 0; //0=new, 1=load

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
