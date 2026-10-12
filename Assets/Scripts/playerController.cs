
using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -20f;
    public float jumpHeight = 2f;
    public Transform spawnPoint;
    public float rotationSpeed = 10f;

    public Transform cameraTransform;

    private float respawnHeightoffset = 1.5f;
    private CharacterController controller;
    private Vector2 moveInput;
    private float verticalVelocity;
    private bool jumpPressed;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Buscar la camara si no se asigno manualmente
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (jumpPressed && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpPressed = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.SoundType.Jump);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        // Direcciones horizontales de la camara
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Evitar que la inclinacion de la camara afecte el movimiento
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        // Movimiento relativo a la camara
        Vector3 moveDirection =
            cameraRight * moveInput.x +
            cameraForward * moveInput.y;

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Separar el movimiento horizontal de la gravedad
        Vector3 move = moveDirection.normalized * speed;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
        }
    }

    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    public void Die()
    {
        if (spawnPoint == null) return;

        controller.enabled = false;

        Vector3 respawnPosition =
            spawnPoint.position + Vector3.up * respawnHeightoffset;

        transform.position = respawnPosition;
        verticalVelocity = 0f;

        controller.enabled = true;
    }
}
