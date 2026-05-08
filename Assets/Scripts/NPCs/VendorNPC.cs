using System.Collections.Generic;
using UnityEngine;

public class VendorNPC : MonoBehaviour
{
    [Header("Vendor")]
    [SerializeField] private string vendorName = "Merchant";
    [SerializeField] private List<ItemData> itemsForSale = new List<ItemData>();

    public string VendorName => vendorName;
    public IReadOnlyList<ItemData> ItemsForSale => itemsForSale;

    public void ApplyNpcData(NpcData npcData)
    {
        if (npcData == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(npcData.DisplayName))
        {
            vendorName = npcData.DisplayName;
        }

        if (npcData.VendorItems != null && npcData.VendorItems.Count > 0)
        {
            itemsForSale = new List<ItemData>(npcData.VendorItems);
        }
    }

    public void OpenVendor()
    {
        if (VendorUI.Instance == null)
        {
            PostSystem("Vendor UI is missing.");
            return;
        }

        VendorUI.Instance.OpenVendor(this);
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}