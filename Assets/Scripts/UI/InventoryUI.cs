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

    [SerializeField]
    private Inventory _inventoryReference;

    private void Awake() {
        _inventory = GetComponent<Inventory>() ?? _inventoryReference;
        _uiDocument = _uiDocument ?? gameObject.AddComponent<UIDocument>();
        _inventoryWindowAsset = _inventoryWindowAsset ?? Resources.Load<VisualTreeAsset>("InventoryWindow");
        _inventoryWindowStyle = _inventoryWindowStyle ?? Resources.Load<StyleSheet>("InventoryWindow");
    }

    private void Start() {
        StartCoroutine(SetupUICoroutine());
        if (_inventory != null) {
            _inventory.OnSlotChanged += OnSlotChanged;
        }
    }

    private IEnumerator SetupUICoroutine() {
        yield return null;

        _uiDocument = _uiDocument ?? GetComponent<UIDocument>();
        VisualTreeAsset asset = _inventoryWindowAsset ?? Resources.Load<VisualTreeAsset>("InventoryWindow");
        
        if (_uiDocument == null || asset == null) yield break;

        _uiDocument.visualTreeAsset = asset;
        yield return null;
        yield return null;

        if (_uiDocument.rootVisualElement != null) {
            SetupUI();
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            ToggleInventory();
        }
    }

    private void SetupUI() {
        if (_uiDocument?.rootVisualElement == null) return;

        StyleSheet style = _inventoryWindowStyle ?? Resources.Load<StyleSheet>("InventoryWindow");
        if (style != null) {
            _uiDocument.rootVisualElement.styleSheets.Add(style);
        }

        _inventoryWindow = _uiDocument.rootVisualElement.Q<VisualElement>("InventoryWindow") 
            ?? (_uiDocument.rootVisualElement.childCount > 0 ? _uiDocument.rootVisualElement[0] : null);
        
        if (_inventoryWindow == null) return;

        VisualElement grid = _inventoryWindow.Q<VisualElement>("InventoryGrid");
        if (grid == null) return;

        int slotCount = _inventory?.GetSlotCount() ?? 12;
        _slotUIs = new InventorySlotUI[slotCount];

        for (int i = 0; i < slotCount; i++) {
            VisualElement slotElement = grid.Q<VisualElement>($"Slot{i}");
            if (slotElement != null) {
                _slotUIs[i] = new InventorySlotUI(slotElement, i, this);
            }
        }

        UpdateAllSlots();
        _inventoryWindow.AddToClassList("visible");

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
        _lastCursorPosition = Mouse.current?.enabled == true ? Mouse.current.position.ReadValue() : Input.mousePosition;
        _hasStoredCursorPosition = true;
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
        if (slotIndex >= 0 && slotIndex < _slotUIs?.Length) {
            _slotUIs[slotIndex].UpdateSlot(_inventory?.GetSlot(slotIndex));
        }
    }

    private void UpdateAllSlots() {
        if (_inventory == null || _slotUIs == null) return;

        for (int i = 0; i < _slotUIs.Length; i++) {
            _slotUIs[i].UpdateSlot(_inventory.GetSlot(i));
        }
    }

    public Inventory GetInventory() {
        return _inventory;
    }

    public bool IsOpen() {
        return _isOpen;
    }

    public int GetSlotIndexFromElement(VisualElement element) {
        while (element != null) {
            if (element.name.StartsWith("Slot") && int.TryParse(element.name.Substring(4), out int index)) {
                return index;
            }
            element = element.parent;
        }
        return -1;
    }

    public bool IsElementInsideInventory(VisualElement element) {
        while (element != null) {
            if (element == _inventoryWindow) return true;
            element = element.parent;
        }
        return false;
    }

    public void DropItem(int slotIndex) {
        if (_inventory?.GetSlot(slotIndex)?.IsEmpty() != false) return;
        Vector3 dropPosition = CalculateDropPosition();
        if (dropPosition != Vector3.zero) {
            _inventory.DropItem(slotIndex, dropPosition);
        }
    }

    public void DropItemStack(int slotIndex) {
        InventorySlot slot = _inventory?.GetSlot(slotIndex);
        if (slot?.IsEmpty() != false) return;
        Vector3 dropPosition = CalculateDropPosition();
        if (dropPosition != Vector3.zero) {
            _inventory.DropItem(slotIndex, dropPosition, slot.Quantity);
        }
    }

    private Vector3 CalculateDropPosition() {
        Transform playerTransform = _inventory?.transform ?? transform;
        Camera mainCamera = Camera.main;
        Vector3 forwardDirection = mainCamera != null ? mainCamera.transform.forward : playerTransform.forward;
        forwardDirection.y = 0f;
        forwardDirection.Normalize();
        return playerTransform.position + forwardDirection * 2f + Vector3.up * 0.5f;
    }

    private void OnDestroy() {
        _inventory.OnSlotChanged -= OnSlotChanged;
    }
}