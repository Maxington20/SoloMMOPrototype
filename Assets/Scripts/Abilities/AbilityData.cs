using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Solo MMO/Ability")]
public class AbilityData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string displayName = "New Ability";
    [TextArea(2, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("Use Rules")]
    [SerializeField] private bool requiresTarget = true;
    [SerializeField] private float range = 2.5f;
    [SerializeField] private float cooldownSeconds = 2f;
    [SerializeField] private int manaCost = 0;

    [Header("Effects")]
    [SerializeField] private int damageAmount = 0;
    [SerializeField] private int healthRestoreAmount = 0;

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public bool RequiresTarget => requiresTarget;
    public float Range => Mathf.Max(0f, range);
    public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);
    public int ManaCost => Mathf.Max(0, manaCost);

    public int DamageAmount => Mathf.Max(0, damageAmount);
    public int HealthRestoreAmount => Mathf.Max(0, healthRestoreAmount);
}