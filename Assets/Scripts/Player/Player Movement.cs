using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Transform playerCamera;

    [Header("Water")]
    public float waterSurfaceY = 0f;

    [Header("Swimming")]
    public float swimSpeed = 4f;
    public float sprintSpeed = 7f;
    public float verticalSpeed = 3f;
    public float acceleration = 10f;
    public float swimAboveWaterHeight = 1f;

    // Extra buffer prevents switching to walking at the peak.
    public float swimExitBuffer = 0.35f;

    [Header("Surface Gravity")]
    public float surfaceSlowdownDistance = 0.5f;
    public float surfaceGravity = 5f;

    [Header("Walking")]
    public float walkSpeed = 4f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private Vector3 currentVelocity;
    private float verticalVelocity;
    private bool swimming;

    public bool IsSwimming => swimming;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        if (playerCamera != null)
        {
            swimming =
                playerCamera.position.y < waterSurfaceY;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null ||
            playerCamera == null)
        {
            return;
        }

        UpdateSwimmingState();

        if (swimming)
            Swim();
        else
            Walk();
    }

    private void UpdateSwimmingState()
    {
        float cameraY = playerCamera.position.y;

        if (!swimming &&
            cameraY < waterSurfaceY)
        {
            swimming = true;
            verticalVelocity = 0f;
        }

        float swimmingExitHeight =
            waterSurfaceY +
            swimAboveWaterHeight +
            swimExitBuffer;

        if (swimming &&
            cameraY > swimmingExitHeight)
        {
            swimming = false;

            // Preserve motion instead of abruptly stopping it.
            verticalVelocity = currentVelocity.y;
            currentVelocity = Vector3.zero;
        }
    }

    private void Swim()
    {
        float horizontal =
            Keyboard.current.aKey.isPressed ? -1f :
            Keyboard.current.dKey.isPressed ? 1f : 0f;

        float forward =
            Keyboard.current.wKey.isPressed ? 1f :
            Keyboard.current.sKey.isPressed ? -1f : 0f;

        Vector3 direction =
            transform.right * horizontal +
            transform.forward * forward;

        direction = Vector3.ClampMagnitude(
            direction,
            1f
        );

        float speed =
            Keyboard.current.leftShiftKey.isPressed
            ? sprintSpeed
            : swimSpeed;

        Vector3 desiredVelocity =
            direction * speed;

        float maximumCameraY =
            waterSurfaceY + swimAboveWaterHeight;

        float distanceToMaximum =
            maximumCameraY - playerCamera.position.y;

        bool movingUp =
            Keyboard.current.spaceKey.isPressed;

        bool movingDown =
            (Keyboard.current.leftAltKey.isPressed || Keyboard.current.leftCtrlKey.isPressed);

        if (movingUp)
        {
            desiredVelocity.y = verticalSpeed;
        }
        else if (movingDown)
        {
            desiredVelocity.y = -verticalSpeed;
        }
        else
        {
            desiredVelocity.y = 0f;
        }

        if (movingUp &&
            distanceToMaximum < surfaceSlowdownDistance)
        {
            float closeness =
                1f - Mathf.Clamp01(
                    distanceToMaximum /
                    surfaceSlowdownDistance
                );

            desiredVelocity.y -=
                closeness * surfaceGravity;
        }

        if (distanceToMaximum < 0f)
        {
            desiredVelocity.y = -surfaceGravity;
        }

        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            desiredVelocity,
            acceleration * Time.deltaTime
        );

        controller.Move(
            currentVelocity * Time.deltaTime
        );
    }

    private void Walk()
    {
        float horizontal =
            Keyboard.current.aKey.isPressed ? -1f :
            Keyboard.current.dKey.isPressed ? 1f : 0f;

        float forward =
            Keyboard.current.wKey.isPressed ? 1f :
            Keyboard.current.sKey.isPressed ? -1f : 0f;

        Vector3 direction =
            transform.right * horizontal +
            transform.forward * forward;

        direction = Vector3.ClampMagnitude(
            direction,
            1f
        );

        Vector3 movement =
            direction * walkSpeed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
            }
        }

        verticalVelocity +=
            gravity * Time.deltaTime;

        movement.y = verticalVelocity;

        controller.Move(
            movement * Time.deltaTime
        );
    }
}