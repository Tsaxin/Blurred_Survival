using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class HungerManager : MonoBehaviour
{
    [Header("Hunger Settings")]
    public float maxHunger = 100f;
    public float currentHunger;

    public float baseDecayRate = 10f; // hunger per minute when idle
    public float movementDecayMultiplier = 2f;

    [Header("UI")]
    public Slider hungerBarSlider;

    private SquadMover squadMover;
    public Squad squad;
    public bool isDebuffed = false;

    public static HungerManager Instance;

    [Header("Debuff Settings")]
    public float MainDebuffAmount = 0.5f; // halve stats
    public float DebuffAmount = 1f;       // current multiplier

    public Animator DebuffPanel;

    public bool IsPaused;

    void Start()
    {
        IsPaused = false;
        if (Instance == null) Instance = this;
        squadMover = GetComponent<SquadMover>();

        DebuffAmount = 1f;

        if (hungerBarSlider != null)
        {
            hungerBarSlider.maxValue = maxHunger;
            hungerBarSlider.value = currentHunger;
        }
    }

    void Update()
    {
        if (!IsPaused)
        {
            // Tooltip info
            hungerBarSlider.GetComponent<TooltipTrigger>().SetTriggerText(
                "Supplies",
                $"Rations: {(int)currentHunger}/{maxHunger.ToSafeString()}\n" +
                $"Decay Rate: {CalculateDecayRate()}\n" +
                $"Movement Decay Rate:{(int)(baseDecayRate * squad.Characters.Count * movementDecayMultiplier)}\n\n" +
                $"Ration decay rate increases with squad size."
            );

            if (currentHunger <= 0f)
            {
                if (!isDebuffed)
                {
                    ApplyDebuff();
                }
                return;
            }

            float actualDecayRate = CalculateDecayRate();

            // Apply hunger decay
            currentHunger -= actualDecayRate * Time.deltaTime / 60f;
            currentHunger = Mathf.Clamp(currentHunger, 0f, maxHunger);

            if (hungerBarSlider != null)
            {
                hungerBarSlider.value = currentHunger;
            }
        }
    }

    void ApplyDebuff()
    {
        DebuffPanel.SetBool("Show", true);
        isDebuffed = true;
        DebuffAmount = MainDebuffAmount;
        ApplyDebufToAll();
        Debug.Log("⚠️ Hunger debuff applied!");
    }

    public void RestoreHunger(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0f, maxHunger);

        if (currentHunger > 0f && isDebuffed)
        {
            RemoveDebuff();
        }
    }

    void RemoveDebuff()
    {
        DebuffPanel.SetBool("Show", false);
        DebuffAmount = 1f;
        ApplyDebufToAll();
        isDebuffed = false;
        Debug.Log("✅ Hunger debuff removed, stats restored!");
    }

    private float CalculateDecayRate()
    {
        float actualDecayRate = baseDecayRate * squad.Characters.Count;

        if (squadMover != null && squadMover.IsMoving())
        {
            actualDecayRate *= movementDecayMultiplier;
        }

        return actualDecayRate;
    }

    public void ApplyDebufToAll()
    {
        foreach (GameObject obj in squad.Characters)
        {
            var stats = obj.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.RecalculateStats(); // recalc with latest DebuffAmount
            }
        }
    }
}
