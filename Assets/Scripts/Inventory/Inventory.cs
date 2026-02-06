using System;
using UnityEngine;

public class Inventory : MonoBehaviour {
    private InventorySlot[] _slots;
    private ItemVisualFactory _itemFactory;
    private ConfigProvider _configProvider;

    public event Action<int> OnSlotChanged;

    private void Awake() {
        _configProvider = DIContainer.Instance.Get<IConfigProvider>() as ConfigProvider;
        _itemFactory = DIContainer.Instance.Get<IItemFactory>() as ItemVisualFactory;

        MainGameConfig config = _configProvider.GetConfig();
        int slotCount = config.InventorySlotCount;
        _slots = new InventorySlot[slotCount];
        for (int i = 0; i < slotCount; i++) {
            _slots[i] = new InventorySlot();
        }
    }

    public bool AddItem(ItemData item, int quantity = 1) {
        if (item.Stackable) {
            for (int i = 0; i < _slots.Length; i++) {
                if (!_slots[i].IsEmpty() && _slots[i].Item.ID == item.ID) {
                    int canAdd = Mathf.Min(quantity, _slots[i].Item.MaxStack - _slots[i].Quantity);
                    if (canAdd > 0) {
                        _slots[i].Add(canAdd);
                        OnSlotChanged?.Invoke(i);
                        quantity -= canAdd;
                        if (quantity <= 0) {
                            return true;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < _slots.Length; i++) {
            if (_slots[i].IsEmpty()) {
                _slots[i] = new InventorySlot(item, quantity);
                OnSlotChanged?.Invoke(i);
                return true;
            }
        }

        return false;
    }

    public bool RemoveItem(int slotIndex, int quantity = 1) {
        if (slotIndex < 0 || slotIndex >= _slots.Length) {
            return false;
        }

        if (_slots[slotIndex].IsEmpty()) {
            return false;
        }

        _slots[slotIndex].Remove(quantity);
        OnSlotChanged?.Invoke(slotIndex);
        return true;
    }

    public bool MoveItem(int fromSlot, int toSlot) {
        if (fromSlot < 0 || fromSlot >= _slots.Length) {
            return false;
        }

        if (toSlot < 0 || toSlot >= _slots.Length) {
            return false;
        }

        if (fromSlot == toSlot) {
            return false;
        }

        InventorySlot from = _slots[fromSlot];
        InventorySlot to = _slots[toSlot];

        if (from.IsEmpty()) {
            return false;
        }

        if (to.IsEmpty()) {
            _slots[toSlot] = new InventorySlot(from.Item, from.Quantity);
            from.Clear();
            OnSlotChanged?.Invoke(fromSlot);
            OnSlotChanged?.Invoke(toSlot);
            return true;
        }

        if (to.Item.ID == from.Item.ID && to.Item.Stackable) {
            int canAdd = Mathf.Min(from.Quantity, to.Item.MaxStack - to.Quantity);
            if (canAdd > 0) {
                to.Add(canAdd);
                from.Remove(canAdd);
                OnSlotChanged?.Invoke(fromSlot);
                OnSlotChanged?.Invoke(toSlot);
                return true;
            }
        }

        InventorySlot temp = new(from.Item, from.Quantity);
        _slots[fromSlot] = new InventorySlot(to.Item, to.Quantity);
        _slots[toSlot] = temp;
        OnSlotChanged?.Invoke(fromSlot);
        OnSlotChanged?.Invoke(toSlot);
        return true;
    }

    public InventorySlot GetSlot(int index) {
        if (index < 0 || index >= _slots.Length) {
            return null;
        }

        return _slots[index];
    }

    public int GetSlotCount() {
        return _slots.Length;
    }

    public bool DropItem(int slotIndex, Vector3 position) {
        return DropItem(slotIndex, position, 1);
    }

    public bool DropItem(int slotIndex, Vector3 position, int quantity) {
        if (slotIndex < 0 || slotIndex >= _slots.Length) {
            return false;
        }

        if (_slots[slotIndex].IsEmpty()) {
            return false;
        }

        if (quantity <= 0) {
            return false;
        }

        int availableQuantity = _slots[slotIndex].Quantity;
        if (availableQuantity <= 0) {
            return false;
        }

        if (quantity > availableQuantity) {
            quantity = availableQuantity;
        }

        ItemData item = _slots[slotIndex].Item;
        for (int i = 0; i < quantity; i++) {
            GameObject droppedItem = _itemFactory.CreateItem(item, position);
            if (droppedItem == null) {
                return false;
            }
        }

        return RemoveItem(slotIndex, quantity);
    }
}