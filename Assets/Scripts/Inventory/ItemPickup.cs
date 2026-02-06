using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ItemPickup : MonoBehaviour {
    [SerializeField]
    private LayerMask _itemLayer = -1;

    private InputAction _interactAction;
    private CollectableItem _nearbyItem;
    private VisualElement _hintElement;
    private Label _hintLabel;
    private Inventory _inventory;
    private UIDocument _uiDocument;
    private ItemSpawner _itemSpawner;
    private InventoryFullNotification _notification;
    private bool _uiReady;

    private void Awake() {
        _inventory = GetComponent<Inventory>();
        if (_inventory == null) {
            _inventory = FindObjectOfType<Inventory>();
        }

        _itemSpawner = FindObjectOfType<ItemSpawner>();

        _notification = GetComponent<InventoryFullNotification>();
        if (_notification == null) {
            GameObject notificationObject = new("InventoryFullNotification");
            notificationObject.transform.SetParent(transform);
            _notification = notificationObject.AddComponent<InventoryFullNotification>();
        }

        _interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
    }

    private void Start() {
        StartCoroutine(SetupUICoroutine());
    }

    private IEnumerator SetupUICoroutine() {
        yield return null;
        yield return null;

        UIDocument[] allUIDocuments = FindObjectsOfType<UIDocument>();
        foreach (UIDocument doc in allUIDocuments) {
            if (doc.rootVisualElement != null) {
                _uiDocument = doc;
                break;
            }
        }

        if (_uiDocument == null) {
            GameObject uiObject = new("PickupHintUI");
            _uiDocument = uiObject.AddComponent<UIDocument>();
            yield return null;
            yield return null;
        }

        int attempts = 0;
        while (_uiDocument.rootVisualElement == null && attempts < 10) {
            yield return null;
            attempts++;
        }

        if (_uiDocument.rootVisualElement == null) {
            yield break;
        }

        VisualElement root = _uiDocument.rootVisualElement;

        _hintElement = root.Q<VisualElement>("PickupHint");
        if (_hintElement == null) {
            _hintElement = new VisualElement {
                name = "PickupHint",
                style = {
                    position = Position.Absolute,
                    width = 200,
                    height = 50,
                    backgroundColor = new Color(0, 0, 0, 0.9f),
                    display = DisplayStyle.None,
                    borderTopWidth = 2,
                    borderBottomWidth = 2,
                    borderLeftWidth = 2,
                    borderRightWidth = 2,
                    borderTopColor = new Color(1, 1, 1, 0.8f),
                    borderBottomColor = new Color(1, 1, 1, 0.8f),
                    borderLeftColor = new Color(1, 1, 1, 0.8f),
                    borderRightColor = new Color(1, 1, 1, 0.8f),
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5
                }
            };

            _hintLabel = new Label("Нажмите E") {
                style = {
                    fontSize = 24,
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    width = Length.Percent(100),
                    height = Length.Percent(100),
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0
                }
            };

            _hintElement.Add(_hintLabel);
            root.Add(_hintElement);
        } else {
            _hintLabel = _hintElement.Q<Label>();
            if (_hintLabel == null) {
                _hintLabel = new Label("Нажмите E") {
                    style = {
                        fontSize = 24,
                        color = Color.white,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        width = Length.Percent(100),
                        height = Length.Percent(100)
                    }
                };
                _hintElement.Add(_hintLabel);
            }
        }

        _uiReady = true;
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
        if (!_uiReady) {
            return;
        }

        CheckForNearbyItems();
        UpdateHint();
    }

    private void CheckForNearbyItems() {
        MainGameConfig config = ConfigManager.Config;
        float pickupDistance = config != null ? config.PickupDistance : 2f;

        int layerMask = _itemLayer.value != 0 ? _itemLayer.value : -1;

        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupDistance, layerMask);
        CollectableItem closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders) {
            CollectableItem item = col.GetComponent<CollectableItem>();
            if (item != null) {
                float distance = Vector3.Distance(transform.position, item.transform.position);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }

        CollectableItem newNearbyItem = closestItem;

        if (newNearbyItem != _nearbyItem) {
            if (_nearbyItem != null) {
                ItemOutline outline = _nearbyItem.GetComponent<ItemOutline>();
                if (outline != null) {
                    outline.HideOutline();
                }
            }

            if (newNearbyItem != null) {
                ItemOutline outline = newNearbyItem.GetComponent<ItemOutline>();
                if (outline == null) {
                    outline = newNearbyItem.gameObject.AddComponent<ItemOutline>();
                }

                outline.ShowOutline();
            }

            _nearbyItem = newNearbyItem;
        }
    }

    private void UpdateHint() {
        if (_hintElement == null || !_uiReady || _uiDocument == null) {
            return;
        }

        MainGameConfig config = ConfigManager.Config;
        if (config != null && config.IsInventoryBlockingView) {
            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null && inventoryUI.IsOpen()) {
                _hintElement.style.display = DisplayStyle.None;
                return;
            }
        }

        if (_nearbyItem != null && Camera.main != null) {
            float hintHeight = config != null ? config.PickupHintHeight : 0.75f;
            Vector3 worldPosition = _nearbyItem.transform.position + Vector3.up * hintHeight;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            if (screenPosition.z > 0) {
                VisualElement root = _uiDocument.rootVisualElement;

                float elementWidth = _hintElement.resolvedStyle.width;
                float elementHeight = _hintElement.resolvedStyle.height;

                if (elementWidth == 0) {
                    elementWidth = 200;
                }

                if (elementHeight == 0) {
                    elementHeight = 50;
                }

                float panelWidth = root.resolvedStyle.width;
                float panelHeight = root.resolvedStyle.height;

                if (panelWidth <= 0) {
                    panelWidth = Screen.width;
                }

                if (panelHeight <= 0) {
                    panelHeight = Screen.height;
                }

                float scaleX = panelWidth / Screen.width;
                float scaleY = panelHeight / Screen.height;

                float screenX = screenPosition.x;
                float screenY = Screen.height - screenPosition.y;

                float x = screenX * scaleX - elementWidth * 0.5f;
                float y = screenY * scaleY - elementHeight;

                _hintElement.style.display = DisplayStyle.Flex;
                _hintElement.style.left = x;
                _hintElement.style.top = y;
            } else {
                _hintElement.style.display = DisplayStyle.None;
            }
        } else {
            _hintElement.style.display = DisplayStyle.None;
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
            ItemOutline outline = _nearbyItem.GetComponent<ItemOutline>();
            if (outline != null) {
                outline.HideOutline();
            }

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
        MainGameConfig config = ConfigManager.Config;
        float pickupDistance = config != null ? config.PickupDistance : 2f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}