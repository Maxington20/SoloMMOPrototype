using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class AbilityCastController : MonoBehaviour
{
    [Header("Casting")]
    [SerializeField] private float movementCancelDistance = 0.08f;

    private Health playerHealth;

    private bool isCasting;
    private AbilityData currentCastingAbility;
    private float castStartTime;
    private float castDuration;
    private Vector3 castStartPosition;

    public bool IsCasting => isCasting;
    public AbilityData CurrentCastingAbility => currentCastingAbility;

    public float CastProgress => !isCasting || castDuration <= 0f
        ? 0f
        : Mathf.Clamp01((Time.time - castStartTime) / castDuration);

    public event Action<AbilityData, float> OnCastStarted;
    public event Action<AbilityData> OnCastReadyToComplete;
    public event Action<AbilityData> OnCastCancelled;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged += HandlePlayerDamaged;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamaged -= HandlePlayerDamaged;
        }
    }

    private void Update()
    {
        UpdateCasting();
    }

    public bool BeginCast(AbilityData ability)
    {
        if (ability == null || isCasting)
        {
            return false;
        }

        currentCastingAbility = ability;
        isCasting = true;
        castStartTime = Time.time;
        castStartPosition = transform.position;

        castDuration = ability.CastType switch
        {
            AbilityCastType.CastTime => ability.CastTimeSeconds,
            AbilityCastType.Channel => ability.ChannelDurationSeconds,
            _ => 0f
        };

        if (castDuration <= 0f)
        {
            AbilityData instantAbility = currentCastingAbility;
            ClearCastState();
            OnCastReadyToComplete?.Invoke(instantAbility);
            return true;
        }

        OnCastStarted?.Invoke(ability, castDuration);
        return true;
    }

    public void CancelCurrentCast(string message)
    {
        if (!isCasting || currentCastingAbility == null)
        {
            return;
        }

        AbilityData cancelledAbility = currentCastingAbility;

        PostSystem(message);

        ClearCastState();

        OnCastCancelled?.Invoke(cancelledAbility);
    }

    public void ClearCastState()
    {
        isCasting = false;
        currentCastingAbility = null;
        castStartTime = 0f;
        castDuration = 0f;
        castStartPosition = Vector3.zero;
    }

    private void UpdateCasting()
    {
        if (!isCasting || currentCastingAbility == null)
        {
            return;
        }

        if (!currentCastingAbility.CanMoveWhileCasting)
        {
            float distanceMoved = Vector3.Distance(castStartPosition, transform.position);

            if (distanceMoved > movementCancelDistance)
            {
                CancelCurrentCast("Casting cancelled by movement.");
                return;
            }
        }

        if (Time.time - castStartTime >= castDuration)
        {
            AbilityData completedAbility = currentCastingAbility;

            ClearCastState();

            OnCastReadyToComplete?.Invoke(completedAbility);
        }
    }

    private void HandlePlayerDamaged(int amount, GameObject source)
    {
        if (!isCasting || currentCastingAbility == null)
        {
            return;
        }

        if (!currentCastingAbility.CanBeInterrupted)
        {
            return;
        }

        CancelCurrentCast("Casting interrupted.");
    }

    private void PostSystem(string message)
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.PostSystem(message);
        }
    }
}