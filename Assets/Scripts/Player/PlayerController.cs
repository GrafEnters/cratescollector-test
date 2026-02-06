using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;
    private InputAction moveAction;
    private Vector2 moveInput;
    private Vector3 velocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        characterController.center = new Vector3(0, characterController.height / 2f, 0);
        
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }
        
        moveAction = new InputAction(type: InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    private void Start()
    {
        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMoveCanceled;
        moveAction?.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void LateUpdate()
    {
        if (characterController.isGrounded && Mathf.Abs(transform.position.y) > 0.05f)
        {
            Vector3 pos = transform.position;
            pos.y = 0f;
            transform.position = pos;
        }
    }

    private void HandleMovement()
    {
        

        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }

        if (cameraTransform == null)
        {
            return;
        }

        Vector3 moveDirection = Vector3.zero;

        if (moveInput.magnitude > 0.1f)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        }

        if (moveDirection.magnitude > 0.1f)
        {
            float moveSpeed = ConfigManager.Config != null ? ConfigManager.Config.playerMoveSpeed : 5f;
            Vector3 move = moveDirection * moveSpeed;
            velocity.x = move.x;
            velocity.z = move.z;
        }
        else
        {
            velocity.x = 0f;
            velocity.z = 0f;
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (moveInput.magnitude > 0.1f && cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            
            if (moveDirection.magnitude > 0.1f)
            {
                float rotationSpeed = ConfigManager.Config != null ? ConfigManager.Config.playerRotationSpeed : 10f;
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
