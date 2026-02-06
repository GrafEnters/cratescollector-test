using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    [SerializeField]
    private Transform _cameraTransform;

    [SerializeField]
    private Transform _groundTransform;

    private CharacterController _characterController;
    private InputAction _moveAction;
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private Bounds _platformBounds;
    private IInventoryStateProvider _inventoryStateProvider;
    private IConfigProvider _configProvider;
    private MainGameConfig _cachedConfig;
    private bool _isInventoryOpen;
    private float _cachedMoveSpeed;
    private float _cachedRotationSpeed;

    private void Awake() {
        _characterController = GetComponent<CharacterController>();

        _characterController.center = new Vector3(0, _characterController.height / 2f, 0);

        _cameraTransform = GetCameraTransform();

        _inventoryStateProvider = DIContainer.Instance.Get<IInventoryStateProvider>();
        if (!DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider)) {
            Debug.LogError("IConfigProvider not found in DI container");
        }

        _moveAction = new InputAction(type: InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    private void Start() {
        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;

        CacheConfigValues();
        FindPlatformBounds();
    }

    private void CacheConfigValues() {
        _cachedConfig = _configProvider.GetConfig();
        _cachedMoveSpeed = _cachedConfig.PlayerMoveSpeed;
        _cachedRotationSpeed = _cachedConfig.PlayerRotationSpeed;
    }

    private void FindPlatformBounds() {
        if (_groundTransform == null) {
            GameObject ground = GameObject.FindWithTag("Ground");
            if (ground != null) {
                _groundTransform = ground.transform;
            }
        }

        if (_groundTransform != null) {
            BoxCollider groundCollider = _groundTransform.GetComponent<BoxCollider>();
            if (groundCollider != null) {
                _platformBounds = groundCollider.bounds;
            } else {
                Vector3 groundScale = _groundTransform.localScale;
                Vector3 groundPosition = _groundTransform.position;
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
        if (_cachedConfig.IsInventoryBlockingView && _inventoryStateProvider != null) {
            _isInventoryOpen = _inventoryStateProvider.IsInventoryOpen();
        } else {
            _isInventoryOpen = false;
        }

        HandleMovement();
        HandleRotation();
    }

    private void LateUpdate() {
        Vector3 pos = transform.position;

        if (_characterController.isGrounded && Mathf.Abs(pos.y) > 0.05f) {
            pos.y = 0f;
            transform.position = pos;
        }

        Vector3 clampedPos = ClampPositionToBounds(pos);
        if (clampedPos != pos) {
            transform.position = clampedPos;
        }
    }

    private void HandleMovement() {
        if (_isInventoryOpen) {
            return;
        }

        Transform camera = GetCameraTransform();
        if (camera == null) {
            return;
        }

        Vector3 moveDirection = CalculateMoveDirection(_moveInput, camera);

        if (moveDirection.magnitude > 0.1f) {
            float moveSpeed = _cachedMoveSpeed > 0 ? _cachedMoveSpeed : 5f;
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
        Vector3 clampedNextPosition = ClampPositionToBounds(nextPosition);

        if (clampedNextPosition.x != nextPosition.x) {
            _velocity.x = 0f;
        }
        if (clampedNextPosition.z != nextPosition.z) {
            _velocity.z = 0f;
        }

        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void HandleRotation() {
        if (_isInventoryOpen) {
            return;
        }

        if (_moveInput.magnitude > 0.1f) {
            Transform camera = GetCameraTransform();
            if (camera == null) {
                return;
            }

            Vector3 moveDirection = CalculateMoveDirection(_moveInput, camera);

            if (moveDirection.magnitude > 0.1f) {
                float rotationSpeed = _cachedRotationSpeed > 0 ? _cachedRotationSpeed : 10f;
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private Transform GetCameraTransform() {
        if (_cameraTransform != null) {
            return _cameraTransform;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null) {
            _cameraTransform = mainCamera.transform;
            return _cameraTransform;
        }

        return null;
    }

    private Vector3 CalculateMoveDirection(Vector2 input, Transform camera) {
        if (input.magnitude <= 0.1f || camera == null) {
            return Vector3.zero;
        }

        Vector3 forward = camera.forward;
        Vector3 right = camera.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return (forward * input.y + right * input.x).normalized;
    }

    private Vector3 ClampPositionToBounds(Vector3 position) {
        float playerRadius = _characterController.radius;
        Vector3 clampedPos = position;

        if (clampedPos.x - playerRadius < _platformBounds.min.x) {
            clampedPos.x = _platformBounds.min.x + playerRadius;
        } else if (clampedPos.x + playerRadius > _platformBounds.max.x) {
            clampedPos.x = _platformBounds.max.x - playerRadius;
        }

        if (clampedPos.z - playerRadius < _platformBounds.min.z) {
            clampedPos.z = _platformBounds.min.z + playerRadius;
        } else if (clampedPos.z + playerRadius > _platformBounds.max.z) {
            clampedPos.z = _platformBounds.max.z - playerRadius;
        }

        return clampedPos;
    }
}