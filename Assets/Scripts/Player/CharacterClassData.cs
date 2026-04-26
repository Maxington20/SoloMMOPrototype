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
    [SerializeField] private WeaponType[] allowedWeaponTypes = new WeaponType[0];

    [Header("Starting Abilities")]
    [SerializeField] private AbilityData[] startingAbilities = new AbilityData[6];

    public string ClassName => className;
    public CharacterRole Role => role;
    public string Description => description;
    public Sprite Icon => icon;
    public int StartingMaxHealth => Mathf.Max(1, startingMaxHealth);
    public int StartingBaseDamage => Mathf.Max(0, startingBaseDamage);
    public AbilityData[] StartingAbilities => startingAbilities;

    public bool CanUseWeaponType(WeaponType weaponType)
    {
        if (weaponType == WeaponType.None)
        {
            return true;
        }

        if (allowedWeaponTypes == null || allowedWeaponTypes.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < allowedWeaponTypes.Length; i++)
        {
            if (allowedWeaponTypes[i] == weaponType)
            {
                return true;
            }
        }

        return false;
    }
}