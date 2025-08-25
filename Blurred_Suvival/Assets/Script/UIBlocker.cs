using UnityEngine.EventSystems;

public static class UIBlocker
{
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return true;
            }
        }
#else
        if (EventSystem.current.IsPointerOverGameObject())
            return true;
#endif

        return false;
    }
}
