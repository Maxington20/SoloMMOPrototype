using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainerWindowUI : MonoBehaviour
{
    public static TrainerWindowUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject trainerWindow;
    [SerializeField] private TMP_Text trainerNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button closeButton;

    [Header("Ability List")]
    [SerializeField] private Transform abilityListContainer;
    [SerializeField] private Button abilityButtonPrefab;

    private TrainerNPC currentTrainer;
    private PlayerAbilityLoadout playerAbilityLoadout;
    private PlayerProgression playerProgression;
    private PlayerClassController playerClassController;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        Close();
    }

    private void Update()
    {
        if (trainerWindow != null && trainerWindow.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open(TrainerNPC trainer)
    {
        currentTrainer = trainer;
        CachePlayerReferences();

        if (trainerWindow != null)
        {
            trainerWindow.SetActive(true);
        }

        Refresh();
    }

    public void Close()
    {
        if (trainerWindow != null)
        {
            trainerWindow.SetActive(false);
        }

        currentTrainer = null;
    }

    public void Refresh()
    {
        ClearAbilityList();

        if (currentTrainer == null)
        {
            return;
        }

        if (trainerNameText != null)
        {
            trainerNameText.text = currentTrainer.TrainerName;
        }

        if (!currentTrainer.PlayerMeetsClassRequirement(playerClassController))
        {
            if (descriptionText != null)
            {
                string requiredClassName = currentTrainer.ClassRestriction != null
                    ? currentTrainer.ClassRestriction.ClassName
                    : "another class";

                descriptionText.text = $"This trainer only trains {requiredClassName}.";
            }

            CreateDisabledButton("Wrong class");
            return;
        }

        if (descriptionText != null)
        {
            descriptionText.text = "Select an ability to learn.";
        }

        IReadOnlyList<ClassAbilityUnlock> abilities = currentTrainer.TrainableAbilities;

        if (abilities == null || abilities.Count == 0)
        {
            CreateDisabledButton("No abilities available");
            return;
        }

        for (int i = 0; i < abilities.Count; i++)
        {
            ClassAbilityUnlock unlock = abilities[i];

            if (unlock == null || unlock.Ability == null)
            {
                continue;
            }

            CreateAbilityButton(unlock);
        }
    }

    private void CreateAbilityButton(ClassAbilityUnlock unlock)
    {
        Button button = Instantiate(abilityButtonPrefab, abilityListContainer);
        button.gameObject.SetActive(true);

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);

        AbilityData ability = unlock.Ability;
        bool alreadyKnown = playerAbilityLoadout != null && playerAbilityLoadout.HasLearnedAbility(ability);
        bool meetsLevel = GetPlayerLevel() >= unlock.UnlockLevel;

        if (buttonText != null)
        {
            string status = "";

            if (alreadyKnown)
            {
                status = " - Known";
            }
            else if (!meetsLevel)
            {
                status = $" - Requires Level {unlock.UnlockLevel}";
            }

            buttonText.text = $"{ability.DisplayName}{status}";
            buttonText.color = alreadyKnown || !meetsLevel ? Color.gray : Color.white;
        }

        button.interactable = !alreadyKnown && meetsLevel;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            TryLearnAbility(unlock);
        });
    }

    private void TryLearnAbility(ClassAbilityUnlock unlock)
    {
        if (unlock == null || unlock.Ability == null)
        {
            return;
        }

        if (playerAbilityLoadout == null)
        {
            PostSystem("Player ability loadout is missing.");
            return;
        }

        if (GetPlayerLevel() < unlock.UnlockLevel)
        {
            PostSystem($"You must be level {unlock.UnlockLevel} to learn {unlock.Ability.DisplayName}.");
            return;
        }

        bool learned = playerAbilityLoadout.LearnAbility(unlock.Ability, true);

        if (!learned)
        {
            PostSystem($"You already know {unlock.Ability.DisplayName}.");
            Refresh();
            return;
        }

        PlayerHotbar hotbar = PlayerHotbar.Instance;

        if (unlock.AutoAssignToHotbar && hotbar != null && !hotbar.IsAbilityAssigned(unlock.Ability))
        {
            hotbar.AssignAbilityToFirstEmptySlot(unlock.Ability);
        }

        Refresh();
    }

    private void CreateDisabledButton(string label)
    {
        Button button = Instantiate(abilityButtonPrefab, abilityListContainer);
        button.gameObject.SetActive(true);
        button.interactable = false;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);

        if (buttonText != null)
        {
            buttonText.text = label;
            buttonText.color = Color.gray;
        }
    }

    private void CachePlayerReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        playerAbilityLoadout = playerObject.GetComponent<PlayerAbilityLoadout>();
        playerProgression = playerObject.GetComponent<PlayerProgression>();
        playerClassController = playerObject.GetComponent<PlayerClassController>();
    }

    private int GetPlayerLevel()
    {
        return playerProgression != null
            ? playerProgression.Level
            : 1;
    }

    private void ClearAbilityList()
    {
        if (abilityListContainer == null)
        {
            return;
        }

        for (int i = abilityListContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(abilityListContainer.GetChild(i).gameObject);
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