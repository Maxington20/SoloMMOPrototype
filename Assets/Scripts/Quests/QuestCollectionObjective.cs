using System;
using UnityEngine;

[Serializable]
public class QuestCollectionObjective
{
    [SerializeField] private ItemData item;
    [SerializeField] private int requiredAmount = 1;

    public ItemData Item => item;
    public int RequiredAmount => Mathf.Max(1, requiredAmount);
}