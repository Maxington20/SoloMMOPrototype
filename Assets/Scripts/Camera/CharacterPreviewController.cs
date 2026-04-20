using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterPreviewController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform previewTarget;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 1.5f;
    [SerializeField] private float maxZoom = 5f;

    private bool dragging;

    private void Update()
    {
        if (dragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            previewTarget.Rotate(Vector3.up, -mouseX * rotationSpeed * Time.deltaTime);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 pos = cameraPivot.localPosition;
            pos.z += scroll * zoomSpeed;
            pos.z = Mathf.Clamp(pos.z, -maxZoom, -minZoom);
            cameraPivot.localPosition = pos;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }
}