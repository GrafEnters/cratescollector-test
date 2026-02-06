using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour {
    [SerializeField]
    private LayerMask _itemLayer = -1;
    [SerializeField]
    private Inventory _inventory;
    [SerializeField]
    private ItemSpawner _itemSpawner;
    [SerializeField]
    private InventoryFullNotification _notification;
    private ItemDetector _itemDetector;
    private ItemOutlineManager _outlineManager;
    private ItemPickupHintUI _hintUI;
    private ConfigProvider _configProvider;

    private InputAction _interactAction;
    private CollectableItem _nearbyItem;

    private void Awake() {
        _itemDetector = DIContainer.Instance.Get<IItemDetector>() as ItemDetector;
        _outlineManager = DIContainer.Instance.Get<IItemOutlineManager>() as ItemOutlineManager;
        _hintUI = GetComponent<ItemPickupHintUI>();
        _configProvider = DIContainer.Instance.Get<IConfigProvider>() as ConfigProvider;

        if (_notification == null) {
            _notification = GetComponent<InventoryFullNotification>();
            if (_notification == null) {
                GameObject notificationObject = new("InventoryFullNotification");
                notificationObject.transform.SetParent(transform);
                _notification = notificationObject.AddComponent<InventoryFullNotification>();
            }
        }

        _interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
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
        MainGameConfig config = _configProvider.GetConfig();
        float pickupDistance = config.PickupDistance;

        CollectableItem newNearbyItem = _itemDetector.FindNearestItem(transform.position, pickupDistance, _itemLayer);

        if (newNearbyItem != _nearbyItem) {
            if (_nearbyItem != null) {
                _outlineManager.HideOutline(_nearbyItem);
            }

            if (newNearbyItem != null) {
                _outlineManager.ShowOutline(newNearbyItem);
            }

            _nearbyItem = newNearbyItem;
        }
    }

    private void UpdateHint() {
        if (_hintUI != null) {
            _hintUI.UpdateHint(_nearbyItem);
        }
    }

    private void OnInteract(InputAction.CallbackContext context) {
        if (_nearbyItem == null || _inventory == null) {
            return;
        }

        ItemData itemData = _nearbyItem.GetItemData();
        if (itemData == null) {
            return;
        }

        if (_inventory.AddItem(itemData)) {
            _outlineManager.HideOutline(_nearbyItem);
            _nearbyItem.Pickup();

            if (_itemSpawner != null) {
                _itemSpawner.SpawnItemAtRandomPosition(itemData);
            }

            _nearbyItem = null;
        } else {
            if (_notification != null) {
                _notification.Show();
            }
        }
    }

    private void OnDrawGizmosSelected() {
        MainGameConfig config = _configProvider.GetConfig();
        float pickupDistance = config.PickupDistance;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}