using System;
using UnityEngine;

[Serializable]
public class EnemyLootTableEntry
{
    [SerializeField] private ItemData item;
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.5f;
    [SerializeField] private int minQuantity = 1;
    [SerializeField] private int maxQuantity = 1;

    public ItemData Item => item;
    public float DropChance => Mathf.Clamp01(dropChance);
    public int MinQuantity => Mathf.Max(1, minQuantity);
    public int MaxQuantity => Mathf.Max(MinQuantity, maxQuantity);
}