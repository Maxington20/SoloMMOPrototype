using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWorldInteractable", menuName = "Solo MMO/World Interactable")]
public class WorldInteractableData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string displayName = "Interactable";
    [TextArea(2, 5)]
    [SerializeField] private string interactionMessage = "You interact with the object.";

    [Header("Interaction")]
    [SerializeField] private bool canOnlyBeUsedOnce = true;
    [SerializeField] private bool hideAfterUse = false;

    [Header("Rewards")]
    [SerializeField] private int goldReward;
    [SerializeField] private List<WorldInteractableItemReward> itemRewards = new List<WorldInteractableItemReward>();

    public string DisplayName => displayName;
    public string InteractionMessage => interactionMessage;

    public bool CanOnlyBeUsedOnce => canOnlyBeUsedOnce;
    public bool HideAfterUse => hideAfterUse;

    public int GoldReward => Mathf.Max(0, goldReward);
    public IReadOnlyList<WorldInteractableItemReward> ItemRewards => itemRewards;
}