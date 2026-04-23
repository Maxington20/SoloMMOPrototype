using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Solo MMO/Ability")]
public class AbilityData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string displayName = "New Ability";
    [SerializeField] private Sprite icon;
    [SerializeField] private float cooldownSeconds = 3f;

    [Header("Targeting")]
    [SerializeField] private bool requiresTarget = true;
    [SerializeField] private float range = 2.5f;

    [Header("Effects")]
    [SerializeField] private int damageAmount = 0;
    [SerializeField] private int healthRestoreAmount = 0;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);
    public bool RequiresTarget => requiresTarget;
    public float Range => Mathf.Max(0f, range);
    public int DamageAmount => Mathf.Max(0, damageAmount);
    public int HealthRestoreAmount => Mathf.Max(0, healthRestoreAmount);
}