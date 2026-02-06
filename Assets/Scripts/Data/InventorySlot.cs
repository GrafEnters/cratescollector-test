using UnityEngine;

[System.Serializable]
public class InventorySlot {
    public ItemData item;
    public int quantity;

    public InventorySlot() {
        item = null;
        quantity = 0;
    }

    public InventorySlot(ItemData item, int quantity) {
        this.item = item;
        this.quantity = quantity;
    }

    public bool IsEmpty() {
        return item == null || quantity <= 0;
    }

    public bool CanAdd(int amount) {
        if (IsEmpty()) {
            return false;
        }

        if (!item.stackable) {
            return false;
        }

        return quantity + amount <= item.maxStack;
    }

    public void Add(int amount) {
        if (CanAdd(amount)) {
            quantity += amount;
        }
    }

    public void Remove(int amount) {
        quantity -= amount;
        if (quantity <= 0) {
            item = null;
            quantity = 0;
        }
    }

    public void Clear() {
        item = null;
        quantity = 0;
    }
}