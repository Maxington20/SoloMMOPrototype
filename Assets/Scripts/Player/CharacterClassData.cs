using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Solo MMO/Character Class")]
public class CharacterClassData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string className = "New Class";
    [SerializeField] private CharacterRole role = CharacterRole.Damage;

    [TextArea(3, 6)]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;

    [Header("Starting Stats")]
    [SerializeField] private int startingMaxHealth = 100;
    [SerializeField] private int startingBaseDamage = 20;

    [Header("Weapon Rules")]
    [SerializeField] private WeaponType[] allowedMainHandTypes = new WeaponType[0];
    [SerializeField] private WeaponType[] allowedOffhandTypes = new WeaponType[0];
    [SerializeField] private bool canDualWieldWeapons = false;

    [Header("Starting Abilities")]
    [SerializeField] private AbilityData[] startingAbilities = new AbilityData[6];

    public string ClassName => className;
    public CharacterRole Role => role;
    public string Description => description;
    public Sprite Icon => icon;
    public int StartingMaxHealth => Mathf.Max(1, startingMaxHealth);
    public int StartingBaseDamage => Mathf.Max(0, startingBaseDamage);
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