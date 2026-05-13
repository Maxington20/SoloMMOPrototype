using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPC", menuName = "Solo MMO/NPC")]
public class NpcData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string displayName = "NPC";
    [SerializeField] private string title = "";
    [SerializeField] private Sprite portrait;

    [Header("Roles")]
    [SerializeField] private bool canGiveQuests;
    [SerializeField] private bool canVendor;
    [SerializeField] private bool canTrain;

    [Header("Quests")]
    [SerializeField] private List<QuestDefinition> quests = new List<QuestDefinition>();

    [Header("Vendor")]
    [SerializeField] private List<ItemData> vendorItems = new List<ItemData>();

    [Header("Trainer")]
    [SerializeField] private CharacterClassData trainerClassRestriction;
    [SerializeField] private List<ClassAbilityUnlock> trainableAbilities = new List<ClassAbilityUnlock>();

    public string DisplayName => displayName;
    public string Title => title;
    public Sprite Portrait => portrait;

    public bool CanGiveQuests => canGiveQuests;
    public bool CanVendor => canVendor;
    public bool CanTrain => canTrain;

    public IReadOnlyList<QuestDefinition> Quests => quests;
    public IReadOnlyList<ItemData> VendorItems => vendorItems;

    public CharacterClassData TrainerClassRestriction => trainerClassRestriction;
    public IReadOnlyList<ClassAbilityUnlock> TrainableAbilities => trainableAbilities;

    public string FullDisplayName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return displayName;
            }

            return $"{displayName}\n{title}";
        }
    }
}