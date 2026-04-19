using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerProgression : MonoBehaviour
{
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int startingXp = 0;
    [SerializeField] private int baseXpToNextLevel = 100;
    [SerializeField] private int xpGrowthPerLevel = 50;

    [Header("Level Rewards")]
    [SerializeField] private int healthGainPerLevel = 20;
    [SerializeField] private int damageGainPerLevel = 5;

    private Health playerHealth;
    private PlayerCombat playerCombat;

    public int Level { get; private set; }
    public int CurrentXp { get; private set; }
    public int XpToNextLevel { get; private set; }

    public float XpPercent => XpToNextLevel <= 0 ? 0f : (float)CurrentXp / XpToNextLevel;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        playerCombat = GetComponent<PlayerCombat>();

        Level = startingLevel;
        CurrentXp = startingXp;
        XpToNextLevel = CalculateXpNeededForLevel(Level);
    }

    public void AddXp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentXp += amount;

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You gain {amount} XP.");
        }

        while (CurrentXp >= XpToNextLevel)
        {
            CurrentXp -= XpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;

        playerHealth.IncreaseMaxHealth(healthGainPerLevel, true);
        playerCombat.IncreaseDamage(damageGainPerLevel);

        XpToNextLevel = CalculateXpNeededForLevel(Level);

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You reached level {Level}!");
        }

        Debug.Log($"Player leveled up to {Level}");
    }

    private int CalculateXpNeededForLevel(int level)
    {
        return baseXpToNextLevel + ((level - 1) * xpGrowthPerLevel);
    }
}