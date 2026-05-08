using UnityEngine;

[RequireComponent(typeof(AbilityCooldownController))]
[RequireComponent(typeof(AbilityResourceController))]
[RequireComponent(typeof(AbilityTargetValidator))]
[RequireComponent(typeof(AbilityExecutor))]
public class AbilityUseValidator : MonoBehaviour
{
    private AbilityCooldownController cooldownController;
    private AbilityResourceController resourceController;
    private AbilityTargetValidator targetValidator;
    private AbilityExecutor abilityExecutor;

    private void Awake()
    {
        cooldownController = GetComponent<AbilityCooldownController>();
        resourceController = GetComponent<AbilityResourceController>();
        targetValidator = GetComponent<AbilityTargetValidator>();
        abilityExecutor = GetComponent<AbilityExecutor>();
    }

    public bool CanBeginAbility(AbilityData ability, bool postMessages)
    {
        if (ability == null)
        {
            return false;
        }

        float cooldownRemaining = cooldownController != null
            ? cooldownController.GetRemainingCooldown(ability)
            : 0f;

        if (cooldownRemaining > 0f)
        {
            if (postMessages)
            {
                PostSystem($"{ability.DisplayName} is on cooldown for {Mathf.CeilToInt(cooldownRemaining)} more second(s).");
            }

            return false;
        }

        if (resourceController != null && !resourceController.CanPayResourceCost(ability))
        {
            if (postMessages)
            {
                PostSystem($"Not enough {resourceController.GetResourceName()} for {ability.DisplayName}.");
            }

            return false;
        }

        if (ability.RequiresTarget)
        {
            return targetValidator != null &&
                   targetValidator.CanUseAbilityOnCurrentTarget(
                       ability.DisplayName,
                       ability.Range,
                       postMessages);
        }

        return abilityExecutor != null &&
               abilityExecutor.CanExecuteSelfAbility(ability, postMessages);
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}