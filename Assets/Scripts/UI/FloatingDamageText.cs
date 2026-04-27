using TMPro;
using UnityEngine;

public enum FloatingCombatTextType
{
    Damage,
    Healing,
    Miss,
    Dodge
}

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    [Header("Colours")]
    [SerializeField] private Color enemyDamageColor = Color.yellow;
    [SerializeField] private Color playerDamageColor = Color.red;
    [SerializeField] private Color healingColor = Color.green;
    [SerializeField] private Color missColor = Color.gray;
    [SerializeField] private Color dodgeColor = Color.cyan;

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

    public void InitializeDamage(int damageAmount, Vector3 worldPosition, bool isPlayerTarget)
    {
        Initialize(
            damageAmount.ToString(),
            worldPosition,
            isPlayerTarget ? playerDamageColor : enemyDamageColor);
    }

    public void InitializeHealing(int healingAmount, Vector3 worldPosition)
    {
        Initialize(
            $"+{healingAmount}",
            worldPosition,
            healingColor);
    }

    public void InitializeMiss(Vector3 worldPosition)
    {
        Initialize(
            "Miss",
            worldPosition,
            missColor);
    }

    public void InitializeDodge(Vector3 worldPosition)
    {
        Initialize(
            "Dodge",
            worldPosition,
            dodgeColor);
    }

    private void Initialize(string text, Vector3 worldPosition, Color color)
    {
        transform.position = worldPosition + worldOffset;
        timer = lifetime;

        if (textComponent != null)
        {
            textComponent.text = text;
            textComponent.color = color;
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