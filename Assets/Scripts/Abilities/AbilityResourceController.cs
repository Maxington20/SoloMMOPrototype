using UnityEngine;

[RequireComponent(typeof(PlayerResource))]
public class AbilityResourceController : MonoBehaviour
{
    private PlayerResource playerResource;

    private void Awake()
    {
        playerResource = GetComponent<PlayerResource>();
    }

    public bool CanPayResourceCost(AbilityData ability)
    {
        if (ability == null || ability.ResourceCost <= 0)
        {
            return true;
        }

        if (playerResource == null || !playerResource.HasResource)
        {
            return false;
        }

        return playerResource.HasEnoughResource(ability.ResourceCost);
    }

    public void SpendResourceCost(AbilityData ability)
    {
        if (ability == null || ability.ResourceCost <= 0)
        {
            return;
        }

        if (playerResource != null)
        {
            playerResource.TrySpendResource(ability.ResourceCost);
        }
    }

    public void GenerateResourceFromAbility(AbilityData ability)
    {
        if (ability == null || ability.ResourceGenerated <= 0)
        {
            return;
        }

        if (playerResource != null)
        {
            playerResource.GenerateResource(ability.ResourceGenerated);
        }
    }

    public string GetResourceName()
    {
        if (playerResource == null || !playerResource.HasResource)
        {
            return "resource";
        }

        return playerResource.ResourceDisplayName.ToLower();
    }
}