using TMPro;
using UnityEngine;

public class StatBreakdownTooltipUI : MonoBehaviour
{
    public static StatBreakdownTooltipUI Instance { get; private set; }

    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private Vector2 offset = new Vector2(12f, -12f);
    [SerializeField] private Vector2 screenPadding = new Vector2(16f, 16f);

    private Canvas rootCanvas;
    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rootCanvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        if (root != null && root.activeSelf)
        {
            UpdatePosition();
        }
    }

    public void Show(string title, string details)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (detailsText != null)
        {
            detailsText.text = details;
        }

        UpdatePosition();
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void UpdatePosition()
    {
        if (rectTransform == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        Vector2 desired = (Vector2)Input.mousePosition + offset;
        Vector2 size = rectTransform.rect.size;

        Vector2 finalPosition = desired;

        if (finalPosition.x + size.x > Screen.width - screenPadding.x)
        {
            finalPosition.x = Screen.width - screenPadding.x - size.x;
        }

        if (finalPosition.y - size.y < screenPadding.y)
        {
            finalPosition.y = screenPadding.y + size.y;
        }

        finalPosition.x = Mathf.Max(screenPadding.x, finalPosition.x);
        finalPosition.y = Mathf.Min(Screen.height - screenPadding.y, finalPosition.y);

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            RectTransform canvasRect = rootCanvas.transform as RectTransform;

            if (canvasRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    finalPosition,
                    rootCanvas.worldCamera,
                    out Vector2 localPoint))
            {
                rectTransform.localPosition = localPoint;
                return;
            }
        }

        rectTransform.position = finalPosition;
    }
}