using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform movingRoot;

    [Header("Pickup Hold")]
    [SerializeField] private float pickupPauseDelay = 0.45f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int PickupHash = Animator.StringToHash("Pickup");

    private Vector3 lastPosition;

    private bool waitingToPausePickup;
    private bool holdingPickupPose;
    private float pickupStartedAt;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (movingRoot == null)
            movingRoot = transform;

        lastPosition = movingRoot.position;
    }

    private void Update()
    {
        UpdateMovementSpeed();
        UpdatePickupHoldTimer();
    }

    private void UpdateMovementSpeed()
    {
        if (animator == null || movingRoot == null)
            return;

        Vector3 movement = movingRoot.position - lastPosition;
        movement.y = 0f;

        float speed = movement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        animator.SetFloat(SpeedHash, speed);

        lastPosition = movingRoot.position;
    }

    private void UpdatePickupHoldTimer()
    {
        if (animator == null || !waitingToPausePickup || holdingPickupPose)
            return;

        if (Time.time - pickupStartedAt >= pickupPauseDelay)
        {
            holdingPickupPose = true;
            waitingToPausePickup = false;
            animator.speed = 0f;
        }
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        animator.speed = 1f;
        animator.SetTrigger(AttackHash);
    }

    public void StartPickupHold()
    {
        if (animator == null)
            return;

        holdingPickupPose = false;
        waitingToPausePickup = true;
        pickupStartedAt = Time.time;

        animator.speed = 1f;
        animator.ResetTrigger(PickupHash);
        animator.SetTrigger(PickupHash);
    }

    public void FinishPickupHold()
    {
        if (animator == null)
            return;

        holdingPickupPose = false;
        waitingToPausePickup = false;
        animator.speed = 1f;
    }
}