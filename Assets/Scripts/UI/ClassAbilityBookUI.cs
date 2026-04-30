using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassAbilityBookUI : MonoBehaviour
{
    public static ClassAbilityBookUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject abilityBookWindow;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform abilityContainer;
    [SerializeField] private ClassAbilityBookSlotUI abilitySlotPrefab;

    [Header("Player")]
    [SerializeField] private PlayerClassController classController;
    [SerializeField] private PlayerProgression progression;

    [Header("Drag Preview")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Image dragPreviewImage;
    [SerializeField] private Vector2 dragPreviewOffset = new Vector2(12f, -12f);

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.K;

    private readonly List<ClassAbilityBookSlotUI> abilitySlots = new List<ClassAbilityBookSlotUI>();

    private bool isOpen;
    private bool isDraggingAbility;
    private AbilityData draggedAbility;

    public bool IsDraggingAbility => isDraggingAbility;
    public AbilityData DraggedAbility => draggedAbility;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }

        HideDragPreview();
    }

    private void Start()
    {
        Close();
        Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }

        if (isOpen)
        {
            RefreshHeader();
        }

        if (isDraggingAbility)
        {
            UpdateDragPreviewPosition();
        }
    }

    public void Toggle()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        isOpen = true;

        if (abilityBookWindow != null)
        {
            abilityBookWindow.SetActive(true);
        }

        Refresh();
    }

    public void Close()
    {
        isOpen = false;

        if (abilityBookWindow != null)
        {
            abilityBookWindow.SetActive(false);
        }

        ClearDragState();
    }

    public void BeginAbilityDrag(AbilityData ability)
    {
        if (ability == null)
        {
            return;
        }

        isDraggingAbility = true;
        draggedAbility = ability;

        ShowDragPreview(ability);
        UpdateDragPreviewPosition();
    }

    public void CompleteAbilityDrag(bool assigned)
    {
        ClearDragState();

        if (assigned)
        {
            Refresh();
        }
    }

    public void AssignAbilityToFirstEmptyHotbarSlot(AbilityData ability)
    {
        if (ability == null || PlayerHotbar.Instance == null)
        {
            return;
        }

        for (int i = 0; i < PlayerHotbar.Instance.SlotCount; i++)
        {
            if (PlayerHotbar.Instance.IsSlotEmpty(i))
            {
                PlayerHotbar.Instance.AssignAbilityToSlot(i, ability);
                PostSystem($"{ability.DisplayName} added to hotbar.");
                return;
            }
        }

        PostSystem("No empty hotbar slot available.");
    }

    private void Refresh()
    {
        RefreshHeader();
        BuildAbilitySlots();
    }

    private void RefreshHeader()
    {
        if (titleText == null)
        {
            return;
        }

        CharacterClassData selectedClass = classController != null
            ? classController.SelectedClass
            : null;

        titleText.text = selectedClass == null
            ? "Class Abilities"
            : $"{selectedClass.ClassName} Abilities";
    }

    private void BuildAbilitySlots()
    {
        if (abilityContainer == null || abilitySlotPrefab == null)
        {
            return;
        }

        for (int i = abilityContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(abilityContainer.GetChild(i).gameObject);
        }

        abilitySlots.Clear();

        CharacterClassData selectedClass = classController != null
            ? classController.SelectedClass
            : null;

        if (selectedClass == null)
        {
            return;
        }

        HashSet<AbilityData> alreadyAdded = new HashSet<AbilityData>();

        IReadOnlyCollection<AbilityData> learnedAbilities = classController.GetLearnedAbilities();

        foreach (AbilityData learnedAbility in learnedAbilities)
        {
            if (learnedAbility == null || alreadyAdded.Contains(learnedAbility))
            {
                continue;
            }

            CreateAbilitySlot(learnedAbility, true, 1);
            alreadyAdded.Add(learnedAbility);
        }

        ClassAbilityUnlock[] unlocks = selectedClass.AbilityUnlocks;

        if (unlocks == null)
        {
            return;
        }

        for (int i = 0; i < unlocks.Length; i++)
        {
            ClassAbilityUnlock unlock = unlocks[i];

            if (unlock == null || unlock.Ability == null)
            {
                continue;
            }

            if (alreadyAdded.Contains(unlock.Ability))
            {
                continue;
            }

            bool isLearned = classController.HasLearnedAbility(unlock.Ability);
            CreateAbilitySlot(unlock.Ability, isLearned, unlock.UnlockLevel);
            alreadyAdded.Add(unlock.Ability);
        }
    }

    private void CreateAbilitySlot(AbilityData ability, bool isLearned, int unlockLevel)
    {
        ClassAbilityBookSlotUI slot = Instantiate(abilitySlotPrefab, abilityContainer);
        slot.gameObject.SetActive(true);
        slot.Initialize(
            ability,
            isLearned,
            unlockLevel,
            HandleAbilityClicked,
            HandleAbilityDragStarted,
            HandleAbilityDragEnded);

        abilitySlots.Add(slot);
    }

    private void HandleAbilityClicked(AbilityData ability)
    {
        AssignAbilityToFirstEmptyHotbarSlot(ability);
    }

    private void HandleAbilityDragStarted(AbilityData ability)
    {
        BeginAbilityDrag(ability);
    }

    private void HandleAbilityDragEnded(AbilityData ability)
    {
        if (isDraggingAbility)
        {
            CompleteAbilityDrag(false);
        }
    }

    private void ShowDragPreview(AbilityData ability)
    {
        if (dragPreviewImage == null || ability == null)
        {
            return;
        }

        dragPreviewImage.gameObject.SetActive(true);
        dragPreviewImage.sprite = ability.Icon;
        dragPreviewImage.enabled = ability.Icon != null;
        dragPreviewImage.raycastTarget = false;
        dragPreviewImage.color = Color.white;

        dragPreviewImage.transform.SetAsLastSibling();
    }

    private void HideDragPreview()
    {
        if (dragPreviewImage != null)
        {
            dragPreviewImage.gameObject.SetActive(false);
            dragPreviewImage.sprite = null;
        }
    }

    private void UpdateDragPreviewPosition()
    {
        if (dragPreviewImage == null)
        {
            return;
        }

        RectTransform previewRect = dragPreviewImage.rectTransform;
        Vector2 screenPosition = (Vector2)Input.mousePosition + dragPreviewOffset;

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            RectTransform canvasRect = rootCanvas.transform as RectTransform;

            if (canvasRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    rootCanvas.worldCamera,
                    out Vector2 localPoint))
            {
                previewRect.localPosition = localPoint;
                return;
            }
        }

        previewRect.position = screenPosition;
    }

    private void ClearDragState()
    {
        isDraggingAbility = false;
        draggedAbility = null;
        HideDragPreview();
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}