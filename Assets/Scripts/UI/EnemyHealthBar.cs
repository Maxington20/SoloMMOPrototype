using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Health targetHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateBar;
            targetHealth.OnDied += HideBar;
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= UpdateBar;
            targetHealth.OnDied -= HideBar;
        }
    }

    private void Start()
    {
        UpdateBar();
    }

    private void LateUpdate()
    {
        if (targetHealth == null || mainCamera == null)
        {
            return;
        }

        transform.position = targetHealth.transform.position + worldOffset;

        transform.rotation = Quaternion.LookRotation(
            transform.position - mainCamera.transform.position);
    }

    private void UpdateBar()
    {
        if (targetHealth == null || fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = targetHealth.HealthPercent;
    }

    private void HideBar()
    {
        gameObject.SetActive(false);
    }
}