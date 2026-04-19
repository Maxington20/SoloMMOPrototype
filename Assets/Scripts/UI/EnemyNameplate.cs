using TMPro;
using UnityEngine;

public class EnemyNameplate : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private string displayName = "Wolf";
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
    [SerializeField] private GameObject visualRoot;

    private Camera mainCamera;
    private Health targetHealth;

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
        }
    }

    private void Start()
    {
        if (nameText != null)
        {
            nameText.text = displayName;
        }
    }

    private void Update()
    {
        if (targetHealth == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(!targetHealth.IsDead);
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

    private void SetVisible(bool visible)
    {
        if (visualRoot != null)
        {
            visualRoot.SetActive(visible);
        }
    }
}