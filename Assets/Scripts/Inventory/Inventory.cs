using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {
    private InventorySlot[] _slots;
    private IItemFactory _itemFactory;
    private IConfigProvider _configProvider;

    public event Action<int> OnSlotChanged;

    private void Awake() {
        DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider);
        DIContainer.Instance.TryGet<IItemFactory>(out _itemFactory);

        int slotCount = _configProvider?.GetConfig()?.InventorySlotCount ?? 12;
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
        if (slotIndex < 0 || slotIndex >= _slots.Length || _slots[slotIndex].IsEmpty()) {
            return false;
        }

        _slots[slotIndex].Remove(quantity);
        OnSlotChanged?.Invoke(slotIndex);
        return true;
    }

    public bool MoveItem(int fromSlot, int toSlot) {
        if (fromSlot < 0 || fromSlot >= _slots.Length || toSlot < 0 || toSlot >= _slots.Length || fromSlot == toSlot) {
            return false;
        }

        InventorySlot from = _slots[fromSlot];
        InventorySlot to = _slots[toSlot];

        if (from.IsEmpty()) return false;

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
        if (slotIndex < 0 || slotIndex >= _slots.Length || _slots[slotIndex].IsEmpty() || quantity <= 0) {
            return false;
        }

        quantity = Mathf.Min(quantity, _slots[slotIndex].Quantity);
        ItemData item = _slots[slotIndex].Item;

        for (int i = 0; i < quantity; i++) {
            _itemFactory?.CreateItem(item, position);
        }

        return RemoveItem(slotIndex, quantity);
    }
}