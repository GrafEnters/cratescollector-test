using UnityEngine;

public class InventoryStateProvider : MonoBehaviour, IInventoryStateProvider {
    private InventoryUI _inventoryUI;

    private void Awake() {
        _inventoryUI = GetComponent<InventoryUI>();
        if (_inventoryUI == null) {
            _inventoryUI = FindObjectOfType<InventoryUI>();
        }
    }

    public bool IsInventoryOpen() {
        return _inventoryUI != null && _inventoryUI.IsOpen();
    }
}
