using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int slotCount = 12;
    private InventorySlot[] slots;

    public event Action<int> OnSlotChanged;

    private void Awake()
    {
        slots = new InventorySlot[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            slots[i] = new InventorySlot();
        }
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item.stackable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty() && slots[i].item.id == item.id)
                {
                    int canAdd = Mathf.Min(quantity, slots[i].item.maxStack - slots[i].quantity);
                    if (canAdd > 0)
                    {
                        slots[i].Add(canAdd);
                        OnSlotChanged?.Invoke(i);
                        quantity -= canAdd;
                        if (quantity <= 0) return true;
                    }
                }
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i] = new InventorySlot(item, quantity);
                OnSlotChanged?.Invoke(i);
                return true;
            }
        }

        return false;
    }

    public bool RemoveItem(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        if (slots[slotIndex].IsEmpty()) return false;

        slots[slotIndex].Remove(quantity);
        OnSlotChanged?.Invoke(slotIndex);
        return true;
    }

    public bool MoveItem(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || fromSlot >= slots.Length) return false;
        if (toSlot < 0 || toSlot >= slots.Length) return false;
        if (fromSlot == toSlot) return false;

        InventorySlot from = slots[fromSlot];
        InventorySlot to = slots[toSlot];

        if (from.IsEmpty()) return false;

        if (to.IsEmpty())
        {
            slots[toSlot] = new InventorySlot(from.item, from.quantity);
            from.Clear();
            OnSlotChanged?.Invoke(fromSlot);
            OnSlotChanged?.Invoke(toSlot);
            return true;
        }

        if (to.item.id == from.item.id && to.item.stackable)
        {
            int canAdd = Mathf.Min(from.quantity, to.item.maxStack - to.quantity);
            if (canAdd > 0)
            {
                to.Add(canAdd);
                from.Remove(canAdd);
                OnSlotChanged?.Invoke(fromSlot);
                OnSlotChanged?.Invoke(toSlot);
                return true;
            }
        }

        InventorySlot temp = new InventorySlot(from.item, from.quantity);
        slots[fromSlot] = new InventorySlot(to.item, to.quantity);
        slots[toSlot] = temp;
        OnSlotChanged?.Invoke(fromSlot);
        OnSlotChanged?.Invoke(toSlot);
        return true;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index];
    }

    public int GetSlotCount()
    {
        return slots.Length;
    }

    public bool DropItem(int slotIndex, Vector3 position)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        if (slots[slotIndex].IsEmpty()) return false;

        ItemData item = slots[slotIndex].item;
        int quantityToDrop = 1;

        GameObject droppedItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        droppedItem.transform.position = position;
        droppedItem.transform.localScale = Vector3.one * 0.5f;
        droppedItem.name = item.name;

        MeshRenderer renderer = droppedItem.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = item.color;
        renderer.material = mat;

        CollectableItem collectable = droppedItem.AddComponent<CollectableItem>();
        collectable.SetItemData(item);

        Collider collider = droppedItem.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        RemoveItem(slotIndex, quantityToDrop);
        return true;
    }
}
