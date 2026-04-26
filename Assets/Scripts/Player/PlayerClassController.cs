using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerClassController : MonoBehaviour
{
    [Header("Class")]
    [SerializeField] private CharacterClassData selectedClass;

    [Header("Startup")]
    [SerializeField] private bool applyClassStatsOnStart = true;
    [SerializeField] private bool assignClassAbilitiesToHotbar = true;

    private Health health;
    private PlayerCombat combat;
    private PlayerHotbar hotbar;

    public CharacterClassData SelectedClass => selectedClass;
    public string ClassName => selectedClass != null ? selectedClass.ClassName : "No Class";

    private void Awake()
    {
        health = GetComponent<Health>();
        combat = GetComponent<PlayerCombat>();
        hotbar = GetComponent<PlayerHotbar>();
    }

    private void Start()
    {
        ApplyClass();
    }

    public void ApplyClass()
    {
        if (selectedClass == null)
        {
            Debug.LogWarning("PlayerClassController has no selected class assigned.");
            return;
        }

        if (applyClassStatsOnStart)
        {
            ApplyClassStats();
        }

        if (assignClassAbilitiesToHotbar)
        {
            AssignClassAbilities();
        }

        PostSystem($"Class selected: {selectedClass.ClassName}.");
    }

    private void ApplyClassStats()
    {
        if (health != null)
        {
            health.SetBaseMaxHealth(selectedClass.StartingMaxHealth, true);
        }

        if (combat != null)
        {
            combat.SetBaseDamage(selectedClass.StartingBaseDamage);
        }
    }

    private void AssignClassAbilities()
    {
        if (hotbar == null || selectedClass.StartingAbilities == null)
        {
            return;
        }

        int maxSlots = Mathf.Min(hotbar.SlotCount, selectedClass.StartingAbilities.Length);

        for (int i = 0; i < maxSlots; i++)
        {
            AbilityData ability = selectedClass.StartingAbilities[i];

            if (ability != null)
            {
                hotbar.AssignAbilityToSlot(i, ability);
            }
        }
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}