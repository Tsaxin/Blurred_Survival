using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float floatSpeed = 30f;
    public float lifetime = 1.5f;
    public float yOffset = 1.5f; // 👈 Offset above character's head

    private Vector3 moveDirection = Vector3.up;

    void Start()
    {
        transform.position += new Vector3(0f, yOffset, 0f); // Apply vertical offset
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += moveDirection * floatSpeed * Time.deltaTime;
    }

    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }

    public void SetColor(Color color)
    {
        if (textMesh != null)
        {
            textMesh.color = color;
        }
    }

}
