using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;
    public TextMeshProUGUI tooltipText;

    public TextMeshProUGUI Name;
    private Vector2 padding = new Vector2(10f, 10f);

    public GameObject TooltipHolder;

    void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    void Update()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        Vector2 position = Input.mousePosition;
        transform.position = position;
    }

    public void ShowTooltip(string name, string description)
    {
        TooltipHolder.SetActive(true);
        Name.text = name;
        tooltipText.text = description;

        tooltipText.ForceMeshUpdate();
    }


    public void HideTooltip()
    {
        TooltipHolder.SetActive(false);
    }
}
