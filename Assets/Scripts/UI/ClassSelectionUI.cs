using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassSelectionUI : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private GameObject classSelectionWindow;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text selectedClassNameText;
    [SerializeField] private TMP_Text selectedClassDescriptionText;
    [SerializeField] private TMP_Text selectedClassDetailsText;
    [SerializeField] private Button startButton;

    [Header("Class List")]
    [SerializeField] private Transform classButtonContainer;
    [SerializeField] private ClassSelectionButtonUI classButtonPrefab;
    [SerializeField] private CharacterClassData[] availableClasses;

    [Header("Player")]
    [SerializeField] private PlayerClassController playerClassController;

    [Header("Startup")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private bool pauseGameUntilClassSelected = true;

    private CharacterClassData selectedClass;

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(ConfirmSelection);
            startButton.interactable = false;
        }

        BuildClassButtons();

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
        if (classSelectionWindow != null)
        {
            classSelectionWindow.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "Choose Your Class";
        }

        selectedClass = null;
        RefreshSelectedClassDetails();

        if (pauseGameUntilClassSelected)
        {
            Time.timeScale = 0f;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close(bool restoreTimeScale)
    {
        if (classSelectionWindow != null)
        {
            classSelectionWindow.SetActive(false);
        }

        if (restoreTimeScale)
        {
            Time.timeScale = 1f;
        }
    }

    private void BuildClassButtons()
    {
        if (classButtonContainer == null || classButtonPrefab == null)
        {
            return;
        }

        for (int i = classButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(classButtonContainer.GetChild(i).gameObject);
        }

        if (availableClasses == null)
        {
            return;
        }

        foreach (CharacterClassData classData in availableClasses)
        {
            if (classData == null)
            {
                continue;
            }

            ClassSelectionButtonUI button = Instantiate(classButtonPrefab, classButtonContainer);
            button.gameObject.SetActive(true);
            button.Initialize(classData, SelectClass);
        }
    }

    private void SelectClass(CharacterClassData classData)
    {
        selectedClass = classData;
        RefreshSelectedClassDetails();

        if (startButton != null)
        {
            startButton.interactable = selectedClass != null;
        }
    }

    private void RefreshSelectedClassDetails()
    {
        if (selectedClass == null)
        {
            if (selectedClassNameText != null)
            {
                selectedClassNameText.text = "No Class Selected";
            }

            if (selectedClassDescriptionText != null)
            {
                selectedClassDescriptionText.text = "Select a class from the left.";
            }

            if (selectedClassDetailsText != null)
            {
                selectedClassDetailsText.text = string.Empty;
            }

            return;
        }

        if (selectedClassNameText != null)
        {
            selectedClassNameText.text = selectedClass.ClassName;
        }

        if (selectedClassDescriptionText != null)
        {
            selectedClassDescriptionText.text = selectedClass.Description;
        }

        if (selectedClassDetailsText != null)
        {
            StatBlock stats = selectedClass.StartingStats;

            selectedClassDetailsText.text =
                $"Role: {selectedClass.Role}\n" +
                $"Primary Stat: {selectedClass.PrimaryStat}\n\n" +
                $"Starting Stats\n" +
                $"Strength: {stats.Strength}\n" +
                $"Agility: {stats.Agility}\n" +
                $"Intellect: {stats.Intellect}\n" +
                $"Stamina: {stats.Stamina}\n" +
                $"Armour: {stats.Armor}\n" +
                $"Hit: {stats.HitChance}";
        }
    }

    private void ConfirmSelection()
    {
        if (selectedClass == null || playerClassController == null)
        {
            return;
        }

        playerClassController.SetSelectedClass(selectedClass, true);
        Close(true);
    }
}