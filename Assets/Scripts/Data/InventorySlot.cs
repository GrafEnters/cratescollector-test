using System;

[Serializable]
public class InventorySlot {
    public ItemData Item;
    public int Quantity;

    public InventorySlot() { }

    public InventorySlot(ItemData item, int quantity) {
        Item = item;
        Quantity = quantity;
    }

    public bool IsEmpty() {
        return Item == null || Quantity <= 0;
    }

    public bool CanAdd(int amount) {
        return !IsEmpty() && Item.Stackable && Quantity + amount <= Item.MaxStack;
    }

    public void Add(int amount) {
        if (CanAdd(amount)) {
            Quantity += amount;
        }
    }

    public void Remove(int amount) {
        Quantity -= amount;
        if (Quantity <= 0) {
            Item = null;
            Quantity = 0;
        }
    }

    public void Clear() {
        Item = null;
        Quantity = 0;
    }
}