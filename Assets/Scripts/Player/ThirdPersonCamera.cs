using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {
    [SerializeField]
    private Transform _target;

    private float _currentRotationX;
    private float _currentRotationY;
    private IInventoryStateProvider _inventoryStateProvider;
    private IConfigProvider _configProvider;
    private MainGameConfig _cachedConfig;
    private bool _isInventoryOpen;
    private float _cachedRotationSpeed;
    private float _cachedMinVerticalAngle;
    private float _cachedMaxVerticalAngle;
    private float _cachedDistance;
    private float _cachedHeight;

    private void Awake() {
        _inventoryStateProvider = DIContainer.Instance.Get<IInventoryStateProvider>();
        if (!DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider)) {
            Debug.LogError("IConfigProvider not found in DI container");
        }
    }

    private void Start() {
        if (_target == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                _target = player.transform;
            }
        }

        CacheConfigValues();

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

    private void CacheConfigValues() {
        if (_configProvider == null) {
            Debug.LogError("ConfigProvider is null");
            return;
        }

        _cachedConfig = _configProvider.GetConfig();
        if (_cachedConfig == null) {
            Debug.LogError("MainGameConfig is null");
            return;
        }

        _cachedRotationSpeed = _cachedConfig.CameraRotationSpeed;
        _cachedMinVerticalAngle = _cachedConfig.CameraMinVerticalAngle;
        _cachedMaxVerticalAngle = _cachedConfig.CameraMaxVerticalAngle;
        _cachedDistance = _cachedConfig.CameraDistance;
        _cachedHeight = _cachedConfig.CameraHeight;
    }

    private void Update() {
        if (_cachedConfig.IsInventoryBlockingView && _inventoryStateProvider != null) {
            _isInventoryOpen = _inventoryStateProvider.IsInventoryOpen();
        } else {
            _isInventoryOpen = false;
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
        if (_isInventoryOpen) {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * _cachedRotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * _cachedRotationSpeed;

        _currentRotationX += mouseX;
        _currentRotationY -= mouseY;
        _currentRotationY = Mathf.Clamp(_currentRotationY, _cachedMinVerticalAngle, _cachedMaxVerticalAngle);
    }

    private void UpdateCameraPosition() {
        float distance = _cachedDistance;
        float height = _cachedHeight;

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