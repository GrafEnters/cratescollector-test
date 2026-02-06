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

    private InputAction _interactAction;
    private CollectableItem _nearbyItem;

    private void Awake() {
        _itemDetector = DIContainer.Instance.Get<IItemDetector>();
        _outlineManager = DIContainer.Instance.Get<IItemOutlineManager>();
        _inventory = GetComponent<Inventory>();
        _hintUI = GetComponent<ItemPickupHintUI>();
        _notification = GetComponent<InventoryFullNotification>();
        _configProvider = DIContainer.Instance.Get<IConfigProvider>();

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
        if (_configProvider == null || _itemDetector == null || _outlineManager == null) {
            return;
        }

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
        _hintUI.UpdateHint(_nearbyItem);
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
            _notification.Show();
        }
    }

    private void OnDrawGizmosSelected() {
        if (_configProvider == null) {
            return;
        }

        MainGameConfig config = _configProvider.GetConfig();
        float pickupDistance = config.PickupDistance;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}