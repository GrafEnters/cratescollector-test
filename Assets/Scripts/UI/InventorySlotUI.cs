using UnityEngine;
using UnityEngine.UIElements;

public class InventorySlotUI
{
    private VisualElement slotElement;
    private VisualElement iconElement;
    private Label quantityLabel;
    private int slotIndex;
    private InventoryUI inventoryUI;

    public InventorySlotUI(VisualElement slotElement, int slotIndex, InventoryUI inventoryUI)
    {
        this.slotElement = slotElement;
        this.slotIndex = slotIndex;
        this.inventoryUI = inventoryUI;

        iconElement = new VisualElement();
        iconElement.name = "SlotIcon";
        iconElement.AddToClassList("slot-icon");
        slotElement.Add(iconElement);

        quantityLabel = new Label();
        quantityLabel.name = "SlotQuantity";
        quantityLabel.AddToClassList("slot-quantity");
        slotElement.Add(quantityLabel);

        slotElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
        slotElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        slotElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
        slotElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
    }

    private bool isDragging = false;
    private Vector2 dragStartPosition;

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button == 1)
        {
            inventoryUI.DropItem(slotIndex);
            return;
        }

        InventorySlot slot = inventoryUI.GetInventory().GetSlot(slotIndex);
        if (!slot.IsEmpty())
        {
            isDragging = true;
            dragStartPosition = evt.position;
            slotElement.AddToClassList("dragging");
            evt.StopPropagation();
        }
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (isDragging)
        {
            VisualElement elementUnderPointer = evt.target as VisualElement;
            ClearDragOverStates();
            
            if (elementUnderPointer != null)
            {
                VisualElement slotElement = FindSlotElement(elementUnderPointer);
                if (slotElement != null && slotElement != this.slotElement)
                {
                    slotElement.AddToClassList("drag-over");
                }
            }
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (isDragging)
        {
            VisualElement targetElement = evt.target as VisualElement;
            VisualElement targetSlot = FindSlotElement(targetElement);
            
            int targetSlotIndex = -1;
            if (targetSlot != null)
            {
                targetSlotIndex = inventoryUI.GetSlotIndexFromElement(targetSlot);
            }

            if (targetSlotIndex >= 0 && targetSlotIndex != slotIndex)
            {
                inventoryUI.GetInventory().MoveItem(slotIndex, targetSlotIndex);
            }
            else if (targetSlotIndex < 0)
            {
                inventoryUI.DropItem(slotIndex);
            }

            ClearDragState();
            evt.StopPropagation();
        }
    }

    private VisualElement FindSlotElement(VisualElement element)
    {
        if (element == null) return null;
        
        VisualElement current = element;
        while (current != null)
        {
            if (current.ClassListContains("inventory-slot"))
            {
                return current;
            }
            current = current.parent;
        }
        
        return null;
    }

    private void ClearDragOverStates()
    {
        VisualElement root = slotElement.parent;
        if (root != null)
        {
            foreach (VisualElement child in root.Children())
            {
                if (child.ClassListContains("drag-over"))
                {
                    child.RemoveFromClassList("drag-over");
                }
            }
        }
    }

    private void OnPointerLeave(PointerLeaveEvent evt)
    {
        if (isDragging)
        {
            ClearDragState();
        }
    }

    private void ClearDragState()
    {
        isDragging = false;
        slotElement.RemoveFromClassList("dragging");
        ClearDragOverStates();
    }

    public void UpdateSlot(InventorySlot slot)
    {
        if (slot.IsEmpty())
        {
            slotElement.AddToClassList("empty");
            iconElement.style.display = DisplayStyle.None;
            quantityLabel.text = "";
        }
        else
        {
            slotElement.RemoveFromClassList("empty");
            iconElement.style.display = DisplayStyle.Flex;
            iconElement.style.backgroundColor = slot.item.color;

            if (slot.item.stackable && slot.quantity > 1)
            {
                quantityLabel.text = slot.quantity.ToString();
                quantityLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                quantityLabel.style.display = DisplayStyle.None;
            }
        }
    }
}
