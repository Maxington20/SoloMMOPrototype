using TMPro;
using UnityEngine;

public class AdventurerNameplate : MonoBehaviour
{
    [SerializeField] private SimulatedAdventurer target;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI classText;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.4f, 0f);

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        RefreshText();
    }

    private void LateUpdate()
    {
        if (target == null || mainCamera == null)
        {
            return;
        }

        transform.position = target.transform.position + worldOffset;
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    public void RefreshText()
    {
        if (target == null)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = target.AdventurerName;
        }

        if (classText != null)
        {
            classText.text = target.ClassLabel;
        }
    }
}