using UnityEngine;

[CreateAssetMenu(fileName = "MainGameConfig", menuName = "Game/MainGameConfig")]
public class MainGameConfig : ScriptableObject
{
    public bool IsInventoryBlockingView = true;

    [Header("Player")]
    public float playerMoveSpeed = 5f;
    public float playerRotationSpeed = 10f;

    [Header("Inventory")]
    public int inventorySlotCount = 12;

    [Header("Item Spawner")]
    public int itemSpawnerItemCount = 6;
    public float itemSpawnerRadius = 10f;
    public float itemSpawnerMinDistance = 2f;
    public float itemSpawnerHeight = 0.5f;
    public float itemScale = 0.5f;
    public int itemSpawnerMaxAttempts = 50;

    [Header("Camera")]
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;
    public float cameraRotationSpeed = 2f;
    public float cameraMinVerticalAngle = -30f;
    public float cameraMaxVerticalAngle = 60f;

    [Header("Item Pickup")]
    public float pickupDistance = 2f;
    public float pickupHintHeight = 0.75f;
    
    [Header("Item Outline")]
    public Color outlineColor = new Color(1f, 1f, 0f, 0.8f);
    [Range(0.0f, 10.0f)]
    public float outlineWidth = 2.0f;
}
