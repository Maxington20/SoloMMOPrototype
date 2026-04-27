using TMPro;
using UnityEngine;

public enum FloatingCombatTextType
{
    Damage,
    Healing,
    Miss,
    Dodge,
    Crit
}

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("Motion")]
    [SerializeField] private float floatSpeed = 1.8f;
    [SerializeField] private float lifetime = 1.1f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private float randomSpread = 0.5f;

    [Header("Colours")]
    [SerializeField] private Color enemyDamageColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color playerDamageColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color healingColor = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private Color missColor = Color.gray;
    [SerializeField] private Color dodgeColor = new Color(0.3f, 0.8f, 1f);
    [SerializeField] private Color critColor = new Color(1f, 0.5f, 0f);

    private Camera mainCamera;
    private float timer;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void InitializeDamage(int amount, Vector3 position, bool isPlayerTarget)
    {
        bool isCrit = Random.value < 0.15f; // 15% fake crit for now

        if (isCrit)
        {
            Initialize($"CRIT {amount}", position, critColor, 1.4f);
        }
        else
        {
            Initialize(
                amount.ToString(),
                position,
                isPlayerTarget ? playerDamageColor : enemyDamageColor,
                1f);
        }
    }

    public void InitializeHealing(int amount, Vector3 position)
    {
        Initialize($"+{amount}", position, healingColor, 1.1f);
    }

    public void InitializeMiss(Vector3 position)
    {
        Initialize("MISS", position, missColor, 1f);
    }

    public void InitializeDodge(Vector3 position)
    {
        Initialize("DODGE", position, dodgeColor, 1f);
    }

    private void Initialize(string text, Vector3 position, Color color, float scaleMultiplier)
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-randomSpread, randomSpread),
            Random.Range(0f, randomSpread),
            0f);

        transform.position = position + worldOffset + randomOffset;
        timer = lifetime;

        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
            textComponent.fontSize *= scaleMultiplier;
        }
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer -= Time.deltaTime;

        if (textComponent != null)
        {
            Color color = textComponent.color;
            color.a = Mathf.Clamp01(timer / lifetime);
            textComponent.color = color;
        }

        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}