using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public event Action<int> OnGoldChanged;
    public event Action OnInventoryChanged;

    [Header("Gold")]
    [SerializeField] private int startingGold = 0;

    [Header("Inventory")]
    [SerializeField] private int slotCount = 20;

    private List<InventorySlotData> slots = new List<InventorySlotData>();

    public int Gold { get; private set; }
    public int SlotCount => slotCount;
    public IReadOnlyList<InventorySlotData> Slots => slots;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Gold = Mathf.Max(0, startingGold);
        InitializeSlots();
    }

    private void Start()
    {
        OnGoldChanged?.Invoke(Gold);
        OnInventoryChanged?.Invoke();
    }

    private void InitializeSlots()
    {
        slots.Clear();

        int finalSlotCount = Mathf.Max(1, slotCount);

        for (int i = 0; i < finalSlotCount; i++)
        {
            slots.Add(new InventorySlotData());
        }
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Gold += amount;
        OnGoldChanged?.Invoke(Gold);

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You loot {amount} gold.");
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    public InventorySlotData GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return null;
        }

        return slots[index];
    }

    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    // Temporary test helper so you can see something in the UI later if needed.
    public void AddTestItemToFirstSlot()
    {
        if (slots.Count == 0)
        {
            return;
        }

        slots[0].IsEmpty = false;
        slots[0].ItemId = "test_item";
        slots[0].Quantity = 1;

        OnInventoryChanged?.Invoke();
    }

    public void ClearAllSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Clear();
        }

        OnInventoryChanged?.Invoke();
    }
}