using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public event Action<int> OnGoldChanged;

    [SerializeField] private int startingGold = 0;

    public int Gold { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Gold = Mathf.Max(0, startingGold);
    }

    private void Start()
    {
        OnGoldChanged?.Invoke(Gold);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        Gold += amount;
        OnGoldChanged?.Invoke(Gold);

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem($"You loot {amount} gold.");
        }
    }
}