using TMPro;
using UnityEngine;

public class CharacterUI : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private GameObject characterWindow;

    [Header("Text")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text goldText;

    [Header("Player References")]
    [SerializeField] private DisplayName displayName;
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private Health health;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerInventory inventory;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;

    private bool isOpen;

    private void Start()
    {
        if (characterWindow != null)
        {
            characterWindow.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }

        if (isOpen)
        {
            Refresh();
        }
    }

    private void Toggle()
    {
        isOpen = !isOpen;

        if (characterWindow != null)
        {
            characterWindow.SetActive(isOpen);
        }

        if (isOpen)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (nameText != null)
        {
            nameText.text = displayName != null
                ? displayName.Display
                : "Name: Unknown";
        }

        if (levelText != null)
        {
            levelText.text = progression != null
                ? $"Level: {progression.Level}"
                : "Level: ?";
        }

        if (healthText != null)
        {
            healthText.text = health != null
                ? $"Health: {health.CurrentHealth}/{health.MaxHealth}"
                : "Health: ?";
        }

        if (damageText != null)
        {
            damageText.text = combat != null
                ? $"Damage: {combat.Damage}"
                : "Damage: ?";
        }

        if (goldText != null)
        {
            goldText.text = inventory != null
                ? $"Gold: {inventory.Gold}"
                : "Gold: 0";
        }
    }
}