using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestGiverUI : MonoBehaviour
{
    public static QuestGiverUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject questGiverWindow;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;

    [Header("Sections")]
    [SerializeField] private Transform turnInQuestContainer;
    [SerializeField] private Transform availableQuestContainer;
    [SerializeField] private Transform inProgressQuestContainer;

    [Header("Prefabs")]
    [SerializeField] private Button questButtonPrefab;

    public bool IsOpen => questGiverWindow != null && questGiverWindow.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

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
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open()
    {
        if (questGiverWindow != null)
        {
            questGiverWindow.SetActive(true);
        }

        Refresh();
    }

    public void Close()
    {
        if (questGiverWindow != null)
        {
            questGiverWindow.SetActive(false);
        }
    }

    public void Refresh()
    {
        ClearContainer(turnInQuestContainer);
        ClearContainer(availableQuestContainer);
        ClearContainer(inProgressQuestContainer);

        if (titleText != null)
        {
            titleText.text = "Quest Giver";
        }

        if (QuestManager.Instance == null)
        {
            return;
        }

        BuildTurnInQuests();
        BuildAvailableQuests();
        BuildInProgressQuests();
    }

    private void BuildTurnInQuests()
    {
        List<ActiveQuest> completableQuests = QuestManager.Instance.GetCompletableQuests();

        if (completableQuests.Count == 0)
        {
            CreateDisabledButton(turnInQuestContainer, "No quests ready to turn in");
            return;
        }

        foreach (ActiveQuest quest in completableQuests)
        {
            if (quest == null || quest.definition == null)
            {
                continue;
            }

            ActiveQuest capturedQuest = quest;

            Button button = CreateButton(turnInQuestContainer, $"Turn In: {quest.definition.title}");
            button.onClick.AddListener(() =>
            {
                QuestManager.Instance.TurnInQuest(capturedQuest);
                Refresh();
            });
        }
    }

    private void BuildAvailableQuests()
    {
        List<QuestDefinition> availableQuests = QuestManager.Instance.GetAvailableQuests();

        if (availableQuests.Count == 0)
        {
            CreateDisabledButton(availableQuestContainer, "No quests available");
            return;
        }

        foreach (QuestDefinition quest in availableQuests)
        {
            if (quest == null)
            {
                continue;
            }

            QuestDefinition capturedQuest = quest;

            Button button = CreateButton(availableQuestContainer, $"Accept: {quest.title}");
            button.onClick.AddListener(() =>
            {
                QuestManager.Instance.AcceptQuest(capturedQuest);
                Refresh();
            });
        }
    }

    private void BuildInProgressQuests()
    {
        List<ActiveQuest> inProgressQuests = QuestManager.Instance.GetInProgressQuests();

        if (inProgressQuests.Count == 0)
        {
            CreateDisabledButton(inProgressQuestContainer, "No quests in progress");
            return;
        }

        foreach (ActiveQuest quest in inProgressQuests)
        {
            if (quest == null || quest.definition == null)
            {
                continue;
            }

            CreateDisabledButton(inProgressQuestContainer, $"In Progress: {quest.definition.title}");
        }
    }

    private Button CreateButton(Transform parent, string label)
    {
        Button button = Instantiate(questButtonPrefab, parent);
        button.gameObject.SetActive(true);

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = label;
            text.color = Color.white;
        }

        button.interactable = true;
        return button;
    }

    private void CreateDisabledButton(Transform parent, string label)
    {
        Button button = Instantiate(questButtonPrefab, parent);
        button.gameObject.SetActive(true);
        button.interactable = false;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = label;
            text.color = Color.gray;
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
}