using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float strafeSpeed = 4f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float forwardInput = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            forwardInput += 1f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            forwardInput -= 1f;
        }

        float turnInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            turnInput -= 1f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            turnInput += 1f;
        }

        float strafeInput = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            strafeInput -= 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            strafeInput += 1f;
        }

        transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime);

        Vector3 moveDirection =
            transform.forward * forwardInput * moveSpeed +
            transform.right * strafeInput * strafeSpeed;

        characterController.Move(moveDirection * Time.deltaTime);

        HandleGravity();
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);
    }
}