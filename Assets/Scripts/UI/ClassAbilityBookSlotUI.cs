using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClassAbilityBookSlotUI : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private CanvasGroup canvasGroup;

    private AbilityData ability;
    private bool isLearned;
    private int unlockLevel;

    private Action<AbilityData> onClicked;
    private Action<AbilityData> onDragStarted;
    private Action<AbilityData> onDragEnded;

    public void Initialize(
        AbilityData abilityData,
        bool learned,
        int requiredLevel,
        Action<AbilityData> clickedCallback,
        Action<AbilityData> dragStartedCallback,
        Action<AbilityData> dragEndedCallback)
    {
        ability = abilityData;
        isLearned = learned;
        unlockLevel = Mathf.Max(1, requiredLevel);

        onClicked = clickedCallback;
        onDragStarted = dragStartedCallback;
        onDragEnded = dragEndedCallback;

        Refresh();
    }

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        DisableChildRaycasts();
    }

    private void Refresh()
    {
        if (iconImage != null)
        {
            iconImage.sprite = ability != null ? ability.Icon : null;
            iconImage.enabled = ability != null && ability.Icon != null;
            iconImage.color = isLearned ? Color.white : new Color(1f, 1f, 1f, 0.35f);
        }

        if (nameText != null)
        {
            nameText.text = ability != null ? ability.DisplayName : "Unknown Ability";
            nameText.color = isLearned ? Color.white : Color.gray;
        }

        if (detailsText != null)
        {
            detailsText.text = BuildDetailsText();
            detailsText.color = isLearned ? Color.white : Color.gray;
        }

        if (descriptionText != null)
        {
            descriptionText.text = ability != null ? ability.Description : string.Empty;
            descriptionText.color = isLearned ? Color.white : Color.gray;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = isLearned ? 1f : 0.55f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = isLearned;
        }
    }

    private string BuildDetailsText()
    {
        if (ability == null)
        {
            return string.Empty;
        }

        if (!isLearned)
        {
            return $"Unlocks at Level {unlockLevel}";
        }

        string manaText = ability.ManaCost > 0 ? $"Mana: {ability.ManaCost}" : "No mana cost";
        string cooldownText = $"CD: {ability.CooldownSeconds:0.#}s";
        string castText = ability.CastType switch
        {
            AbilityCastType.CastTime => $"Cast: {ability.CastTimeSeconds:0.#}s",
            AbilityCastType.Channel => $"Channel: {ability.ChannelDurationSeconds:0.#}s",
            _ => "Instant"
        };

        return $"{manaText}  |  {cooldownText}  |  {castText}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isLearned || ability == null)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onClicked?.Invoke(ability);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isLearned || ability == null || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        onDragStarted?.Invoke(ability);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Required so Unity treats this as a real drag operation.
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isLearned || ability == null || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        onDragEnded?.Invoke(ability);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ability == null)
        {
            return;
        }

        AbilityTooltipUI.Instance?.Show(ability);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AbilityTooltipUI.Instance?.Hide();
    }

    private void DisableChildRaycasts()
    {
        if (iconImage != null) iconImage.raycastTarget = false;
        if (nameText != null) nameText.raycastTarget = false;
        if (detailsText != null) detailsText.raycastTarget = false;
        if (descriptionText != null) descriptionText.raycastTarget = false;
    }
}