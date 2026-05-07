using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EnemyMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Wandering")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderStopDistance = 0.2f;
    [SerializeField] private float minIdleTimeBetweenWanders = 1.5f;
    [SerializeField] private float maxIdleTimeBetweenWanders = 4f;

    private CharacterController characterController;
    private StatusEffectController statusEffectController;

    private Vector3 verticalVelocity;
    private Vector3 homePosition;

    private Vector3 wanderDestination;
    private bool hasWanderDestination;
    private float wanderIdleTimer;

    public bool HasWanderDestination => hasWanderDestination;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        statusEffectController = GetComponent<StatusEffectController>();
        homePosition = transform.position;
    }

    private void Start()
    {
        ResetWanderTimer();
    }

    public void SetHomePosition(Vector3 position)
    {
        homePosition = position;
    }

    public void TickGravity()
    {
        if (characterController == null || !characterController.enabled)
        {
            return;
        }

        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
    }

    public void ResetVerticalVelocity()
    {
        verticalVelocity = Vector3.zero;
    }

    public void MoveTowardPosition(Vector3 destination)
    {
        if (characterController == null || !characterController.enabled)
        {
            return;
        }

        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        direction.Normalize();

        float movementMultiplier = statusEffectController != null
            ? statusEffectController.MovementSpeedMultiplier
            : 1f;

        Vector3 movement = direction * moveSpeed * movementMultiplier;
        characterController.Move(movement * Time.deltaTime);

        FaceDirection(direction);
    }

    public void FacePosition(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        FaceDirection(direction.normalized);
    }

    public void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    public void TickWandering()
    {
        if (hasWanderDestination)
        {
            float distanceToDestination = Vector3.Distance(transform.position, wanderDestination);

            if (distanceToDestination <= wanderStopDistance)
            {
                ClearWanderDestination();
                ResetWanderTimer();
                return;
            }

            MoveTowardPosition(wanderDestination);
            return;
        }

        wanderIdleTimer -= Time.deltaTime;

        if (wanderIdleTimer <= 0f)
        {
            PickNewWanderDestination();
        }
    }

    public void ClearWanderDestination()
    {
        hasWanderDestination = false;
    }

    public void ResetWanderTimer()
    {
        wanderIdleTimer = Random.Range(minIdleTimeBetweenWanders, maxIdleTimeBetweenWanders);
    }

    private void PickNewWanderDestination()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        wanderDestination = homePosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
        hasWanderDestination = true;
    }
}