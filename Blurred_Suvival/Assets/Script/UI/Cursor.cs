using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        SetDefaultCursorTexture();
    }
    #region Cursor
    [SerializeField]
    Texture2D DefaultCursorTexture;

    public void SetDefaultCursorTexture()
    {
        UnityEngine.Cursor.SetCursor(DefaultCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    #endregion
}
