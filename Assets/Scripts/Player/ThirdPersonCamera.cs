using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
    [SerializeField]
    private Transform _target;

    private float _currentRotationX;
    private float _currentRotationY;
    private InventoryStateProvider _inventoryStateProvider;
    private ConfigProvider _configProvider;

    private void Awake() {
        _inventoryStateProvider = DIContainer.Instance.Get<IInventoryStateProvider>() as InventoryStateProvider;
        _configProvider = DIContainer.Instance.Get<IConfigProvider>() as ConfigProvider;
    }

    private void Start() {
        if (_target == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                _target = player.transform;
            }
        }

        if (_target != null) {
            Vector3 direction = transform.position - _target.position;
            _currentRotationX = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _currentRotationY = Mathf.Asin(direction.y / direction.magnitude) * Mathf.Rad2Deg;
        } else {
            Vector3 angles = transform.eulerAngles;
            _currentRotationX = angles.y;
            _currentRotationY = angles.x;
        }
    }

    private void LateUpdate() {
        if (_target == null) {
            return;
        }

        HandleRotation();
        UpdateCameraPosition();
    }

    private void HandleRotation() {
        MainGameConfig config = _configProvider.GetConfig();
        if (config.IsInventoryBlockingView) {
            if (_inventoryStateProvider.IsInventoryOpen()) {
                return;
            }
        }

        float rotationSpeed = config.CameraRotationSpeed;
        float minVerticalAngle = config.CameraMinVerticalAngle;
        float maxVerticalAngle = config.CameraMaxVerticalAngle;

        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        _currentRotationX += mouseX;
        _currentRotationY -= mouseY;
        _currentRotationY = Mathf.Clamp(_currentRotationY, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateCameraPosition() {
        MainGameConfig config = _configProvider.GetConfig();
        float distance = config.CameraDistance;
        float height = config.CameraHeight;

        Quaternion rotation = Quaternion.Euler(_currentRotationY, _currentRotationX, 0);
        Vector3 direction = rotation * Vector3.back;
        Vector3 targetPosition = _target.position + Vector3.up * height;
        Vector3 desiredPosition = targetPosition + direction * distance;

        RaycastHit hit;
        if (Physics.Raycast(targetPosition, direction, out hit, distance)) {
            desiredPosition = hit.point - direction * 0.5f;
        }

        transform.position = desiredPosition;
        transform.LookAt(targetPosition);
    }
}