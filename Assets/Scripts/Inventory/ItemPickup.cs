using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour {
    [SerializeField]
    private LayerMask _itemLayer = -1;

    [SerializeField]
    private ItemSpawner _itemSpawner;

    private Inventory _inventory;
    private InventoryFullNotification _notification;
    private IItemDetector _itemDetector;
    private IItemOutlineManager _outlineManager;
    private ItemPickupHintUI _hintUI;
    private IConfigProvider _configProvider;
    private float _cachedPickupDistance;

    private InputAction _interactAction;
    private CollectableItem _nearbyItem;

    private void Awake() {
        DIContainer.Instance.TryGet<IItemDetector>(out _itemDetector);
        DIContainer.Instance.TryGet<IItemOutlineManager>(out _outlineManager);
        DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider);

        _inventory = GetComponent<Inventory>();
        _hintUI = GetComponent<ItemPickupHintUI>();
        _notification = GetComponent<InventoryFullNotification>();
        _interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
    }

    private void Start() {
        _cachedPickupDistance = _configProvider?.GetConfig()?.PickupDistance ?? 2f;
    }

    private void OnEnable() {
        _interactAction?.Enable();
        _interactAction.performed += OnInteract;
    }

    private void OnDisable() {
        _interactAction.performed -= OnInteract;
        _interactAction?.Disable();
    }

    private void Update() {
        CheckForNearbyItems();
        UpdateHint();
    }

    private void CheckForNearbyItems() {
        if (_itemDetector == null || _outlineManager == null) return;

        CollectableItem newNearbyItem = _itemDetector.FindNearestItem(transform.position, _cachedPickupDistance, _itemLayer);

        if (newNearbyItem != _nearbyItem) {
            _outlineManager.HideOutline(_nearbyItem);
            _outlineManager.ShowOutline(newNearbyItem);
            _nearbyItem = newNearbyItem;
        }
    }

    private void UpdateHint() {
        _hintUI?.UpdateHint(_nearbyItem);
    }

    private void OnInteract(InputAction.CallbackContext context) {
        if (_nearbyItem == null || _inventory == null) return;

        ItemData itemData = _nearbyItem.GetItemData();
        if (itemData == null) return;

        if (_inventory.AddItem(itemData)) {
            _outlineManager?.HideOutline(_nearbyItem);
            _nearbyItem.Pickup();
            _itemSpawner?.SpawnItemAtRandomPosition(itemData);
            _nearbyItem = null;
        } else {
            _notification?.Show();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _cachedPickupDistance);
    }
}