using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class InventoryUI : MonoBehaviour {
    [SerializeField]
    private UIDocument _uiDocument;

    [SerializeField]
    private VisualTreeAsset _inventoryWindowAsset;

    [SerializeField]
    private StyleSheet _inventoryWindowStyle;

    private VisualElement _inventoryWindow;
    private InventorySlotUI[] _slotUIs;
    private Inventory _inventory;
    private bool _isOpen = true;
    private Vector2 _lastCursorPosition;
    private bool _hasStoredCursorPosition;

    private void Awake() {
        _inventory = GetComponent<Inventory>();
        if (_inventory == null) {
            _inventory = FindObjectOfType<Inventory>();
        }

        if (_uiDocument == null) {
            _uiDocument = gameObject.AddComponent<UIDocument>();
        }

        if (_inventoryWindowAsset == null) {
            _inventoryWindowAsset = Resources.Load<VisualTreeAsset>("InventoryWindow");
        }

        if (_inventoryWindowStyle == null) {
            _inventoryWindowStyle = Resources.Load<StyleSheet>("InventoryWindow");
        }
    }

    private void Start() {
        StartCoroutine(SetupUICoroutine());
        if (_inventory != null) {
            _inventory.OnSlotChanged += OnSlotChanged;
        }
    }

    private IEnumerator SetupUICoroutine() {
        yield return null;

        if (_uiDocument == null) {
            _uiDocument = GetComponent<UIDocument>();
        }

        if (_uiDocument == null) {
            Debug.LogError("UIDocument component not found!");
            yield break;
        }

        VisualTreeAsset asset = _inventoryWindowAsset;
        if (asset == null) {
            asset = Resources.Load<VisualTreeAsset>("InventoryWindow");
        }

        if (asset == null) {
            Debug.LogError("Failed to load InventoryWindow UXML from Resources!");
            yield break;
        }

        _uiDocument.visualTreeAsset = asset;

        yield return null;
        yield return null;

        if (_uiDocument.rootVisualElement == null) {
            Debug.LogError("Failed to create root visual element after setting visualTreeAsset!");
            yield break;
        }

        SetupUI();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            ToggleInventory();
        }
    }

    private void SetupUI() {
        if (_uiDocument == null || _uiDocument.rootVisualElement == null) {
            Debug.LogError("UIDocument or rootVisualElement is null!");
            return;
        }

        StyleSheet style = _inventoryWindowStyle;
        if (style == null) {
            style = Resources.Load<StyleSheet>("InventoryWindow");
        }

        if (style != null) {
            _uiDocument.rootVisualElement.styleSheets.Add(style);
        }

        _inventoryWindow = _uiDocument.rootVisualElement.Q<VisualElement>("InventoryWindow");
        if (_inventoryWindow == null) {
            if (_uiDocument.rootVisualElement.childCount > 0) {
                _inventoryWindow = _uiDocument.rootVisualElement[0];
                if (_inventoryWindow != null && _inventoryWindow.name != "InventoryWindow") {
                    _inventoryWindow.name = "InventoryWindow";
                }
            }

            if (_inventoryWindow == null) {
                Debug.LogError("InventoryWindow not found in UXML! Root element: " + _uiDocument.rootVisualElement.name);
                if (_uiDocument.rootVisualElement.childCount > 0) {
                    Debug.LogError("Available children: " + string.Join(", ", _uiDocument.rootVisualElement.Children().Select(c => c.name)));
                }

                return;
            }
        }

        VisualElement grid = _inventoryWindow.Q<VisualElement>("InventoryGrid");
        if (grid == null) {
            Debug.LogError("InventoryGrid not found in UXML!");
            return;
        }

        int slotCount = _inventory != null ? _inventory.GetSlotCount() : 12;
        _slotUIs = new InventorySlotUI[slotCount];

        for (int i = 0; i < slotCount; i++) {
            VisualElement slotElement = grid.Q<VisualElement>($"Slot{i}");
            if (slotElement != null) {
                _slotUIs[i] = new InventorySlotUI(slotElement, i, this);
            }
        }

        UpdateAllSlots();

        if (_inventoryWindow != null) {
            _inventoryWindow.AddToClassList("visible");
        }

        if (_isOpen) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            HideCursor();
        }
    }

    private void ToggleInventory() {
        _isOpen = !_isOpen;
        if (_inventoryWindow != null) {
            if (_isOpen) {
                _inventoryWindow.AddToClassList("visible");
                RestoreCursorPosition();
            } else {
                SaveCursorPosition();
                _inventoryWindow.RemoveFromClassList("visible");
                HideCursor();
            }
        }
    }

    private void SaveCursorPosition() {
        if (Mouse.current != null && Mouse.current.enabled) {
            _lastCursorPosition = Mouse.current.position.ReadValue();
            _hasStoredCursorPosition = true;
        } else {
            _lastCursorPosition = Input.mousePosition;
            _hasStoredCursorPosition = true;
        }
    }

    private void RestoreCursorPosition() {
        if (_hasStoredCursorPosition) {
            StartCoroutine(RestoreCursorPositionCoroutine());
        }
    }

    private IEnumerator RestoreCursorPositionCoroutine() {
        yield return null;

        if (_hasStoredCursorPosition) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            yield return null;

            if (Mouse.current != null && Mouse.current.enabled) {
                Mouse.current.WarpCursorPosition(_lastCursorPosition);
            }
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HideCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnSlotChanged(int slotIndex) {
        if (_slotUIs != null && slotIndex >= 0 && slotIndex < _slotUIs.Length) {
            if (_inventory != null) {
                InventorySlot slot = _inventory.GetSlot(slotIndex);
                _slotUIs[slotIndex].UpdateSlot(slot);
            }
        }
    }

    private void UpdateAllSlots() {
        if (_inventory == null || _slotUIs == null) {
            return;
        }

        for (int i = 0; i < _slotUIs.Length; i++) {
            InventorySlot slot = _inventory.GetSlot(i);
            _slotUIs[i].UpdateSlot(slot);
        }
    }

    public Inventory GetInventory() {
        return _inventory;
    }

    public bool IsOpen() {
        return _isOpen;
    }

    public int GetSlotIndexFromElement(VisualElement element) {
        if (element == null) {
            return -1;
        }

        string elementName = element.name;
        if (elementName.StartsWith("Slot")) {
            string indexStr = elementName.Substring(4);
            if (int.TryParse(indexStr, out int index)) {
                return index;
            }
        }

        VisualElement parent = element.parent;
        while (parent != null) {
            string parentName = parent.name;
            if (parentName.StartsWith("Slot")) {
                string indexStr = parentName.Substring(4);
                if (int.TryParse(indexStr, out int index)) {
                    return index;
                }
            }

            parent = parent.parent;
        }

        return -1;
    }

    public bool IsElementInsideInventory(VisualElement element) {
        if (_inventoryWindow == null || element == null) {
            return false;
        }

        VisualElement current = element;
        while (current != null) {
            if (current == _inventoryWindow) {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    public void DropItem(int slotIndex) {
        if (_inventory == null) {
            return;
        }

        InventorySlot slot = _inventory.GetSlot(slotIndex);
        if (slot.IsEmpty()) {
            return;
        }

        Transform playerTransform = _inventory.transform;
        if (playerTransform == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) {
                return;
            }

            playerTransform = player.transform;
        }

        Vector3 forwardDirection = Vector3.forward;
        Camera mainCamera = Camera.main;
        if (mainCamera != null) {
            forwardDirection = mainCamera.transform.forward;
            forwardDirection.y = 0f;
            forwardDirection.Normalize();
        } else {
            forwardDirection = playerTransform.forward;
        }

        Vector3 dropPosition = playerTransform.position + forwardDirection * 2f + Vector3.up * 0.5f;
        _inventory.DropItem(slotIndex, dropPosition);
    }

    public void DropItemStack(int slotIndex) {
        if (_inventory == null) {
            return;
        }

        InventorySlot slot = _inventory.GetSlot(slotIndex);
        if (slot.IsEmpty()) {
            return;
        }

        Transform playerTransform = _inventory.transform;
        if (playerTransform == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) {
                return;
            }

            playerTransform = player.transform;
        }

        Vector3 forwardDirection = Vector3.forward;
        Camera mainCamera = Camera.main;
        if (mainCamera != null) {
            forwardDirection = mainCamera.transform.forward;
            forwardDirection.y = 0f;
            forwardDirection.Normalize();
        } else {
            forwardDirection = playerTransform.forward;
        }

        Vector3 dropPosition = playerTransform.position + forwardDirection * 2f + Vector3.up * 0.5f;
        _inventory.DropItem(slotIndex, dropPosition, slot.Quantity);
    }

    private void OnDestroy() {
        if (_inventory != null) {
            _inventory.OnSlotChanged -= OnSlotChanged;
        }
    }
}