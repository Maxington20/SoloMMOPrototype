using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private GameObject emptyStateObject;

    private int slotIndex;

    public void Initialize(int index)
    {
        slotIndex = index;
        gameObject.name = $"InventorySlot_{index}";
    }

    public void Refresh(InventorySlotData slotData)
    {
        if (slotData == null || slotData.IsEmpty)
        {
            SetEmptyVisual();
            return;
        }

        SetFilledVisual(slotData);
    }

    private void SetEmptyVisual()
    {
        if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = string.Empty;
        }

        if (emptyStateObject != null)
        {
            emptyStateObject.SetActive(true);
        }
    }

    private void SetFilledVisual(InventorySlotData slotData)
    {
        if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = slotData.Quantity > 1 ? slotData.Quantity.ToString() : string.Empty;
        }

        if (emptyStateObject != null)
        {
            emptyStateObject.SetActive(false);
        }
    }
}