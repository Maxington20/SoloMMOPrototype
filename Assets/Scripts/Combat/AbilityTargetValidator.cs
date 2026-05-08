using UnityEngine;

[RequireComponent(typeof(PlayerTargetingController))]
public class AbilityTargetValidator : MonoBehaviour
{
    private PlayerTargetingController targetingController;

    private void Awake()
    {
        targetingController = GetComponent<PlayerTargetingController>();
    }

    public bool CanUseAbilityOnCurrentTarget(string abilityName, float range, bool postMessages)
    {
        Health currentTarget = targetingController != null
            ? targetingController.CurrentTargetHealth
            : null;

        if (currentTarget == null)
        {
            if (postMessages)
            {
                PostSystem("No target.");
            }

            return false;
        }

        if (currentTarget.IsDead)
        {
            ClearTarget();

            if (postMessages)
            {
                PostSystem("Target is dead.");
            }

            return false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (distanceToTarget > range)
        {
            if (postMessages)
            {
                PostSystem($"{abilityName} is out of range.");
            }

            return false;
        }

        return true;
    }

    public void FaceCurrentTarget()
    {
        Transform currentTargetTransform = targetingController != null
            ? targetingController.CurrentTargetTransform
            : null;

        if (currentTargetTransform == null)
        {
            return;
        }

        FaceTarget(currentTargetTransform);
    }

    public void ClearTarget()
    {
        if (targetingController != null)
        {
            targetingController.ClearTarget();
        }
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            10f * Time.deltaTime);
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}