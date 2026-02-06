using UnityEngine;

public class InventoryStateProvider : MonoBehaviour, IInventoryStateProvider {
    [SerializeField]
    private InventoryUI _inventoryUI;

    private void Awake() {
        if (_inventoryUI == null) {
            _inventoryUI = GetComponent<InventoryUI>();
        }
        if (_inventoryUI == null) {
            _inventoryUI = GetComponentInParent<InventoryUI>();
        }
    }

    public bool IsInventoryOpen() {
        return _inventoryUI != null && _inventoryUI.IsOpen();
    }
}
