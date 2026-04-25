using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestGiverUI : MonoBehaviour
{
    public static QuestGiverUI Instance { get; private set; }

    private enum SelectedQuestState
    {
        None,
        Available,
        InProgress,
        Completable
    }

    [Header("Window")]
    [SerializeField] private GameObject questGiverWindow;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;

    [Header("Sections")]
    [SerializeField] private Transform turnInQuestContainer;
    [SerializeField] private Transform availableQuestContainer;
    [SerializeField] private Transform inProgressQuestContainer;

    [Header("Detail Panel")]
    [SerializeField] private TMP_Text detailTitleText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailObjectiveText;
    [SerializeField] private TMP_Text detailRewardText;
    [SerializeField] private TMP_Text detailStatusText;

    [Header("Action Button")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    [Header("Prefabs")]
    [SerializeField] private Button questButtonPrefab;

    public bool IsOpen => questGiverWindow != null && questGiverWindow.activeSelf;

    private SelectedQuestState selectedState = SelectedQuestState.None;
    private QuestDefinition selectedAvailableQuest;
    private ActiveQuest selectedActiveQuest;

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

        if (actionButton != null)
        {
            actionButton.onClick.AddListener(HandleActionButtonClicked);
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
            ClearSelection();
            RefreshDetails();
            return;
        }

        ValidateSelection();

        BuildTurnInQuests();
        BuildAvailableQuests();
        BuildInProgressQuests();

        AutoSelectFirstQuestIfNeeded();
        RefreshDetails();
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
            bool isSelected = selectedState == SelectedQuestState.Completable && selectedActiveQuest == capturedQuest;

            Button button = CreateButton(
                turnInQuestContainer,
                $"{GetSelectionPrefix(isSelected)}? {quest.definition.title}");

            button.onClick.AddListener(() =>
            {
                SelectCompletableQuest(capturedQuest);
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
            bool isSelected = selectedState == SelectedQuestState.Available && selectedAvailableQuest == capturedQuest;

            Button button = CreateButton(
                availableQuestContainer,
                $"{GetSelectionPrefix(isSelected)}! {quest.title}");

            button.onClick.AddListener(() =>
            {
                SelectAvailableQuest(capturedQuest);
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

            ActiveQuest capturedQuest = quest;
            bool isSelected = selectedState == SelectedQuestState.InProgress && selectedActiveQuest == capturedQuest;

            Button button = CreateButton(
                inProgressQuestContainer,
                $"{GetSelectionPrefix(isSelected)}? {quest.definition.title}");

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.color = isSelected ? Color.white : Color.gray;
            }

            button.onClick.AddListener(() =>
            {
                SelectInProgressQuest(capturedQuest);
            });
        }
    }

    private void SelectAvailableQuest(QuestDefinition quest)
    {
        selectedState = SelectedQuestState.Available;
        selectedAvailableQuest = quest;
        selectedActiveQuest = null;

        Refresh();
    }

    private void SelectInProgressQuest(ActiveQuest quest)
    {
        selectedState = SelectedQuestState.InProgress;
        selectedAvailableQuest = null;
        selectedActiveQuest = quest;

        Refresh();
    }

    private void SelectCompletableQuest(ActiveQuest quest)
    {
        selectedState = SelectedQuestState.Completable;
        selectedAvailableQuest = null;
        selectedActiveQuest = quest;

        Refresh();
    }

    private void HandleActionButtonClicked()
    {
        if (QuestManager.Instance == null)
        {
            return;
        }

        if (selectedState == SelectedQuestState.Available && selectedAvailableQuest != null)
        {
            QuestManager.Instance.AcceptQuest(selectedAvailableQuest);
            ClearSelection();
            Refresh();
            return;
        }

        if (selectedState == SelectedQuestState.Completable && selectedActiveQuest != null)
        {
            QuestManager.Instance.TurnInQuest(selectedActiveQuest);
            Refresh();
        }
    }

    private void RefreshDetails()
    {
        if (selectedState == SelectedQuestState.Available && selectedAvailableQuest != null)
        {
            ShowQuestDefinitionDetails(selectedAvailableQuest, null, "Available");
            SetActionButton(true, "Accept");
            return;
        }

        if (selectedState == SelectedQuestState.InProgress && selectedActiveQuest != null && selectedActiveQuest.definition != null)
        {
            ShowQuestDefinitionDetails(selectedActiveQuest.definition, selectedActiveQuest, "In Progress");
            SetActionButton(false, "In Progress");
            return;
        }

        if (selectedState == SelectedQuestState.Completable && selectedActiveQuest != null && selectedActiveQuest.definition != null)
        {
            ShowQuestDefinitionDetails(selectedActiveQuest.definition, selectedActiveQuest, "Complete");
            SetActionButton(true, "Turn In");
            return;
        }

        ShowNoQuestSelected();
        SetActionButton(false, "Select Quest");
    }

    private void ShowQuestDefinitionDetails(QuestDefinition definition, ActiveQuest activeQuest, string status)
    {
        if (detailTitleText != null)
        {
            detailTitleText.text = definition.title;
        }

        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = definition.description;
        }

        if (detailObjectiveText != null)
        {
            detailObjectiveText.text = BuildObjectiveText(definition, activeQuest);
        }

        if (detailRewardText != null)
        {
            detailRewardText.text = BuildRewardText(definition);
        }

        if (detailStatusText != null)
        {
            detailStatusText.text = status;
        }
    }

    private string BuildObjectiveText(QuestDefinition definition, ActiveQuest activeQuest)
    {
        string text = "<b>Objectives</b>";
        bool hasObjective = false;

        if (definition.killObjectives != null)
        {
            foreach (QuestKillObjective objective in definition.killObjectives)
            {
                if (objective == null)
                {
                    continue;
                }

                int currentAmount = activeQuest != null
                    ? activeQuest.GetKillProgress(objective.EnemyType)
                    : 0;

                text += $"\nKill {objective.RequiredAmount} {GetEnemyDisplayName(objective.EnemyType)} " +
                        $"({currentAmount}/{objective.RequiredAmount})";

                hasObjective = true;
            }
        }

        if (definition.collectionObjectives != null)
        {
            foreach (QuestCollectionObjective objective in definition.collectionObjectives)
            {
                if (objective == null || objective.Item == null)
                {
                    continue;
                }

                int currentAmount = PlayerInventory.Instance != null
                    ? PlayerInventory.Instance.GetTotalQuantityOfItem(objective.Item)
                    : 0;

                text += $"\nCollect {objective.RequiredAmount} {objective.Item.DisplayName} " +
                        $"({currentAmount}/{objective.RequiredAmount})";

                hasObjective = true;
            }
        }

        if (!hasObjective)
        {
            text += "\nNone";
        }

        return text;
    }

    private string BuildRewardText(QuestDefinition definition)
    {
        string reward = "<b>Rewards</b>";
        bool hasReward = false;

        if (definition.xpReward > 0)
        {
            reward += $"\n{definition.xpReward} XP";
            hasReward = true;
        }

        if (definition.goldReward > 0)
        {
            reward += $"\n{definition.goldReward} Gold";
            hasReward = true;
        }

        if (definition.guaranteedItemRewards != null)
        {
            foreach (QuestItemReward itemReward in definition.guaranteedItemRewards)
            {
                if (itemReward == null || itemReward.Item == null)
                {
                    continue;
                }

                reward += "\n" + FormatItemReward(itemReward, false);
                hasReward = true;
            }
        }

        if (definition.choiceItemRewards != null && definition.choiceItemRewards.Count > 0)
        {
            reward += "\n\n<b>Choose One</b>";
            hasReward = true;

            foreach (QuestItemReward itemReward in definition.choiceItemRewards)
            {
                if (itemReward == null || itemReward.Item == null)
                {
                    continue;
                }

                reward += "\n- " + FormatItemReward(itemReward, false);
            }
        }

        if (!hasReward)
        {
            reward += "\nNone";
        }

        return reward;
    }

    private string FormatItemReward(QuestItemReward itemReward, bool includeDash)
    {
        ItemData item = itemReward.Item;
        string itemName = item.DisplayName;

        Color color = ItemRarityUtility.GetColor(item.Rarity);
        string colorHex = ColorUtility.ToHtmlStringRGB(color);

        string quantityText = itemReward.Quantity > 1
            ? $" x{itemReward.Quantity}"
            : string.Empty;

        string prefix = includeDash ? "- " : string.Empty;

        return $"{prefix}<color=#{colorHex}>{itemName}</color>{quantityText}";
    }

    private void ShowNoQuestSelected()
    {
        if (detailTitleText != null)
        {
            detailTitleText.text = "No Quest Selected";
        }

        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = "Select a quest from the list to view its details.";
        }

        if (detailObjectiveText != null)
        {
            detailObjectiveText.text = "<b>Objectives</b>\nNone";
        }

        if (detailRewardText != null)
        {
            detailRewardText.text = "<b>Rewards</b>\nNone";
        }

        if (detailStatusText != null)
        {
            detailStatusText.text = "No quest selected";
        }
    }

    private void SetActionButton(bool interactable, string label)
    {
        if (actionButton != null)
        {
            actionButton.interactable = interactable;
        }

        if (actionButtonText != null)
        {
            actionButtonText.text = label;
            actionButtonText.color = interactable ? Color.white : Color.gray;
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

    private void ClearSelection()
    {
        selectedState = SelectedQuestState.None;
        selectedAvailableQuest = null;
        selectedActiveQuest = null;
    }

    private void ValidateSelection()
    {
        if (selectedState == SelectedQuestState.Available)
        {
            if (selectedAvailableQuest == null || !QuestManager.Instance.GetAvailableQuests().Contains(selectedAvailableQuest))
            {
                ClearSelection();
            }

            return;
        }

        if (selectedState == SelectedQuestState.InProgress)
        {
            if (selectedActiveQuest == null || !QuestManager.Instance.GetInProgressQuests().Contains(selectedActiveQuest))
            {
                ClearSelection();
            }

            return;
        }

        if (selectedState == SelectedQuestState.Completable)
        {
            if (selectedActiveQuest == null || !QuestManager.Instance.GetCompletableQuests().Contains(selectedActiveQuest))
            {
                ClearSelection();
            }
        }
    }

    private void AutoSelectFirstQuestIfNeeded()
    {
        if (selectedState != SelectedQuestState.None || QuestManager.Instance == null)
        {
            return;
        }

        List<ActiveQuest> completableQuests = QuestManager.Instance.GetCompletableQuests();
        if (completableQuests.Count > 0)
        {
            SelectCompletableQuestWithoutRefresh(completableQuests[0]);
            return;
        }

        List<QuestDefinition> availableQuests = QuestManager.Instance.GetAvailableQuests();
        if (availableQuests.Count > 0)
        {
            SelectAvailableQuestWithoutRefresh(availableQuests[0]);
            return;
        }

        List<ActiveQuest> inProgressQuests = QuestManager.Instance.GetInProgressQuests();
        if (inProgressQuests.Count > 0)
        {
            SelectInProgressQuestWithoutRefresh(inProgressQuests[0]);
        }
    }

    private void SelectAvailableQuestWithoutRefresh(QuestDefinition quest)
    {
        selectedState = SelectedQuestState.Available;
        selectedAvailableQuest = quest;
        selectedActiveQuest = null;
    }

    private void SelectInProgressQuestWithoutRefresh(ActiveQuest quest)
    {
        selectedState = SelectedQuestState.InProgress;
        selectedAvailableQuest = null;
        selectedActiveQuest = quest;
    }

    private void SelectCompletableQuestWithoutRefresh(ActiveQuest quest)
    {
        selectedState = SelectedQuestState.Completable;
        selectedAvailableQuest = null;
        selectedActiveQuest = quest;
    }

    private string GetSelectionPrefix(bool isSelected)
    {
        return isSelected ? "> " : "  ";
    }

    private string GetEnemyDisplayName(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Wolf => "Wolves",
            EnemyType.Goblin => "Goblins",
            _ => enemyType.ToString()
        };
    }
}