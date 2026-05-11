using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform movingRoot;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Vector3 lastPosition;

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
        if (animator == null || movingRoot == null)
            return;

        Vector3 movement = movingRoot.position - lastPosition;
        movement.y = 0f;

        float speed = movement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        animator.SetFloat(SpeedHash, speed);

        lastPosition = movingRoot.position;
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        animator.SetTrigger(AttackHash);
    }
}