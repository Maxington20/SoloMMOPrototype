using UnityEngine;

public class PlayerItemUseController : MonoBehaviour
{
    [SerializeField] private Health playerHealth;

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<Health>();
        }
    }

    public bool TryUseItem(ItemData item)
    {
        if (item == null || !item.IsUsable)
        {
            return false;
        }

        bool usedSomething = false;

        if (item.HealthRestoreAmount > 0)
        {
            if (playerHealth == null)
            {
                return false;
            }

            if (playerHealth.IsDead)
            {
                PostSystem("You cannot use that while dead.");
                return false;
            }

            if (playerHealth.CurrentHealth >= playerHealth.MaxHealth)
            {
                PostSystem("You are already at full health.");
                return false;
            }

            int restored = playerHealth.RestoreHealth(item.HealthRestoreAmount);
            if (restored > 0)
            {
                usedSomething = true;
                PostSystem($"{item.DisplayName} restores {restored} health.");
            }
        }

        return usedSomething;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}