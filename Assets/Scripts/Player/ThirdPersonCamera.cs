using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    private float currentRotationX;
    private float currentRotationY;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            Vector3 direction = transform.position - target.position;
            currentRotationX = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            currentRotationY = Mathf.Asin(direction.y / direction.magnitude) * Mathf.Rad2Deg;
        }
        else
        {
            Vector3 angles = transform.eulerAngles;
            currentRotationX = angles.y;
            currentRotationY = angles.x;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleRotation();
        UpdateCameraPosition();
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        currentRotationX += mouseX;
        currentRotationY -= mouseY;
        currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        Vector3 direction = rotation * Vector3.back;
        Vector3 targetPosition = target.position + Vector3.up * height;
        Vector3 desiredPosition = targetPosition + direction * distance;

        RaycastHit hit;
        if (Physics.Raycast(targetPosition, direction, out hit, distance))
        {
            desiredPosition = hit.point - direction * 0.5f;
        }

        transform.position = desiredPosition;
        transform.LookAt(targetPosition);
    }
}
