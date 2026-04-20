using UnityEngine;

public class PlayerStartingItems : MonoBehaviour
{
    [SerializeField] private ItemData[] startingItems;

    private bool granted;

    private void Start()
    {
        if (granted)
        {
            return;
        }

        PlayerInventory inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            return;
        }

        for (int i = 0; i < startingItems.Length; i++)
        {
            if (startingItems[i] != null)
            {
                inventory.AddItem(startingItems[i], 1);
            }
        }

        granted = true;
    }
}