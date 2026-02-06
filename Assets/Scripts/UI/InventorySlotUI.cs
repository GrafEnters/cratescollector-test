using UnityEngine;
using UnityEngine.UIElements;

public class InventorySlotUI {
    private VisualElement slotElement;
    private VisualElement iconElement;
    private Label quantityLabel;
    private int slotIndex;
    private InventoryUI inventoryUI;

    public InventorySlotUI(VisualElement slotElement, int slotIndex, InventoryUI inventoryUI) {
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
    }

    private bool isDragging = false;
    private Vector2 dragStartPosition;
    private int capturedPointerId = -1;

    private void OnPointerDown(PointerDownEvent evt) {
        if (inventoryUI == null || slotElement == null) {
            return;
        }

        if (evt.button == 1) {
            inventoryUI.DropItem(slotIndex);
            return;
        }

        Inventory inventory = inventoryUI.GetInventory();
        if (inventory == null) {
            return;
        }

        InventorySlot slot = inventory.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty()) {
            return;
        }

        isDragging = true;
        dragStartPosition = evt.position;
        capturedPointerId = evt.pointerId;
        slotElement.AddToClassList("dragging");
        slotElement.CapturePointer(evt.pointerId);

        if (slotElement.panel != null) {
            VisualElement root = slotElement.panel.visualTree;
            if (root != null) {
                root.RegisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
                root.RegisterCallback<PointerUpEvent>(OnGlobalPointerUp);
            }
        }

        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt) {
        if (isDragging && evt.pointerId == capturedPointerId && slotElement != null) {
            HandleDragMove(evt.position, slotElement.panel);
        }
    }

    private void OnGlobalPointerMove(PointerMoveEvent evt) {
        if (isDragging && evt.pointerId == capturedPointerId && slotElement != null) {
            HandleDragMove(evt.position, slotElement.panel);
        }
    }

    private void HandleDragMove(Vector2 position, IPanel panel) {
        if (panel == null) {
            return;
        }

        VisualElement elementUnderPointer = panel.Pick(position) as VisualElement;
        ClearDragOverStates();

        if (elementUnderPointer != null) {
            VisualElement slotElement = FindSlotElement(elementUnderPointer);
            if (slotElement != null && slotElement != this.slotElement) {
                slotElement.AddToClassList("drag-over");
            }
        }
    }

    private void OnPointerUp(PointerUpEvent evt) {
        if (isDragging && evt.pointerId == capturedPointerId && slotElement != null) {
            HandleDragEnd(evt.position, slotElement.panel);
        }
    }

    private void OnGlobalPointerUp(PointerUpEvent evt) {
        if (isDragging && evt.pointerId == capturedPointerId && slotElement != null) {
            HandleDragEnd(evt.position, slotElement.panel);
        }
    }

    private void HandleDragEnd(Vector2 position, IPanel panel) {
        if (panel == null || inventoryUI == null) {
            ClearDragState();
            return;
        }

        VisualElement targetElement = panel.Pick(position) as VisualElement;
        VisualElement targetSlot = FindSlotElement(targetElement);

        int targetSlotIndex = -1;
        if (targetSlot != null) {
            targetSlotIndex = inventoryUI.GetSlotIndexFromElement(targetSlot);
        }

        Inventory inventory = inventoryUI.GetInventory();
        if (inventory != null) {
            if (targetSlotIndex >= 0 && targetSlotIndex != slotIndex) {
                inventory.MoveItem(slotIndex, targetSlotIndex);
            } else if (targetSlotIndex < 0) {
                inventoryUI.DropItem(slotIndex);
            }
        }

        ClearDragState();
    }

    private VisualElement FindSlotElement(VisualElement element) {
        if (element == null) {
            return null;
        }

        VisualElement current = element;
        while (current != null) {
            if (current.ClassListContains("inventory-slot")) {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    private void ClearDragOverStates() {
        if (slotElement == null) {
            return;
        }

        VisualElement grid = FindInventoryGrid();
        if (grid != null) {
            grid.Query(className: "drag-over").ForEach(elem => elem.RemoveFromClassList("drag-over"));
        }
    }

    private VisualElement FindInventoryGrid() {
        if (slotElement == null) {
            return null;
        }

        VisualElement current = slotElement;
        while (current != null) {
            if (current.ClassListContains("inventory-grid")) {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    private void ClearDragState() {
        isDragging = false;

        if (slotElement != null) {
            slotElement.RemoveFromClassList("dragging");

            if (slotElement.panel != null) {
                VisualElement root = slotElement.panel.visualTree;
                if (root != null) {
                    root.UnregisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
                    root.UnregisterCallback<PointerUpEvent>(OnGlobalPointerUp);
                }
            }

            if (capturedPointerId >= 0 && slotElement.panel != null) {
                try {
                    slotElement.ReleasePointer(capturedPointerId);
                } catch { }
            }
        }

        capturedPointerId = -1;
        ClearDragOverStates();
    }

    public void UpdateSlot(InventorySlot slot) {
        if (slotElement == null || iconElement == null || quantityLabel == null) {
            return;
        }

        if (slot == null || slot.IsEmpty()) {
            slotElement.AddToClassList("empty");
            iconElement.style.display = DisplayStyle.None;
            quantityLabel.text = "";
        } else {
            slotElement.RemoveFromClassList("empty");
            iconElement.style.display = DisplayStyle.Flex;
            iconElement.style.backgroundColor = slot.item.color;

            if (slot.item.stackable && slot.quantity > 1) {
                quantityLabel.text = slot.quantity.ToString();
                quantityLabel.style.display = DisplayStyle.Flex;
            } else {
                quantityLabel.style.display = DisplayStyle.None;
            }
        }
    }
}