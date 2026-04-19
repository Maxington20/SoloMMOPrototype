using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

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

    public void Initialize(int damageAmount, Vector3 worldPosition, bool isPlayerTarget)
    {
        transform.position = worldPosition + worldOffset;
        timer = lifetime;

        if (textComponent != null)
        {
            textComponent.text = damageAmount.ToString();
            textComponent.color = isPlayerTarget ? Color.red : Color.yellow;
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