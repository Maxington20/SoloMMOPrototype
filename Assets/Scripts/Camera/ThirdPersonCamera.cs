using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float distance = 6f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float zoomSpeed = 2f;

    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = 15f;
    [SerializeField] private float maxPitch = 70f;

    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.8f, 0f);

    [SerializeField] private float followSmoothness = 12f;
    [SerializeField] private float returnToBehindSpeed = 8f;

    [SerializeField] private float defaultPitch = 25f;

    private float yaw;
    private float pitch;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("ThirdPersonCamera has no target assigned.");
            return;
        }

        yaw = target.eulerAngles.y;
        pitch = defaultPitch;

        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        HandleRotation();
        HandleZoom();
        UpdatePosition();
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * mouseSensitivity;
            pitch -= mouseY * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            float targetYaw = target.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, targetYaw, returnToBehindSpeed * Time.deltaTime);
            pitch = Mathf.Lerp(pitch, defaultPitch, returnToBehindSpeed * Time.deltaTime);
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void UpdatePosition()
    {
        Vector3 lookTarget = target.position + targetOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = lookTarget - (rotation * Vector3.forward * distance);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSmoothness * Time.deltaTime);

        transform.LookAt(lookTarget);
    }

    private void SnapToTarget()
    {
        Vector3 lookTarget = target.position + targetOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = lookTarget - (rotation * Vector3.forward * distance);

        transform.position = desiredPosition;
        transform.LookAt(lookTarget);
    }
}