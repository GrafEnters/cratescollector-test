using System;
using UnityEngine;

public class Inventory : MonoBehaviour {
    private InventorySlot[] _slots;

    public event Action<int> OnSlotChanged;

    private void Awake() {
        MainGameConfig config = ConfigManager.Config;
        int slotCount = config != null ? config.InventorySlotCount : 12;
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
        if (slotIndex < 0 || slotIndex >= _slots.Length) {
            return false;
        }

        if (_slots[slotIndex].IsEmpty()) {
            return false;
        }

        ItemData item = _slots[slotIndex].Item;

        GameObject droppedItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        droppedItem.transform.position = position;
        MainGameConfig config = ConfigManager.Config;
        float itemScale = config != null ? config.ItemScale : 0.5f;
        droppedItem.transform.localScale = Vector3.one * itemScale;
        droppedItem.name = item.Name;

        MeshRenderer renderer = droppedItem.GetComponent<MeshRenderer>();
        Material mat = new(Shader.Find("Standard")) {
            color = item.Color
        };
        renderer.material = mat;

        CollectableItem collectable = droppedItem.AddComponent<CollectableItem>();
        collectable.SetItemData(item);

        droppedItem.AddComponent<ItemOutline>();

        Collider collider = droppedItem.GetComponent<Collider>();
        collider.isTrigger = true;

        RemoveItem(slotIndex);
        return true;
    }
}