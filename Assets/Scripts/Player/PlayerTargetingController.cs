using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerTargetingController : MonoBehaviour
{
    private Health currentTarget;
    private EnemyController currentEnemyTarget;

    public Health CurrentTargetHealth => currentTarget;
    public EnemyController CurrentEnemyTarget => currentEnemyTarget;
    public Transform CurrentTargetTransform => currentTarget != null ? currentTarget.transform : null;
    public bool HasTarget => currentTarget != null;

    private void Update()
    {
        HandleTargetSelection();
    }

    public void ClearTarget()
    {
        currentTarget = null;
        currentEnemyTarget = null;
    }

    public bool CurrentTargetIsValid()
    {
        return currentTarget != null && !currentTarget.IsDead;
    }

    private void HandleTargetSelection()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            ClearTarget();
            return;
        }

        if (TryHandleInteractableClick(hit))
        {
            return;
        }

        TryHandleEnemyTargetClick(hit);
    }

    private bool TryHandleInteractableClick(RaycastHit hit)
    {
        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null)
        {
            return false;
        }

        if (interactable is EnemyLoot enemyLoot && !enemyLoot.CanBeLooted)
        {
            return false;
        }

        ClearTarget();
        interactable.Interact(transform);
        return true;
    }

    private void TryHandleEnemyTargetClick(RaycastHit hit)
    {
        Health targetHealth = hit.collider.GetComponentInParent<Health>();

        if (targetHealth == null || targetHealth.IsDead)
        {
            ClearTarget();
            return;
        }

        EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();

        if (enemy == null)
        {
            ClearTarget();
            return;
        }

        currentTarget = targetHealth;
        currentEnemyTarget = enemy;

        string targetName = GetTargetDisplayName(targetHealth.gameObject);

        Debug.Log($"Target selected: {targetName}");
        PostSystem($"Target selected: {targetName}.");
    }

    private string GetTargetDisplayName(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return "target";
        }

        DisplayName displayName = targetObject.GetComponent<DisplayName>();
        return displayName != null ? displayName.Display : targetObject.name;
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}