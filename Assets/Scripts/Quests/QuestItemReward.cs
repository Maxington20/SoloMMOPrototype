using System;
using UnityEngine;

[Serializable]
public class QuestItemReward
{
    [SerializeField] private ItemData item;
    [SerializeField] private int quantity = 1;

    public ItemData Item => item;
    public int Quantity => Mathf.Max(1, quantity);
}