using UnityEngine;

public class InventoryStateProvider : MonoBehaviour, IInventoryStateProvider {
    [SerializeField]
    private InventoryUI _inventoryUI;

    private void Awake() {
        _inventoryUI = _inventoryUI ?? GetComponent<InventoryUI>() ?? GetComponentInParent<InventoryUI>();
    }

    public bool IsInventoryOpen() {
        return _inventoryUI?.IsOpen() == true;
    }
}
