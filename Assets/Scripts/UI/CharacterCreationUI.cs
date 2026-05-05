using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationUI : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private GameObject characterCreationWindow;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Button startButton;

    [Header("Race List")]
    [SerializeField] private Transform raceButtonContainer;
    [SerializeField] private CharacterCreationOptionButtonUI raceButtonPrefab;
    [SerializeField] private CharacterRaceData[] availableRaces;

    [Header("Class List")]
    [SerializeField] private Transform classButtonContainer;
    [SerializeField] private CharacterCreationOptionButtonUI classButtonPrefab;
    [SerializeField] private CharacterClassData[] availableClasses;

    [Header("Background List")]
    [SerializeField] private Transform backgroundButtonContainer;
    [SerializeField] private CharacterCreationOptionButtonUI backgroundButtonPrefab;
    [SerializeField] private CharacterBackgroundData[] availableBackgrounds;

    [Header("Player")]
    [SerializeField] private PlayerCharacterIdentity characterIdentity;
    [SerializeField] private PlayerClassController playerClassController;
    [SerializeField] private PlayerHotbar playerHotbar;
    [SerializeField] private PlayerStats playerStats;

    [Header("Startup")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private bool pauseGameUntilCreated = true;

    private CharacterRaceData selectedRace;
    private CharacterClassData selectedClass;
    private CharacterBackgroundData selectedBackground;

    private CharacterCreationOptionButtonUI selectedRaceButton;
    private CharacterCreationOptionButtonUI selectedClassButton;
    private CharacterCreationOptionButtonUI selectedBackgroundButton;

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(ConfirmCharacterCreation);
            startButton.interactable = false;
        }

        BuildRaceButtons();
        BuildClassButtons();
        BuildBackgroundButtons();

        if (showOnStart)
        {
            Open();
        }
        else
        {
            Close(false);
        }
    }

    public void Open()
    {
        if (characterCreationWindow != null)
        {
            characterCreationWindow.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "Create Your Character";
        }

        selectedRace = null;
        selectedClass = null;
        selectedBackground = null;

        selectedRaceButton = null;
        selectedClassButton = null;
        selectedBackgroundButton = null;

        ClearAllButtonSelections();
        RefreshSummary();

        if (pauseGameUntilCreated)
        {
            Time.timeScale = 0f;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close(bool restoreTimeScale)
    {
        if (characterCreationWindow != null)
        {
            characterCreationWindow.SetActive(false);
        }

        if (restoreTimeScale)
        {
            Time.timeScale = 1f;
        }
    }

    private void BuildRaceButtons()
    {
        ClearContainer(raceButtonContainer);

        if (raceButtonContainer == null || raceButtonPrefab == null || availableRaces == null)
        {
            return;
        }

        foreach (CharacterRaceData race in availableRaces)
        {
            if (race == null)
            {
                continue;
            }

            CharacterCreationOptionButtonUI button = Instantiate(raceButtonPrefab, raceButtonContainer);
            button.gameObject.SetActive(true);

            CharacterRaceData capturedRace = race;
            CharacterCreationOptionButtonUI capturedButton = button;

            button.Initialize(
                race.Icon,
                race.RaceName,
                BuildStatSummary(race.StatBonuses),
                race.Description,
                () => SelectRace(capturedRace, capturedButton));
        }
    }

    private void BuildClassButtons()
    {
        ClearContainer(classButtonContainer);

        if (classButtonContainer == null || classButtonPrefab == null || availableClasses == null)
        {
            return;
        }

        foreach (CharacterClassData characterClass in availableClasses)
        {
            if (characterClass == null)
            {
                continue;
            }

            CharacterCreationOptionButtonUI button = Instantiate(classButtonPrefab, classButtonContainer);
            button.gameObject.SetActive(true);

            CharacterClassData capturedClass = characterClass;
            CharacterCreationOptionButtonUI capturedButton = button;

            button.Initialize(
                characterClass.Icon,
                characterClass.ClassName,
                $"Role: {characterClass.Role} | Primary: {characterClass.PrimaryStat}",
                characterClass.Description,
                () => SelectClass(capturedClass, capturedButton));
        }
    }

    private void BuildBackgroundButtons()
    {
        ClearContainer(backgroundButtonContainer);

        if (backgroundButtonContainer == null || backgroundButtonPrefab == null || availableBackgrounds == null)
        {
            return;
        }

        foreach (CharacterBackgroundData background in availableBackgrounds)
        {
            if (background == null)
            {
                continue;
            }

            CharacterCreationOptionButtonUI button = Instantiate(backgroundButtonPrefab, backgroundButtonContainer);
            button.gameObject.SetActive(true);

            CharacterBackgroundData capturedBackground = background;
            CharacterCreationOptionButtonUI capturedButton = button;

            button.Initialize(
                background.Icon,
                background.BackgroundName,
                BuildStatSummary(background.StatBonuses),
                background.Description,
                () => SelectBackground(capturedBackground, capturedButton));
        }
    }

    private void ClearContainer(Transform container)
    {
        if (container == null)
        {
            return;
        }

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    private void ClearAllButtonSelections()
    {
        ClearButtonSelectionsInContainer(raceButtonContainer);
        ClearButtonSelectionsInContainer(classButtonContainer);
        ClearButtonSelectionsInContainer(backgroundButtonContainer);
    }

    private void ClearButtonSelectionsInContainer(Transform container)
    {
        if (container == null)
        {
            return;
        }

        CharacterCreationOptionButtonUI[] buttons =
            container.GetComponentsInChildren<CharacterCreationOptionButtonUI>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetSelected(false);
        }
    }

    private void SelectRace(CharacterRaceData race, CharacterCreationOptionButtonUI button)
    {
        selectedRace = race;

        if (selectedRaceButton != null)
        {
            selectedRaceButton.SetSelected(false);
        }

        selectedRaceButton = button;

        if (selectedRaceButton != null)
        {
            selectedRaceButton.SetSelected(true);
        }

        RefreshSummary();
    }

    private void SelectClass(CharacterClassData characterClass, CharacterCreationOptionButtonUI button)
    {
        selectedClass = characterClass;

        if (selectedClassButton != null)
        {
            selectedClassButton.SetSelected(false);
        }

        selectedClassButton = button;

        if (selectedClassButton != null)
        {
            selectedClassButton.SetSelected(true);
        }

        RefreshSummary();
    }

    private void SelectBackground(CharacterBackgroundData background, CharacterCreationOptionButtonUI button)
    {
        selectedBackground = background;

        if (selectedBackgroundButton != null)
        {
            selectedBackgroundButton.SetSelected(false);
        }

        selectedBackgroundButton = button;

        if (selectedBackgroundButton != null)
        {
            selectedBackgroundButton.SetSelected(true);
        }

        RefreshSummary();
    }

    private void RefreshSummary()
    {
        if (summaryText != null)
        {
            summaryText.text = BuildSummaryText();
        }

        if (startButton != null)
        {
            startButton.interactable =
                selectedRace != null &&
                selectedClass != null &&
                selectedBackground != null;
        }
    }

    private string BuildSummaryText()
    {
        string raceName = selectedRace != null ? selectedRace.RaceName : "None";
        string className = selectedClass != null ? selectedClass.ClassName : "None";
        string backgroundName = selectedBackground != null ? selectedBackground.BackgroundName : "None";

        string raceStats = selectedRace != null ? BuildStatSummary(selectedRace.StatBonuses) : "-";
        string classStats = selectedClass != null ? BuildStatSummary(selectedClass.StartingStats) : "-";
        string backgroundStats = selectedBackground != null ? BuildStatSummary(selectedBackground.StatBonuses) : "-";

        string innate = selectedRace != null && selectedRace.InnateAbility != null
            ? selectedRace.InnateAbility.DisplayName
            : "None";

        return
            $"Race: {raceName}\n" +
            $"Class: {className}\n" +
            $"Background: {backgroundName}\n\n" +
            $"Race Bonuses: {raceStats}\n" +
            $"Class Starting Stats: {classStats}\n" +
            $"Background Bonuses: {backgroundStats}\n\n" +
            $"Innate Ability: {innate}";
    }

    private string BuildStatSummary(StatBlock stats)
    {
        string result = string.Empty;

        result = AppendStat(result, "Str", stats.Strength);
        result = AppendStat(result, "Agi", stats.Agility);
        result = AppendStat(result, "Int", stats.Intellect);
        result = AppendStat(result, "Sta", stats.Stamina);
        result = AppendStat(result, "Armour", stats.Armor);
        result = AppendStat(result, "Hit", stats.HitChance);

        return string.IsNullOrWhiteSpace(result) ? "No stat bonuses" : result;
    }

    private string AppendStat(string current, string label, int value)
    {
        if (value == 0)
        {
            return current;
        }

        string sign = value > 0 ? "+" : "";

        if (string.IsNullOrWhiteSpace(current))
        {
            return $"{sign}{value} {label}";
        }

        return $"{current}, {sign}{value} {label}";
    }

    private void ConfirmCharacterCreation()
    {
        if (selectedRace == null || selectedClass == null || selectedBackground == null)
        {
            return;
        }

        if (characterIdentity != null)
        {
            characterIdentity.SetIdentity(selectedRace, selectedBackground);
        }

        if (playerClassController != null)
        {
            playerClassController.SetSelectedClass(selectedClass, true);
        }

        AssignRaceInnateAbility();

        if (playerStats != null)
        {
            playerStats.RecalculateAndApplyStats();
        }

        PostSystem($"Created {selectedRace.RaceName} {selectedClass.ClassName} with {selectedBackground.BackgroundName} background.");

        Close(true);
    }

    private void AssignRaceInnateAbility()
    {
        if (selectedRace == null || selectedRace.InnateAbility == null || playerHotbar == null)
        {
            return;
        }

        for (int i = 0; i < playerHotbar.SlotCount; i++)
        {
            if (playerHotbar.IsSlotEmpty(i))
            {
                playerHotbar.AssignAbilityToSlot(i, selectedRace.InnateAbility);
                return;
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