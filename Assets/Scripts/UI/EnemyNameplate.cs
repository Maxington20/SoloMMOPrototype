using TMPro;
using UnityEngine;

public class EnemyNameplate : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private GameObject visualRoot;

    private Camera mainCamera;
    private Health targetHealth;
    private DisplayName targetDisplayName;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (visualRoot == null)
        {
            visualRoot = gameObject;
        }

        if (target != null)
        {
            targetHealth = target.GetComponent<Health>();
            targetDisplayName = target.GetComponent<DisplayName>();
        }
    }

    private void Start()
    {
        RefreshName();
    }

    private void Update()
    {
        if (targetHealth == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(!targetHealth.IsDead);
        RefreshName();
    }

    private void LateUpdate()
    {
        if (target == null || mainCamera == null || targetHealth == null || targetHealth.IsDead)
        {
            return;
        }

        transform.position = target.position + worldOffset;
        transform.forward = mainCamera.transform.forward;
    }

    private void RefreshName()
    {
        if (nameText == null)
        {
            return;
        }

        if (targetDisplayName != null)
        {
            nameText.text = targetDisplayName.Display;
            nameText.color = targetDisplayName.DisplayColor;
            return;
        }

        if (target != null)
        {
            nameText.text = target.name;
        }
    }

    private void SetVisible(bool visible)
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(visible);
        }
    }
}