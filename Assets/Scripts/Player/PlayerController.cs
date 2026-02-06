using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    [SerializeField]
    private Transform cameraTransform;

    private CharacterController characterController;
    private InputAction moveAction;
    private Vector2 moveInput;
    private Vector3 velocity;
    private Bounds platformBounds;

    private void Awake() {
        characterController = GetComponent<CharacterController>();

        characterController.center = new Vector3(0, characterController.height / 2f, 0);

        if (cameraTransform == null) {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) {
                cameraTransform = mainCamera.transform;
            }
        }

        moveAction = new InputAction(type: InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    private void Start() {
        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;

        FindPlatformBounds();
    }

    private void FindPlatformBounds() {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null) {
            BoxCollider groundCollider = ground.GetComponent<BoxCollider>();
            if (groundCollider != null) {
                platformBounds = groundCollider.bounds;
            } else {
                Transform groundTransform = ground.transform;
                Vector3 groundScale = groundTransform.localScale;
                Vector3 groundPosition = groundTransform.position;
                platformBounds = new Bounds(groundPosition, groundScale);
            }
        } else {
            platformBounds = new Bounds(Vector3.zero, new Vector3(20f, 1f, 20f));
        }
    }

    private void OnEnable() {
        moveAction?.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMoveCanceled;
    }

    private void OnDisable() {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMoveCanceled;
        moveAction?.Disable();
    }

    private void OnMove(InputAction.CallbackContext context) {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context) {
        moveInput = Vector2.zero;
    }

    private void Update() {
        HandleMovement();
        HandleRotation();
    }

    private void LateUpdate() {
        Vector3 pos = transform.position;

        if (characterController.isGrounded && Mathf.Abs(pos.y) > 0.05f) {
            pos.y = 0f;
            transform.position = pos;
        }

        float playerRadius = characterController.radius;
        Vector3 originalPos = pos;

        if (pos.x - playerRadius < platformBounds.min.x) {
            pos.x = platformBounds.min.x + playerRadius;
        } else if (pos.x + playerRadius > platformBounds.max.x) {
            pos.x = platformBounds.max.x - playerRadius;
        }

        if (pos.z - playerRadius < platformBounds.min.z) {
            pos.z = platformBounds.min.z + playerRadius;
        } else if (pos.z + playerRadius > platformBounds.max.z) {
            pos.z = platformBounds.max.z - playerRadius;
        }

        if (pos != originalPos) {
            transform.position = pos;
        }
    }

    private void HandleMovement() {
        MainGameConfig config = ConfigManager.Config;
        if (config != null && config.IsInventoryBlockingView) {
            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null && inventoryUI.IsOpen()) {
                return;
            }
        }

        if (cameraTransform == null) {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) {
                cameraTransform = mainCamera.transform;
            } else {
                return;
            }
        }

        Vector3 moveDirection = Vector3.zero;

        if (moveInput.magnitude > 0.1f) {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        }

        if (moveDirection.magnitude > 0.1f) {
            float moveSpeed = config != null ? config.playerMoveSpeed : 5f;
            Vector3 move = moveDirection * moveSpeed;
            velocity.x = move.x;
            velocity.z = move.z;
        } else {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        Vector3 currentPosition = transform.position;
        float playerRadius = characterController.radius;
        Vector3 nextPosition = currentPosition + velocity * Time.deltaTime;

        if (nextPosition.x - playerRadius < platformBounds.min.x) {
            velocity.x = 0f;
        } else if (nextPosition.x + playerRadius > platformBounds.max.x) {
            velocity.x = 0f;
        }

        if (nextPosition.z - playerRadius < platformBounds.min.z) {
            velocity.z = 0f;
        } else if (nextPosition.z + playerRadius > platformBounds.max.z) {
            velocity.z = 0f;
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleRotation() {
        if (moveInput.magnitude > 0.1f && cameraTransform != null) {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

            if (moveDirection.magnitude > 0.1f) {
                MainGameConfig config = ConfigManager.Config;
                float rotationSpeed = config != null ? config.playerRotationSpeed : 10f;
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}