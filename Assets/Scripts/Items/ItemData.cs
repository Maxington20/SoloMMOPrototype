using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Solo MMO/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string displayName = "New Item";
    [SerializeField] private ItemType itemType = ItemType.Junk;
    [SerializeField] private Sprite icon;
    [SerializeField] private int sellValue = 1;
    [SerializeField] private int buyValue = 5;

    [Header("Stacking")]
    [SerializeField] private bool isStackable = false;
    [SerializeField] private int maxStack = 1;

    [Header("Equipment")]
    [SerializeField] private bool isEquippable = false;
    [SerializeField] private EquipmentSlotType equipmentSlot = EquipmentSlotType.None;
    [SerializeField] private int damageBonus = 0;
    [SerializeField] private int healthBonus = 0;

    [Header("Usable / Consumable")]
    [SerializeField] private bool isUsable = false;
    [SerializeField] private int healthRestoreAmount = 0;

    public string DisplayName => displayName;
    public ItemType ItemType => itemType;
    public Sprite Icon => icon;
    public int SellValue => Mathf.Max(0, sellValue);
    public int BuyValue => Mathf.Max(0, buyValue);
    public bool IsStackable => isStackable;
    public int MaxStack => Mathf.Max(1, maxStack);
    public bool IsEquippable => isEquippable;
    public EquipmentSlotType EquipmentSlot => equipmentSlot;
    public int DamageBonus => damageBonus;
    public int HealthBonus => healthBonus;
    public bool IsUsable => isUsable;
    public int HealthRestoreAmount => Mathf.Max(0, healthRestoreAmount);
}