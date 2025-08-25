using UnityEngine;

public class ClickDetector : MonoBehaviour
{
    // Optional: assign your parent object with CharacterController here
    public CharacterController characterController;

    void OnMouseDown()
    {
        //characterController.MouseDown();
        characterController.OnClicked();
    }
}
