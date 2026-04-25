using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRewardChoiceUI : MonoBehaviour
{
    public static QuestRewardChoiceUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject rewardChoiceWindow;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;

    [Header("Buttons")]
    [SerializeField] private Transform rewardButtonContainer;
    [SerializeField] private Button rewardButtonPrefab;

    private Action<int> onRewardChosen;

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

    public void Open(QuestDefinition quest, Action<int> rewardChosenCallback)
    {
        if (quest == null || quest.choiceItemRewards == null || quest.choiceItemRewards.Count == 0)
        {
            return;
        }

        onRewardChosen = rewardChosenCallback;

        if (rewardChoiceWindow != null)
        {
            rewardChoiceWindow.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = $"Choose a reward for {quest.title}";
        }

        BuildButtons(quest);
    }

    public void Close()
    {
        if (rewardChoiceWindow != null)
        {
            rewardChoiceWindow.SetActive(false);
        }

        onRewardChosen = null;
    }

    private void BuildButtons(QuestDefinition quest)
    {
        if (rewardButtonContainer == null || rewardButtonPrefab == null)
        {
            return;
        }

        for (int i = rewardButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(rewardButtonContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < quest.choiceItemRewards.Count; i++)
        {
            QuestItemReward reward = quest.choiceItemRewards[i];
            if (reward == null || reward.Item == null)
            {
                continue;
            }

            int rewardIndex = i;

            Button button = Instantiate(rewardButtonPrefab, rewardButtonContainer);
            button.gameObject.SetActive(true);

            QuestRewardChoiceButtonUI buttonUI = button.GetComponent<QuestRewardChoiceButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Refresh(reward);
            }

            button.onClick.AddListener(() => ChooseReward(rewardIndex));
        }
    }

    private void ChooseReward(int rewardIndex)
    {
        if (rewardChoiceWindow != null)
        {
            rewardChoiceWindow.SetActive(false);
        }

        Action<int> callback = onRewardChosen;
        onRewardChosen = null;

        callback?.Invoke(rewardIndex);
    }
}