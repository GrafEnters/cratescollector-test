using UnityEngine;

[CreateAssetMenu(fileName = "MainGameConfig", menuName = "Game/MainGameConfig")]
public class MainGameConfig : ScriptableObject {
    public bool IsInventoryBlockingView = true;

    [Header("Player")]
    public float PlayerMoveSpeed = 5f;

    public float PlayerRotationSpeed = 10f;

    [Header("Inventory")]
    public int InventorySlotCount = 12;

    [Header("Item Spawner")]
    public int ItemSpawnerItemCount = 6;

    public float ItemSpawnerRadius = 10f;
    public float ItemSpawnerMinDistance = 2f;
    public float ItemSpawnerMinDistanceFromPlayer = 3f;
    public float ItemSpawnerHeight = 0.5f;
    public float ItemScale = 0.5f;
    public int ItemSpawnerMaxAttempts = 50;

    [Header("Camera")]
    public float CameraDistance = 5f;

    public float CameraHeight = 2f;
    public float CameraRotationSpeed = 2f;
    public float CameraMinVerticalAngle = -30f;
    public float CameraMaxVerticalAngle = 60f;

    [Header("Item Pickup")]
    public float PickupDistance = 2f;

    public float PickupHintHeight = 0.75f;

    [Header("Item Outline")]
    public Color OutlineColor = new(1f, 1f, 0f, 0.8f);

    [Range(0.0f, 10.0f)]
    public float OutlineWidth = 2.0f;
}