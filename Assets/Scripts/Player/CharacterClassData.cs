using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Solo MMO/Character Class")]
public class CharacterClassData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string className = "New Class";
    [SerializeField] private CharacterRole role = CharacterRole.Damage;
    [SerializeField] private PrimaryStatType primaryStat = PrimaryStatType.Strength;

    [TextArea(3, 6)]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;

    [Header("Starting Stats")]
    [SerializeField] private int startingMaxHealth = 100;
    [SerializeField] private int startingBaseDamage = 20;
    [SerializeField] private StatBlock startingStats = new StatBlock(5, 5, 5, 5, 0, 95);

    [Header("Stats Gained Per Level")]
    [SerializeField] private StatBlock statsGainedPerLevel = new StatBlock(1, 1, 1, 1, 0, 0);

    [Header("Weapon Rules")]
    [SerializeField] private WeaponType[] allowedMainHandTypes = new WeaponType[0];
    [SerializeField] private WeaponType[] allowedOffhandTypes = new WeaponType[0];
    [SerializeField] private bool canDualWieldWeapons = false;

    [Header("Starting Abilities")]
    [SerializeField] private AbilityData[] startingAbilities = new AbilityData[6];

    public string ClassName => className;
    public CharacterRole Role => role;
    public PrimaryStatType PrimaryStat => primaryStat;
    public string Description => description;
    public Sprite Icon => icon;
    public int StartingMaxHealth => Mathf.Max(1, startingMaxHealth);
    public int StartingBaseDamage => Mathf.Max(0, startingBaseDamage);
    public StatBlock StartingStats => startingStats ?? StatBlock.Zero;
    public StatBlock StatsGainedPerLevel => statsGainedPerLevel ?? StatBlock.Zero;
    public AbilityData[] StartingAbilities => startingAbilities;
    public bool CanDualWieldWeapons => canDualWieldWeapons;

    public bool CanUseMainHandWeaponType(WeaponType weaponType)
    {
        return ContainsWeaponType(allowedMainHandTypes, weaponType);
    }

    public bool CanUseOffhandWeaponType(WeaponType weaponType)
    {
        return ContainsWeaponType(allowedOffhandTypes, weaponType);
    }

    public StatBlock GetLevelBonusStats(int level)
    {
        int levelsGained = Mathf.Max(0, level - 1);
        return StatsGainedPerLevel.Multiply(levelsGained);
    }

    private bool ContainsWeaponType(WeaponType[] allowedTypes, WeaponType weaponType)
    {
        if (weaponType == WeaponType.None)
        {
            return true;
        }

        if (allowedTypes == null || allowedTypes.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < allowedTypes.Length; i++)
        {
            if (allowedTypes[i] == weaponType)
            {
                return true;
            }
        }

        return false;
    }
}