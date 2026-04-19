using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public enum ChatChannel
    {
        Say,
        System
    }

    public static ChatManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI chatText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private float contentPadding = 12f;
    [SerializeField] private int maxMessages = 50;

    private readonly Queue<string> messages = new();

    private void Awake()
    {
        Debug.Log("ChatManager Awake started.");

        if (Instance != null && Instance != this)
        {
            Debug.Log("Duplicate ChatManager destroyed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log("ChatManager Instance assigned.");

        AddMessage("<color=#FFD166>[System]</color> Chat initialized.");
    }

    public void PostSay(string speakerName, string message)
    {
        string line = $"<color=#FFFFFF>[Say]</color> <color=#A6E3FF>{speakerName}</color>: {message}";
        AddMessage(line);
    }

    public void PostSystem(string message)
    {
        string line = $"<color=#FFD166>[System]</color> {message}";
        AddMessage(line);
    }

    private void AddMessage(string message)
    {
        Debug.Log($"Chat AddMessage: {message}");

        messages.Enqueue(message);

        while (messages.Count > maxMessages)
        {
            messages.Dequeue();
        }

        RefreshChatText();
    }

    private void RefreshChatText()
    {
        if (chatText == null)
        {
            Debug.LogWarning("ChatManager: chatText is not assigned.");
            return;
        }

        chatText.text = string.Join("\n", messages);

        Canvas.ForceUpdateCanvases();
        chatText.ForceMeshUpdate();

        if (contentRect != null)
        {
            float preferredHeight = chatText.preferredHeight + contentPadding;
            float minimumHeight = scrollRect != null && scrollRect.viewport != null
                ? scrollRect.viewport.rect.height
                : 100f;

            float finalHeight = Mathf.Max(preferredHeight, minimumHeight);
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);
        }
        else
        {
            Debug.LogWarning("ChatManager: contentRect is not assigned.");
        }

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
        else
        {
            Debug.LogWarning("ChatManager: scrollRect is not assigned.");
        }
    }
}