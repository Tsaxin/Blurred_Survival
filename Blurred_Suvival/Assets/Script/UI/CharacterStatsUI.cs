using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStatsUI : MonoBehaviour
{
    [Header("References")]
    public GameObject StatsPanel;
    public TextMeshProUGUI CharacterName, Level;
    public TextMeshProUGUI remainingPointsText; // ✅ New: Shows available points

    [Header("Stat Upgrade Buttons")]
    public Button attackButton;
    public Button hpButton;
    public Button defenseButton;
    public Button critButton;
    public Button evasionButton;

    [Header("Points Settings")]
    public int attackIncreasePerPoint = 1;
    public int hpIncreasePerPoint = 5;
    public float defenseIncreasePerPoint = .5f;
    public float critIncreasePerPoint = .5f;
    public float evasionIncreasePerPoint = .5f;

    public static CharacterStatsUI Instance;

    CharacterStats targetStats;

    [System.Serializable]
    public class StatSlider
    {
        public Slider baseSlider;    // Grey part
        public Slider boostedSlider; // Yellow part
        public int maxValue;         // Max possible for this stat
    }

    [Header("Stat Sliders")]
    public StatSlider attackStat;
    public StatSlider hpStat;
    public StatSlider defenseStat;
    public StatSlider critStat;
    public StatSlider evasionStat;

    [Header("TextMeshProUGUI")]
    public TextMeshProUGUI AttackValue;
    public TextMeshProUGUI HPValue, DefenseValue, CriticalValue, EvasionValue;

    public Image CharacterSprite;

    void Start()
    {
        if (Instance == null) Instance = this;
    }

    public void UpdateStatsUI(CharacterStats targetStats)
    {
        if (targetStats == null) return;
        
        targetStats.RecalculateStats();

        SetStat(attackStat, targetStats.MainAttack, targetStats.attack,AttackValue);
        SetStat(hpStat, targetStats.MainMaxHealth, targetStats.maxHealth,HPValue);
        SetStat(defenseStat, targetStats.MainDefense, targetStats.Defense,DefenseValue);
        SetStat(critStat, targetStats.MainCriticalChance, targetStats.CriticalChance,CriticalValue);
        SetStat(evasionStat, targetStats.MainEvasionChance, targetStats.EvasionChance,EvasionValue);
    }

    private void SetStat(StatSlider statUI, int baseValue, int boostedValue, TextMeshProUGUI ValueHolder)
    {
        statUI.baseSlider.maxValue = statUI.maxValue;
        statUI.boostedSlider.maxValue = statUI.maxValue;

        statUI.baseSlider.value = baseValue;
        statUI.boostedSlider.value = boostedValue;

        ValueHolder.text = boostedValue.ToString();
    }
    private void SetStat(StatSlider statUI, float baseValue, float boostedValue, TextMeshProUGUI ValueHolder)
    {
        statUI.baseSlider.maxValue = statUI.maxValue;
        statUI.boostedSlider.maxValue = statUI.maxValue;

        statUI.baseSlider.value = baseValue;
        statUI.boostedSlider.value = boostedValue;

        ValueHolder.text = boostedValue.ToString()+"%";
    }

    public void OpenStatPanel(CharacterStats targetStats, Sprite CharacterSprite)
    {
        this.CharacterSprite.sprite = CharacterSprite;
        this.CharacterSprite.SetNativeSize();

        RectTransform rt = this.CharacterSprite.GetComponent<RectTransform>();
        rt.sizeDelta = rt.sizeDelta /2.1f;
        this.targetStats = targetStats;
        StatsPanel.SetActive(true);
        UpdateStatsUI(targetStats);
        CharacterName.text = targetStats.CharacterName;
        Level.text = "Level " + targetStats.Level.ToString();
        UpdatePointsUI();
    }

    public void CloseStatPanel()
    {
        StatsPanel.SetActive(false);
    }

    // ✅ Updates the UI for remaining points
    public void UpdatePointsUI()
    {
        remainingPointsText.text = $"You have <color=green>{targetStats.statPoints}</color> available points";
        CheckStatButtons();
    }

    // ✅ Enables/disables all upgrade buttons based on points
    private void CheckStatButtons()
    {
        bool canUpgrade = targetStats.statPoints > 0;
        attackButton.interactable = canUpgrade;
        hpButton.interactable = canUpgrade;
        defenseButton.interactable = canUpgrade;
        critButton.interactable = canUpgrade;
        evasionButton.interactable = canUpgrade;
    }

    // ✅ Functions to be linked to buttons in Inspector
    public void IncreaseAttack()
    {
        if (targetStats.statPoints <= 0) return;
        targetStats.MainAttack += attackIncreasePerPoint;
        targetStats.statPoints--;
        UpdateStatsUI(targetStats);
        UpdatePointsUI();
    }

    public void IncreaseHP()
    {
        if (targetStats.statPoints <= 0) return;
        targetStats.MainMaxHealth += hpIncreasePerPoint;
        targetStats.statPoints--;
        UpdateStatsUI(targetStats);
        UpdatePointsUI();
    }

    public void IncreaseDefense()
    {
        if (targetStats.statPoints <= 0) return;
        targetStats.MainDefense += defenseIncreasePerPoint;
        targetStats.statPoints--;
        UpdateStatsUI(targetStats);
        UpdatePointsUI();
    }

    public void IncreaseCrit()
    {
        if (targetStats.statPoints <= 0) return;
        targetStats.MainCriticalChance += critIncreasePerPoint;
        targetStats.statPoints--;
        UpdateStatsUI(targetStats);
        UpdatePointsUI();
    }

    public void IncreaseEvasion()
    {
        if (targetStats.statPoints <= 0) return;
        targetStats.MainEvasionChance += evasionIncreasePerPoint;
        targetStats.statPoints--;
        UpdateStatsUI(targetStats);
        UpdatePointsUI();
    }
}
