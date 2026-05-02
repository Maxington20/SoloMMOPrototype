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

    [Header("Resource")]
    [SerializeField] private int resourceCost = 0;
    [SerializeField] private int resourceGenerated = 0;

    [Header("Cast Rules")]
    [SerializeField] private AbilityCastType castType = AbilityCastType.Instant;
    [SerializeField] private float castTimeSeconds = 0f;
    [SerializeField] private float channelDurationSeconds = 0f;
    [SerializeField] private bool canMoveWhileCasting = false;
    [SerializeField] private bool canBeInterrupted = true;

    [Header("Effects")]
    [SerializeField] private int damageAmount = 0;
    [SerializeField] private int healthRestoreAmount = 0;

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public bool RequiresTarget => requiresTarget;
    public float Range => Mathf.Max(0f, range);
    public float CooldownSeconds => Mathf.Max(0f, cooldownSeconds);

    public int ResourceCost => Mathf.Max(0, resourceCost);
    public int ResourceGenerated => Mathf.Max(0, resourceGenerated);

    // Backward-compatible alias so older UI references do not break.
    public int ManaCost => ResourceCost;

    public AbilityCastType CastType => castType;

    public float CastTimeSeconds => castType == AbilityCastType.CastTime
        ? Mathf.Max(0f, castTimeSeconds)
        : 0f;

    public float ChannelDurationSeconds => castType == AbilityCastType.Channel
        ? Mathf.Max(0f, channelDurationSeconds)
        : 0f;

    public bool CanMoveWhileCasting => canMoveWhileCasting;
    public bool CanBeInterrupted => canBeInterrupted;

    public int DamageAmount => Mathf.Max(0, damageAmount);
    public int HealthRestoreAmount => Mathf.Max(0, healthRestoreAmount);

    public bool IsInstant => castType == AbilityCastType.Instant;
    public bool HasCastTime => castType == AbilityCastType.CastTime && CastTimeSeconds > 0f;
    public bool IsChannel => castType == AbilityCastType.Channel && ChannelDurationSeconds > 0f;
}