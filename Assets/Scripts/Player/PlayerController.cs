using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    [SerializeField]
    private Transform _cameraTransform;

    private CharacterController _characterController;
    private InputAction _moveAction;
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private Bounds _platformBounds;

    private void Awake() {
        _characterController = GetComponent<CharacterController>();

        _characterController.center = new Vector3(0, _characterController.height / 2f, 0);

        if (_cameraTransform == null) {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) {
                _cameraTransform = mainCamera.transform;
            }
        }

        _moveAction = new InputAction(type: InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a")
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
                _platformBounds = groundCollider.bounds;
            } else {
                Transform groundTransform = ground.transform;
                Vector3 groundScale = groundTransform.localScale;
                Vector3 groundPosition = groundTransform.position;
                _platformBounds = new Bounds(groundPosition, groundScale);
            }
        } else {
            _platformBounds = new Bounds(Vector3.zero, new Vector3(20f, 1f, 20f));
        }
    }

    private void OnEnable() {
        _moveAction?.Enable();
        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMoveCanceled;
    }

    private void OnDisable() {
        _moveAction.performed -= OnMove;
        _moveAction.canceled -= OnMoveCanceled;
        _moveAction?.Disable();
    }

    private void OnMove(InputAction.CallbackContext context) {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context) {
        _moveInput = Vector2.zero;
    }

    private void Update() {
        HandleMovement();
        HandleRotation();
    }

    private void LateUpdate() {
        Vector3 pos = transform.position;

        if (_characterController.isGrounded && Mathf.Abs(pos.y) > 0.05f) {
            pos.y = 0f;
            transform.position = pos;
        }

        float playerRadius = _characterController.radius;
        Vector3 originalPos = pos;

        if (pos.x - playerRadius < _platformBounds.min.x) {
            pos.x = _platformBounds.min.x + playerRadius;
        } else if (pos.x + playerRadius > _platformBounds.max.x) {
            pos.x = _platformBounds.max.x - playerRadius;
        }

        if (pos.z - playerRadius < _platformBounds.min.z) {
            pos.z = _platformBounds.min.z + playerRadius;
        } else if (pos.z + playerRadius > _platformBounds.max.z) {
            pos.z = _platformBounds.max.z - playerRadius;
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

        if (_cameraTransform == null) {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) {
                _cameraTransform = mainCamera.transform;
            } else {
                return;
            }
        }

        Vector3 moveDirection = Vector3.zero;

        if (_moveInput.magnitude > 0.1f) {
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;
        }

        if (moveDirection.magnitude > 0.1f) {
            float moveSpeed = config != null ? config.PlayerMoveSpeed : 5f;
            Vector3 move = moveDirection * moveSpeed;
            _velocity.x = move.x;
            _velocity.z = move.z;
        } else {
            _velocity.x = 0f;
            _velocity.z = 0f;
        }

        Vector3 currentPosition = transform.position;
        float playerRadius = _characterController.radius;
        Vector3 nextPosition = currentPosition + _velocity * Time.deltaTime;

        if (nextPosition.x - playerRadius < _platformBounds.min.x) {
            _velocity.x = 0f;
        } else if (nextPosition.x + playerRadius > _platformBounds.max.x) {
            _velocity.x = 0f;
        }

        if (nextPosition.z - playerRadius < _platformBounds.min.z) {
            _velocity.z = 0f;
        } else if (nextPosition.z + playerRadius > _platformBounds.max.z) {
            _velocity.z = 0f;
        }

        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void HandleRotation() {
        if (_moveInput.magnitude > 0.1f && _cameraTransform != null) {
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * _moveInput.y + right * _moveInput.x).normalized;

            if (moveDirection.magnitude > 0.1f) {
                MainGameConfig config = ConfigManager.Config;
                float rotationSpeed = config != null ? config.PlayerRotationSpeed : 10f;
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}