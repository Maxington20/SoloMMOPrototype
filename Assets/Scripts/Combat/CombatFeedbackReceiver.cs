using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class CombatFeedbackReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Renderer[] renderersToFlash;
    [SerializeField] private Transform scaleRoot;

    [Header("Default Hit Feedback")]
    [SerializeField] private Color defaultHitFlashColor = Color.white;
    [SerializeField] private float defaultFlashDuration = 0.08f;
    [SerializeField] private float defaultScalePunchAmount = 0.08f;
    [SerializeField] private float defaultScalePunchDuration = 0.10f;

    [Header("Miss / Dodge Feedback")]
    [SerializeField] private Color missFlashColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    [SerializeField] private float missFlashDuration = 0.06f;

    [Header("Death Feedback")]
    [SerializeField] private bool playDeathScale = true;
    [SerializeField] private float deathScaleAmount = 0.85f;
    [SerializeField] private float deathScaleDuration = 0.12f;

    private readonly int baseColorPropertyId = Shader.PropertyToID("_BaseColor");
    private readonly int colorPropertyId = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashRoutine;
    private Coroutine scaleRoutine;

    private Vector3 originalScale;
    private bool hasQueuedAbilityFeedback;
    private Color queuedFlashColor;
    private float queuedScalePunchAmount;

    private void Awake()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (scaleRoot == null)
        {
            scaleRoot = transform;
        }

        if (renderersToFlash == null || renderersToFlash.Length == 0)
        {
            renderersToFlash = GetComponentsInChildren<Renderer>();
        }

        propertyBlock = new MaterialPropertyBlock();
        originalScale = scaleRoot.localScale;
    }

    private void OnEnable()
    {
        if (health == null)
        {
            return;
        }

        health.OnDamaged += HandleDamaged;
        health.OnMissed += HandleMissed;
        health.OnDodged += HandleDodged;
        health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (health == null)
        {
            return;
        }

        health.OnDamaged -= HandleDamaged;
        health.OnMissed -= HandleMissed;
        health.OnDodged -= HandleDodged;
        health.OnDied -= HandleDied;
    }

    public void QueueAbilityImpactFeedback(AbilityData ability)
    {
        if (ability == null || !ability.UseAbilityImpactFeedback)
        {
            ClearQueuedAbilityFeedback();
            return;
        }

        hasQueuedAbilityFeedback = true;
        queuedFlashColor = ability.ImpactFlashColor;
        queuedScalePunchAmount = ability.ImpactScalePunchAmount;
    }

    private void HandleDamaged(int amount, GameObject source)
    {
        Color flashColor = hasQueuedAbilityFeedback ? queuedFlashColor : defaultHitFlashColor;
        float scalePunchAmount = hasQueuedAbilityFeedback ? queuedScalePunchAmount : defaultScalePunchAmount;

        ClearQueuedAbilityFeedback();

        PlayFlash(flashColor, defaultFlashDuration);
        PlayScalePunch(scalePunchAmount, defaultScalePunchDuration);
    }

    private void HandleMissed(GameObject source)
    {
        ClearQueuedAbilityFeedback();
        PlayFlash(missFlashColor, missFlashDuration);
    }

    private void HandleDodged(GameObject source)
    {
        ClearQueuedAbilityFeedback();
        PlayFlash(missFlashColor, missFlashDuration);
    }

    private void HandleDied()
    {
        ClearQueuedAbilityFeedback();

        if (playDeathScale)
        {
            PlayScalePunch(deathScaleAmount, deathScaleDuration);
        }
    }

    private void PlayFlash(Color flashColor, float duration)
    {
        if (renderersToFlash == null || renderersToFlash.Length == 0)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine(flashColor, Mathf.Max(0.01f, duration)));
    }

    private void PlayScalePunch(float amount, float duration)
    {
        if (scaleRoot == null)
        {
            return;
        }

        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
        }

        scaleRoutine = StartCoroutine(ScalePunchRoutine(amount, Mathf.Max(0.01f, duration)));
    }

    private IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        SetRendererColor(flashColor);

        yield return new WaitForSeconds(duration);

        ClearRendererColor();
        flashRoutine = null;
    }

    private IEnumerator ScalePunchRoutine(float amount, float duration)
    {
        if (scaleRoot == null)
        {
            yield break;
        }

        Vector3 punchScale = originalScale * (1f + amount);
        float halfDuration = duration * 0.5f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);
            scaleRoot.localScale = Vector3.Lerp(originalScale, punchScale, t);
            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / halfDuration);
            scaleRoot.localScale = Vector3.Lerp(punchScale, originalScale, t);
            yield return null;
        }

        scaleRoot.localScale = originalScale;
        scaleRoutine = null;
    }

    private void SetRendererColor(Color color)
    {
        for (int i = 0; i < renderersToFlash.Length; i++)
        {
            Renderer targetRenderer = renderersToFlash[i];

            if (targetRenderer == null || targetRenderer.sharedMaterial == null)
            {
                continue;
            }

            targetRenderer.GetPropertyBlock(propertyBlock);

            if (targetRenderer.sharedMaterial.HasProperty(baseColorPropertyId))
            {
                propertyBlock.SetColor(baseColorPropertyId, color);
            }
            else if (targetRenderer.sharedMaterial.HasProperty(colorPropertyId))
            {
                propertyBlock.SetColor(colorPropertyId, color);
            }

            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void ClearRendererColor()
    {
        for (int i = 0; i < renderersToFlash.Length; i++)
        {
            Renderer targetRenderer = renderersToFlash[i];

            if (targetRenderer == null)
            {
                continue;
            }

            targetRenderer.SetPropertyBlock(null);
        }
    }

    private void ClearQueuedAbilityFeedback()
    {
        hasQueuedAbilityFeedback = false;
        queuedFlashColor = defaultHitFlashColor;
        queuedScalePunchAmount = defaultScalePunchAmount;
    }
}