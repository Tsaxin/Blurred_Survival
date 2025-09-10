using UnityEngine;

public class ForceAnimation : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnDisable()
    {
        if (anim != null)
        {
            // Reset the Animator so it doesn't freeze in a weird pose
            anim.Rebind();
            anim.Update(0f);
        }
    }
}
