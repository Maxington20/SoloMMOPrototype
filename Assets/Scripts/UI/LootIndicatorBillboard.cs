using UnityEngine;

public class LootIndicatorBillboard : MonoBehaviour
{
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.15f;

    private Camera mainCamera;
    private Vector3 baseLocalPosition;
    private bool basePositionCaptured;

    private void OnEnable()
    {
        CaptureBasePosition();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        CaptureBasePosition();
    }

    private void LateUpdate()
    {
        if (!basePositionCaptured)
        {
            CaptureBasePosition();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(
                mainCamera.transform.forward,
                Vector3.up);
        }

        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = baseLocalPosition + new Vector3(0f, bobOffset, 0f);
    }

    private void CaptureBasePosition()
    {
        baseLocalPosition = transform.localPosition;
        basePositionCaptured = true;
    }
}