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

    [Header("Vendor")]
    [SerializeField] private List<ItemData> vendorItems = new List<ItemData>();

    public string DisplayName => displayName;
    public string Title => title;
    public Sprite Portrait => portrait;

    public bool CanGiveQuests => canGiveQuests;
    public bool CanVendor => canVendor;

    public IReadOnlyList<ItemData> VendorItems => vendorItems;

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