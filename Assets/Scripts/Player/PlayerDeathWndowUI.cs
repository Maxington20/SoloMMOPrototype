using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeathWindowUI : MonoBehaviour
{
    public static PlayerDeathWindowUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject deathWindow;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button okayButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (okayButton != null)
        {
            okayButton.onClick.AddListener(HandleOkayClicked);
        }

        Close();
    }

    public void Open()
    {
        if (deathWindow != null)
        {
            deathWindow.SetActive(true);
        }

        if (messageText != null)
        {
            messageText.text = "You died.";
        }
    }

    public void Close()
    {
        if (deathWindow != null)
        {
            deathWindow.SetActive(false);
        }
    }

    private void HandleOkayClicked()
    {
        Close();

        if (PlayerDeathController.Instance != null)
        {
            PlayerDeathController.Instance.Respawn();
        }
    }
}