using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private float yOffset = 0.05f;
    [SerializeField] private Renderer indicatorRenderer;

    private void Awake()
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponentInChildren<Renderer>();
        }

        SetVisible(false);
    }

    private void Update()
    {
        if (playerCombat == null)
        {
            SetVisible(false);
            return;
        }

        Transform target = playerCombat.CurrentTargetTransform;

        if (target == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        MoveToTargetFeet(target);
    }

    private void MoveToTargetFeet(Transform target)
    {
        CharacterController characterController = target.GetComponent<CharacterController>();

        if (characterController != null)
        {
            Vector3 feetPosition = target.position + characterController.center;
            feetPosition.y -= (characterController.height * 0.5f);
            feetPosition.y += yOffset;

            transform.position = feetPosition;
            return;
        }

        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider != null)
        {
            Vector3 boundsPosition = targetCollider.bounds.center;
            boundsPosition.y = targetCollider.bounds.min.y + yOffset;

            transform.position = boundsPosition;
            return;
        }

        Vector3 fallbackPosition = target.position;
        fallbackPosition.y += yOffset;
        transform.position = fallbackPosition;
    }

    private void SetVisible(bool visible)
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.enabled = visible;
        }
    }
}