using System.Collections.Generic;
using UnityEngine;

public class VendorNPC : MonoBehaviour
{
    [Header("Fallback Vendor")]
    [SerializeField] private string vendorName = "Merchant";
    [SerializeField] private List<ItemData> fallbackItemsForSale = new List<ItemData>();

    private readonly List<ItemData> activeItemsForSale = new List<ItemData>();

    public string VendorName => vendorName;
    public IReadOnlyList<ItemData> ItemsForSale => activeItemsForSale;

    private void Awake()
    {
        RebuildActiveItemsFromFallback();
    }

    public void ApplyNpcData(NpcData npcData)
    {
        activeItemsForSale.Clear();

        if (npcData == null)
        {
            RebuildActiveItemsFromFallback();
            return;
        }

        if (!string.IsNullOrWhiteSpace(npcData.DisplayName))
        {
            vendorName = npcData.DisplayName;
        }

        if (npcData.VendorItems != null)
        {
            for (int i = 0; i < npcData.VendorItems.Count; i++)
            {
                ItemData item = npcData.VendorItems[i];

                if (item != null)
                {
                    activeItemsForSale.Add(item);
                }
            }
        }
    }

    public void OpenVendor()
    {
        if (VendorUI.Instance == null)
        {
            PostSystem("Vendor UI is missing.");
            return;
        }

        if (activeItemsForSale.Count == 0)
        {
            PostSystem($"{vendorName} has nothing for sale.");
        }

        VendorUI.Instance.OpenVendor(this);
    }

    private void RebuildActiveItemsFromFallback()
    {
        activeItemsForSale.Clear();

        if (fallbackItemsForSale == null)
        {
            return;
        }

        for (int i = 0; i < fallbackItemsForSale.Count; i++)
        {
            ItemData item = fallbackItemsForSale[i];

            if (item != null)
            {
                activeItemsForSale.Add(item);
            }
        }
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}