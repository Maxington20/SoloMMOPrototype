using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerDeathController : MonoBehaviour
{
    public static PlayerDeathController Instance { get; private set; }

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool useStartingPositionIfNoRespawnPoint = true;

    [Header("Disable While Dead")]
    [SerializeField] private Behaviour[] behavioursToDisableWhileDead;

    private Health health;
    private CharacterController characterController;
    private PlayerAnimationController playerAnimationController;

    private Vector3 startingPosition;
    private Quaternion startingRotation;

    private bool isDead;

    public bool IsDead => isDead;

    private void Awake()
    {
        Instance = this;

        health = GetComponent<Health>();
        characterController = GetComponent<CharacterController>();
        playerAnimationController = GetComponent<PlayerAnimationController>();

        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDied += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandlePlayerDeath;
        }
    }

    private void HandlePlayerDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        if (playerAnimationController != null)
        {
            playerAnimationController.PlayDeath();
        }

        SetDeadStateEnabled(true);

        if (PlayerDeathWindowUI.Instance != null)
        {
            PlayerDeathWindowUI.Instance.Open();
        }
    }

    public void Respawn()
    {
        Vector3 targetPosition = startingPosition;
        Quaternion targetRotation = startingRotation;

        if (respawnPoint != null)
        {
            targetPosition = respawnPoint.position;
            targetRotation = respawnPoint.rotation;
        }
        else if (!useStartingPositionIfNoRespawnPoint)
        {
            targetPosition = transform.position;
            targetRotation = transform.rotation;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (health != null)
        {
            health.ResetHealth();
        }

        if (playerAnimationController != null)
        {
            playerAnimationController.ResetToIdle();
        }

        isDead = false;

        SetDeadStateEnabled(false);

        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem("You have respawned.");
        }
    }

    private void SetDeadStateEnabled(bool deadState)
    {
        if (behavioursToDisableWhileDead == null)
        {
            return;
        }

        for (int i = 0; i < behavioursToDisableWhileDead.Length; i++)
        {
            Behaviour behaviour = behavioursToDisableWhileDead[i];

            if (behaviour == null)
            {
                continue;
            }

            behaviour.enabled = !deadState;
        }
    }
}