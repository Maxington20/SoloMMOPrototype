using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LootDrop : MonoBehaviour
{
    private int goldAmount;
    private bool collected;

    public void Initialize(int amount)
    {
        goldAmount = Mathf.Max(0, amount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();
        if (inventory == null) return;

        collected = true;

        inventory.AddGold(goldAmount);
        Destroy(gameObject);
    }
}