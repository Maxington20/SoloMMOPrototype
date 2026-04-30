using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterUI : MonoBehaviour
{
    public static CharacterUI Instance { get; private set; }

    [Header("Window")]
    [SerializeField] private GameObject characterWindow;

    [Header("Header")]
    [SerializeField] private TMP_Text headerText;

    [Header("Stats Text")]
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text armorText;
    [SerializeField] private TMP_Text dodgeText;
    [SerializeField] private TMP_Text hitText;
    [SerializeField] private TMP_Text primaryStatText;
    [SerializeField] private TMP_Text strengthText;
    [SerializeField] private TMP_Text agilityText;
    [SerializeField] private TMP_Text intellectText;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private TMP_Text goldText;

    [Header("Equipment Slots")]
    [SerializeField] private EquipmentSlotUI headSlot;
    [SerializeField] private EquipmentSlotUI chestSlot;
    [SerializeField] private EquipmentSlotUI legsSlot;
    [SerializeField] private EquipmentSlotUI feetSlot;
    [SerializeField] private EquipmentSlotUI weaponSlot;
    [SerializeField] private EquipmentSlotUI offhandSlot;

    [Header("Player References")]
    [SerializeField] private DisplayName displayName;
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private Health health;
    [SerializeField] private PlayerResource playerResource;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerEquipment equipment;
    [SerializeField] private PlayerClassController classController;
    [SerializeField] private PlayerStats playerStats;

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;

    private bool isOpen;

    private bool isDraggingEquipment;
    private bool dropHandledThisDrag;
    private EquipmentSlotType draggedEquipmentSlotType = EquipmentSlotType.None;

    private Canvas rootCanvas;
    private RectTransform canvasRectTransform;

    private EquipmentSlotUI draggedSlotUI;
    private Transform draggedOriginalParent;
    private int draggedOriginalSiblingIndex;

    private GameObject dragPlaceholderObject;

    public bool IsDraggingEquipmentItem => isDraggingEquipment;
    public EquipmentSlotType DraggedEquipmentSlotType => draggedEquipmentSlotType;

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
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
        {
            canvasRectTransform = rootCanvas.transform as RectTransform;
        }

        if (characterWindow != null)
        {
            characterWindow.SetActive(false);
        }

        if (headSlot != null) headSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (chestSlot != null) chestSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (legsSlot != null) legsSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (feetSlot != null) feetSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (weaponSlot != null) weaponSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
        if (offhandSlot != null) offhandSlot.Initialize(HandleEquipmentSlotLeftClicked, HandleEquipmentSlotRightClicked, HandleEquipmentSlotBeginDrag, HandleEquipmentSlotDrag, HandleEquipmentSlotEndDrag, HandleEquipmentSlotDrop);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }

        if (isOpen)
        {
            Refresh();
        }

        if (isDraggingEquipment)
        {
            UpdateDraggedSlotPosition(Input.mousePosition);
        }
    }

    public void CompleteExternalDrop(bool moved)
    {
        if (!isDraggingEquipment)
        {
            return;
        }

        dropHandledThisDrag = true;

        RestoreDraggedSlotVisual();

        if (moved)
        {
            Refresh();
        }

        ClearDragState();
    }

    private void Toggle()
    {
        isOpen = !isOpen;

        if (characterWindow != null)
        {
            characterWindow.SetActive(isOpen);
        }

        if (!isOpen)
        {
            if (ItemContextMenuUI.Instance != null)
            {
                ItemContextMenuUI.Instance.Hide();
            }

            if (ItemTooltipUI.Instance != null)
            {
                ItemTooltipUI.Instance.Hide();
            }

            if (isDraggingEquipment)
            {
                RestoreDraggedSlotVisual();
                ClearDragState();
            }
        }

        if (isOpen)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        CharacterClassData selectedClass = classController != null
            ? classController.SelectedClass
            : null;

        string playerName = displayName != null ? displayName.Display : "Unknown";
        string className = selectedClass != null ? selectedClass.ClassName : "No Class";
        int level = progression != null ? progression.Level : 1;

        if (headerText != null)
        {
            headerText.text = $"{playerName} — Level {level} {className}";
        }

        if (roleText != null)
        {
            roleText.text = selectedClass != null ? $"Role: {selectedClass.Role}" : "Role: None";
        }

        if (healthText != null)
        {
            healthText.text = health != null ? $"Health: {health.CurrentHealth}/{health.MaxHealth}" : "Health: ?";
        }

        if (manaText != null)
        {
            if (playerResource != null && playerResource.HasManaResource)
            {
                manaText.text = $"Mana: {playerResource.CurrentMana}/{playerResource.MaxMana}";
            }
            else
            {
                manaText.text = "Mana: None";
            }
        }

        if (damageText != null)
        {
            damageText.text = combat != null ? $"Damage: {combat.Damage}" : "Damage: ?";
        }

        if (goldText != null)
        {
            goldText.text = inventory != null ? $"Gold: {inventory.Gold}" : "Gold: 0";
        }

        RefreshStatsText();
        RefreshEquipmentSlots();
    }

    private void RefreshStatsText()
    {
        if (playerStats == null)
        {
            SetStatTextsToUnknown();
            return;
        }

        StatBlock total = playerStats.TotalStats;
        StatBlock baseStats = playerStats.BaseStats;
        StatBlock levelStats = playerStats.LevelBonusStats;
        StatBlock gearStats = playerStats.GearStats;

        CharacterClassData selectedClass = classController != null
            ? classController.SelectedClass
            : null;

        if (armorText != null)
        {
            armorText.text = BuildStatBreakdownLine(
                "Armour",
                total.Armor,
                baseStats.Armor,
                levelStats.Armor,
                gearStats.Armor);
        }

        if (dodgeText != null)
        {
            dodgeText.text = $"Dodge: {playerStats.DodgeChancePercent:0.#}%";
        }

        if (hitText != null)
        {
            hitText.text = $"Hit: {playerStats.HitChancePercent:0.#}%";
        }

        if (primaryStatText != null)
        {
            primaryStatText.text = selectedClass != null
                ? $"Primary Stat: {selectedClass.PrimaryStat}"
                : "Primary Stat: None";
        }

        if (strengthText != null)
        {
            strengthText.text = BuildStatBreakdownLine(
                "Strength",
                total.Strength,
                baseStats.Strength,
                levelStats.Strength,
                gearStats.Strength);
        }

        if (agilityText != null)
        {
            agilityText.text = BuildStatBreakdownLine(
                "Agility",
                total.Agility,
                baseStats.Agility,
                levelStats.Agility,
                gearStats.Agility);
        }

        if (intellectText != null)
        {
            intellectText.text = BuildStatBreakdownLine(
                "Intellect",
                total.Intellect,
                baseStats.Intellect,
                levelStats.Intellect,
                gearStats.Intellect);
        }

        if (staminaText != null)
        {
            staminaText.text = BuildStatBreakdownLine(
                "Stamina",
                total.Stamina,
                baseStats.Stamina,
                levelStats.Stamina,
                gearStats.Stamina);
        }
    }

    private string BuildStatBreakdownLine(string label, int total, int baseValue, int levelValue, int gearValue)
    {
        return $"{label}: {total}  <size=75%><color=#BDBDBD>(Base {baseValue} + Level {levelValue} + Gear {gearValue})</color></size>";
    }

    private void SetStatTextsToUnknown()
    {
        if (armorText != null) armorText.text = "Armour: ?";
        if (dodgeText != null) dodgeText.text = "Dodge: ?";
        if (hitText != null) hitText.text = "Hit: ?";
        if (primaryStatText != null) primaryStatText.text = "Primary Stat: ?";
        if (strengthText != null) strengthText.text = "Strength: ?";
        if (agilityText != null) agilityText.text = "Agility: ?";
        if (intellectText != null) intellectText.text = "Intellect: ?";
        if (staminaText != null) staminaText.text = "Stamina: ?";
    }

    private void RefreshEquipmentSlots()
    {
        if (equipment == null)
        {
            return;
        }

        if (headSlot != null) headSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Head));
        if (chestSlot != null) chestSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Chest));
        if (legsSlot != null) legsSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Legs));
        if (feetSlot != null) feetSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Feet));
        if (weaponSlot != null) weaponSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Weapon));

        if (offhandSlot != null)
        {
            ItemData equippedWeapon = equipment.GetEquippedItem(EquipmentSlotType.Weapon);

            if (equipment.IsOffhandBlockedByTwoHandedWeapon())
            {
                offhandSlot.RefreshBlockedByTwoHandedWeapon(equippedWeapon);
            }
            else
            {
                offhandSlot.Refresh(equipment.GetEquippedItem(EquipmentSlotType.Offhand));
            }
        }
    }

    private void HandleEquipmentSlotLeftClicked(EquipmentSlotType slotType)
    {
        // Left click is reserved for drag-and-drop.
    }

    private void HandleEquipmentSlotRightClicked(EquipmentSlotType slotType, Vector2 screenPosition)
    {
        if (!isOpen || isDraggingEquipment || equipment == null || ItemContextMenuUI.Instance == null)
        {
            return;
        }

        if (slotType == EquipmentSlotType.Offhand && equipment.IsOffhandBlockedByTwoHandedWeapon())
        {
            return;
        }

        ItemData item = equipment.GetEquippedItem(slotType);
        if (item == null)
        {
            ItemContextMenuUI.Instance.Hide();
            return;
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        ItemContextMenuUI.Instance.OpenForEquipmentSlot(slotType, item, screenPosition);
    }

    private void HandleEquipmentSlotBeginDrag(EquipmentSlotType slotType, PointerEventData eventData)
    {
        if (!isOpen || equipment == null || rootCanvas == null)
        {
            return;
        }

        if (slotType == EquipmentSlotType.Offhand && equipment.IsOffhandBlockedByTwoHandedWeapon())
        {
            return;
        }

        ItemData item = equipment.GetEquippedItem(slotType);
        if (item == null)
        {
            return;
        }

        EquipmentSlotUI slotUI = GetSlotUI(slotType);
        if (slotUI == null)
        {
            return;
        }

        isDraggingEquipment = true;
        dropHandledThisDrag = false;
        draggedEquipmentSlotType = slotType;
        draggedSlotUI = slotUI;
        draggedOriginalParent = slotUI.transform.parent;
        draggedOriginalSiblingIndex = slotUI.transform.GetSiblingIndex();

        if (ItemContextMenuUI.Instance != null)
        {
            ItemContextMenuUI.Instance.Hide();
        }

        if (ItemTooltipUI.Instance != null)
        {
            ItemTooltipUI.Instance.Hide();
        }

        CreateDragPlaceholder(slotUI);

        draggedSlotUI.SetDraggingVisualState(true);
        draggedSlotUI.transform.SetParent(rootCanvas.transform, true);
        draggedSlotUI.transform.SetAsLastSibling();

        UpdateDraggedSlotPosition(eventData.position);
    }

    private void HandleEquipmentSlotDrag(PointerEventData eventData)
    {
        if (!isDraggingEquipment)
        {
            return;
        }

        UpdateDraggedSlotPosition(eventData.position);
    }

    private void HandleEquipmentSlotEndDrag(EquipmentSlotType slotType, PointerEventData eventData)
    {
        if (!isDraggingEquipment)
        {
            return;
        }

        if (!dropHandledThisDrag)
        {
            RestoreDraggedSlotVisual();
            ClearDragState();
        }
    }

    private void HandleEquipmentSlotDrop(EquipmentSlotType targetSlotType, PointerEventData eventData)
    {
        if (equipment == null)
        {
            return;
        }

        if (InventoryUI.Instance != null && InventoryUI.Instance.IsDraggingInventoryItem)
        {
            bool moved = PlayerInventory.Instance.TryEquipFromSlotToEquipmentSlot(
                InventoryUI.Instance.DraggedInventorySlotIndex,
                targetSlotType);

            InventoryUI.Instance.CompleteExternalDrop(moved);

            if (moved)
            {
                Refresh();
            }

            return;
        }

        if (isDraggingEquipment)
        {
            bool moved = equipment.MoveEquippedItemToSlot(
                draggedEquipmentSlotType,
                targetSlotType);

            dropHandledThisDrag = true;

            RestoreDraggedSlotVisual();

            if (moved)
            {
                Refresh();
            }

            ClearDragState();
        }
    }

    private EquipmentSlotUI GetSlotUI(EquipmentSlotType slotType)
    {
        return slotType switch
        {
            EquipmentSlotType.Head => headSlot,
            EquipmentSlotType.Chest => chestSlot,
            EquipmentSlotType.Legs => legsSlot,
            EquipmentSlotType.Feet => feetSlot,
            EquipmentSlotType.Weapon => weaponSlot,
            EquipmentSlotType.Offhand => offhandSlot,
            _ => null
        };
    }

    private void UpdateDraggedSlotPosition(Vector2 screenPosition)
    {
        if (!isDraggingEquipment || draggedSlotUI == null || draggedSlotUI.RectTransform == null)
        {
            return;
        }

        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            if (canvasRectTransform != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRectTransform,
                    screenPosition,
                    rootCanvas.worldCamera,
                    out Vector2 localPoint))
            {
                draggedSlotUI.RectTransform.localPosition = localPoint;
                return;
            }
        }

        draggedSlotUI.RectTransform.position = screenPosition;
    }

    private void CreateDragPlaceholder(EquipmentSlotUI slotUI)
    {
        if (slotUI == null || draggedOriginalParent == null)
        {
            return;
        }

        DestroyDragPlaceholder();

        dragPlaceholderObject = Instantiate(slotUI.gameObject, draggedOriginalParent);
        dragPlaceholderObject.transform.SetSiblingIndex(draggedOriginalSiblingIndex);
        dragPlaceholderObject.name = "EquipmentDragPlaceholder";

        RectTransform sourceRect = slotUI.GetComponent<RectTransform>();
        RectTransform placeholderRect = dragPlaceholderObject.GetComponent<RectTransform>();

        if (sourceRect != null && placeholderRect != null)
        {
            placeholderRect.anchorMin = sourceRect.anchorMin;
            placeholderRect.anchorMax = sourceRect.anchorMax;
            placeholderRect.pivot = sourceRect.pivot;
            placeholderRect.anchoredPosition = sourceRect.anchoredPosition;
            placeholderRect.sizeDelta = sourceRect.sizeDelta;
            placeholderRect.localScale = sourceRect.localScale;
            placeholderRect.localRotation = sourceRect.localRotation;
        }

        EquipmentSlotUI placeholderSlot = dragPlaceholderObject.GetComponent<EquipmentSlotUI>();
        if (placeholderSlot != null)
        {
            placeholderSlot.Refresh(null);
            placeholderSlot.SetDraggingVisualState(false);
            placeholderSlot.enabled = false;
        }

        CanvasGroup placeholderCanvasGroup = dragPlaceholderObject.GetComponent<CanvasGroup>();
        if (placeholderCanvasGroup != null)
        {
            placeholderCanvasGroup.blocksRaycasts = false;
            placeholderCanvasGroup.interactable = false;
        }
    }

    private void RestoreDraggedSlotVisual()
    {
        if (draggedSlotUI == null)
        {
            DestroyDragPlaceholder();
            return;
        }

        draggedSlotUI.transform.SetParent(draggedOriginalParent, false);

        RectTransform draggedRect = draggedSlotUI.GetComponent<RectTransform>();

        if (dragPlaceholderObject != null)
        {
            RectTransform placeholderRect = dragPlaceholderObject.GetComponent<RectTransform>();
            int placeholderSiblingIndex = dragPlaceholderObject.transform.GetSiblingIndex();
            draggedSlotUI.transform.SetSiblingIndex(placeholderSiblingIndex);

            if (draggedRect != null && placeholderRect != null)
            {
                draggedRect.anchorMin = placeholderRect.anchorMin;
                draggedRect.anchorMax = placeholderRect.anchorMax;
                draggedRect.pivot = placeholderRect.pivot;
                draggedRect.anchoredPosition = placeholderRect.anchoredPosition;
                draggedRect.sizeDelta = placeholderRect.sizeDelta;
                draggedRect.localScale = placeholderRect.localScale;
                draggedRect.localRotation = placeholderRect.localRotation;
            }
        }
        else
        {
            draggedSlotUI.transform.SetSiblingIndex(draggedOriginalSiblingIndex);
        }

        draggedSlotUI.SetDraggingVisualState(false);
        DestroyDragPlaceholder();
    }

    private void DestroyDragPlaceholder()
    {
        if (dragPlaceholderObject != null)
        {
            Destroy(dragPlaceholderObject);
            dragPlaceholderObject = null;
        }
    }

    private void ClearDragState()
    {
        isDraggingEquipment = false;
        dropHandledThisDrag = false;
        draggedEquipmentSlotType = EquipmentSlotType.None;
        draggedSlotUI = null;
        draggedOriginalParent = null;
        draggedOriginalSiblingIndex = -1;
    }
}