using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;

public class CharacterStats : MonoBehaviour
{
    public string CharacterName;
    [Header("Base Stats")]
    public int MainMaxHealth = 10;
    public int MainAttack = 2;
    public int MainRange = 1;
    public float MainDefense = 1;
    public int MainAttackCount = 1;

    public float MainCriticalChance = 1;

    public float MainEvasionChance = 1;

    public int attack;
    public int range;
    public int maxHealth;
    public float Defense;
    public int AttackCount;
    public float CriticalChance;
    public float EvasionChance;

    [Header("Movement")]
    public int MainMovementRange = 1; // Number of tiles the character can move
    public int MovementRange;
    public float chaseRange = 5f;
    public int knockTile = 1;

    [Header("Experience")]
    public int experience = 0;
    public int xpPerKill = 10;
    public int Level = 1;
    public int xpToLevelUp = 20;

    [Header("XP UI")]
    public Slider XPSlider;
    public TextMeshProUGUI levelText;

    [Header("Runtime State (Read Only)")]
    [SerializeField] private int _currentHealth;
    public int currentHealth => _currentHealth;

    [SerializeField] private bool _isDead;
    public bool IsDead => _isDead;

    [Header("Health UI")]
    public GameObject healthBarCanvas;
    public Slider healthSlider;

    [Header("Movement Settings")]
    public bool isZombie = false;
    public float moveSpeed = 7f;

    GearEquipper gearEquipper;

    void Awake()
    {
        _isDead = false;

        attack = MainAttack;
        range = MainRange;
        MovementRange = MainMovementRange;
        Defense = MainDefense;
        AttackCount = MainAttackCount;
        maxHealth = MainMaxHealth;
        CriticalChance = MainCriticalChance;
        EvasionChance = MainEvasionChance;

        gearEquipper = GetComponent<GearEquipper>();
        RecalculateStats();
        _currentHealth = maxHealth;

        UpdateHealthSlider();

        UpdateXPUI();
    }

    void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = _currentHealth;
            UpdateHealthBarVisibility();
        }
    }

    public int TakeDamage(int amount, CharacterStats attacker = null)
    {
        if (_isDead) return 0;

        // 🌟 Evasion check
        float evasionRoll = Random.Range(0, 100f);
        if (evasionRoll < EvasionChance)
        {
            Debug.Log($"{name} evaded the attack!");
            if (DamageTextManager.Instance != null)
                DamageTextManager.Instance.ShowDamage(transform.position, "Miss", true);
            return 0;
        }

        bool isCrit = false; // 🔴 track crit state

        // 🌟 Handle crits
        if (attacker != null && attacker.CriticalChance > 0)
        {
            int critRoll = Random.Range(0, 100);
            if (critRoll < attacker.CriticalChance)
            {
                isCrit = true; // 🔴 mark as crit

                if (isZombie)
                {
                    // Headshot zombie → insta-kill
                    Kill(attacker);
                    if (DamageTextManager.Instance != null)
                        DamageTextManager.Instance.ShowDamage(transform.position, "HeadShot", false);

                    if (healthSlider != null)
                    {
                        healthSlider.value = _currentHealth;
                        UpdateHealthBarVisibility();
                    }

                    // 🔴 Huge blood for zombie headshot
                    BloodPool.Instance.SpawnHuge(this.transform);
                }
                else
                {
                    amount *= 2; // Double damage for non-zombie crit
                }
            }
        }

        // Assuming Defense is a percentage value from 0 to 100
        float defensePercent = Defense / 100f;          // convert to 0-1
        float reducedAmount = amount * (1f - defensePercent);  // reduce by percent
        int damageTaken = Mathf.Max(1, Mathf.RoundToInt(reducedAmount)); // minimum 1

        _currentHealth -= damageTaken;

        if (damageTaken > 0 && DamageTextManager.Instance != null)
            DamageTextManager.Instance.ShowDamage(transform.position, damageTaken.ToString(), false);

        Debug.Log($"{name} took {damageTaken} damage. Current HP: {_currentHealth}");

        if (healthSlider != null)
        {
            healthSlider.value = _currentHealth;
            UpdateHealthBarVisibility();
        }

        if (_currentHealth <= 0 && !_isDead)
        {
            _isDead = true;

            // 🔴 Huge blood on death
            BloodPool.Instance.SpawnHuge(this.transform);

            Die(attacker);
        }
        else
        {
            if (isCrit)
            {
                // 🔴 Huge blood on crit
                BloodPool.Instance.SpawnHuge(this.transform);
            }
            else
            {
                // Normal blood on regular hit
                BloodPool.Instance.SpawnBlood(this.transform);
            }
        }
        return amount;
    }


    public void Kill(CharacterStats killer = null)
    {
        if (_isDead) return;

        _currentHealth = 0;
        _isDead = true;
        Die(killer);
    }

    public void Heal(int amount)
    {
        if (_isDead) return;

        int healedAmount = Mathf.Min(amount, maxHealth - _currentHealth);

        if (DamageTextManager.Instance != null)
        {
            DamageTextManager.Instance.ShowHeal(transform.position, amount);
        }

        if (healedAmount > 0)
        {
            _currentHealth += healedAmount;

            if (healthSlider != null)
            {
                healthSlider.value = _currentHealth;
                UpdateHealthBarVisibility();
            }

            Debug.Log("Heal applied: " + healedAmount);
        }
        else
        {
            // No health added because already full
            if (healthSlider != null)
            {
                UpdateHealthBarVisibility();
            }
        }
    }

    private void UpdateHealthBarVisibility()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(_currentHealth < maxHealth);
        }
    }

    private void Die(CharacterStats attacker)
    {
        Debug.Log($"{name} died.");

        if (attacker != null)
        {
            attacker.GainXP(xpPerKill);

            if (DamageTextManager.Instance != null)
            {
                DamageTextManager.Instance.ShowXP(attacker.transform.position, xpPerKill);
            }
        }

        // Inform TurnManager if it's an enemy
        if (isZombie)
        {
            TurnManager tm = FindObjectOfType<TurnManager>();
            if (tm != null)
            {
                tm.RemoveEnemy(GetComponent<ZombieAIBase>());
            }

            GetComponent<ZombieAIBase>().EnemyManager.SpawnZombie(CharacterName);
            if (GetComponent<ZombieLootDropper>() != null)
            {
                GetComponent<ZombieLootDropper>().DropLoot(transform.position, GetComponent<ZombieAIBase>().currentTileData);
            }
        }
        else if (gameObject.CompareTag("Player"))
        {
            // Notify squad for game over check
            Squad squad = FindObjectOfType<Squad>();
            if (squad != null)
            {
                squad.CheckIfAllPlayersDead();

                squad.RemoveCharacter(this.gameObject);
            }

            // Notify TurnManager that this player is finished with their move
            TurnManager tm = FindObjectOfType<TurnManager>();
            if (tm != null)
            {
                CharacterController controller = GetComponent<CharacterController>();
                if (controller != null)
                {
                    tm.OnPlayerFinishedMove(controller);
                }
            }
        }
        else
        {
            TurnManager tm = FindObjectOfType<TurnManager>();
            if (tm != null)
            {
                tm.RemoveEnemy(GetComponent<ZombieAIBase>());
            }
            // ✅ Handle generic enemies (non-zombie, non-player)
            Debug.Log($"{name} is a generic NPC and is now dead. Destroying.");
        }

        Destroy(gameObject);
    }

    public void GainXP(int amount)
    {
        experience += amount;
        Debug.Log($"{gameObject.name} gained {amount} XP! Total: {experience}");

        while (experience >= xpToLevelUp)
        {
            experience -= xpToLevelUp;
            LevelUp();
        }

        UpdateXPUI();
    }

    public int statPoints = 0;

    private void LevelUp()
    {
        Level++;
        xpToLevelUp = 20 + (Level - 1) * 10;

        // Instead of auto-increasing stats, give 1 point to spend
        statPoints++;

        Debug.Log($"{gameObject.name} leveled up to {Level}! Unspent points: {statPoints}");

        // Keep HP full on level up
        _currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = _currentHealth;
        }

        // No auto stat changes here!
        RecalculateStats();
    }


    private void UpdateXPUI()
    {
        if (XPSlider != null)
        {
            XPSlider.maxValue = xpToLevelUp;
            XPSlider.value = experience;
        }

        if (levelText != null)
        {
            levelText.text = $"Lv {Level}";
        }
    }

    public void ScaleStatsByLevel()
    {
        MainAttack += (Level - 1) * 1;
        MainMaxHealth += (Level - 1) * 3;
        MainDefense += Mathf.FloorToInt((Level - 1) * 0.3f); // Slower scaling

        attack = MainAttack;
        maxHealth = MainMaxHealth;
        MainDefense = Defense;

        _currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = _currentHealth;
            UpdateHealthBarVisibility();
        }
        xpPerKill = xpPerKill + (Level - 1) * 5;
        xpToLevelUp = 20 + (Level - 1) * 10;

        levelText.text = $"Lv {Level}";

        Debug.Log($"{name} scaled to level {Level}: HP {maxHealth}, ATK {attack}, DEF {Defense}");
    }

    public void RecalculateStats()
    {
        float debuff = HungerManager.Instance.DebuffAmount;

        // Collect total modifiers from all equipped gear
        StatModifier totalModifier = new StatModifier();
        if (gearEquipper != null)
        {
            foreach (var mod in gearEquipper.GetAllModifiers())
            {
                totalModifier += mod;
            }
        }

        // Base stats + gear + debuff
        attack = Mathf.Max(1, Mathf.CeilToInt((MainAttack + totalModifier.attack) * debuff));
        range = MainRange + totalModifier.range;
        Defense = (MainDefense + totalModifier.defense) * debuff;
        AttackCount = MainAttackCount + totalModifier.attackCount;
        maxHealth = Mathf.Max(1, Mathf.CeilToInt((MainMaxHealth + totalModifier.health) * debuff));
        MovementRange = MainMovementRange + totalModifier.movement;
        CriticalChance = MainCriticalChance + totalModifier.critical;
        EvasionChance = Mathf.Max(0f, (MainEvasionChance + totalModifier.evasion) * debuff);

        // Keep current health proportional
        float healthPercent = (float)_currentHealth / Mathf.Max(1, healthSlider?.maxValue ?? MainMaxHealth);
        _currentHealth = Mathf.Clamp(Mathf.CeilToInt(maxHealth * healthPercent), 1, maxHealth);

        UpdateHealthSlider();
    }

    public void ModifyStats(float multiplier)
    {
        if (multiplier <= 1 || HungerManager.Instance.isDebuffed)
        {
            // Keep the current health % before scaling
            float healthPercent = (float)_currentHealth / maxHealth;

            attack = Mathf.Max(1, Mathf.CeilToInt(attack * multiplier));
            maxHealth = Mathf.Max(1, Mathf.CeilToInt(maxHealth * multiplier));

            // Restore health proportionally
            _currentHealth = Mathf.Max(1, Mathf.CeilToInt(maxHealth * healthPercent));

            Defense *= multiplier;
            EvasionChance *= multiplier;
        }
    }

}
