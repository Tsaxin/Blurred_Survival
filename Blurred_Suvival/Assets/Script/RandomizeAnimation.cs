
using UnityEngine;

public class RandomizeAnimation : MonoBehaviour
{
    public float MaxScale,MinScale;
    // Start is called before the first frame update
    void OnEnable()
    {
        Animator animator = GetComponent<Animator>();

        // Randomize idle animation start point (if it's already the default state)
        animator.Play(0, 0, Random.value);

        // Optional: Add slight speed variation
        animator.speed = Random.Range(0.9f, 1.1f);
    }

    [ContextMenu("Randomize Scale")]
    void RandomizeScale()
    {
        int Scale = Random.Range(0, 2);
        float Rand = Random.Range(MinScale, MaxScale);
        if (Scale == 0)
        {
            Scale = -1;
        }
        transform.localScale = new Vector2(Scale*Rand, Rand);
    }
}
