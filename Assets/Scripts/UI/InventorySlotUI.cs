using UnityEngine;
using UnityEngine.UIElements;

public class InventorySlotUI {
    private readonly VisualElement _slotElement;
    private readonly VisualElement _iconElement;
    private readonly Label _quantityLabel;
    private readonly int _slotIndex;
    private readonly InventoryUI _inventoryUI;

    public InventorySlotUI(VisualElement slotElement, int slotIndex, InventoryUI inventoryUI) {
        _slotElement = slotElement;
        _slotIndex = slotIndex;
        _inventoryUI = inventoryUI;

        _iconElement = new VisualElement {
            name = "SlotIcon"
        };
        _iconElement.AddToClassList("slot-icon");
        slotElement.Add(_iconElement);

        _quantityLabel = new Label {
            name = "SlotQuantity"
        };
        _quantityLabel.AddToClassList("slot-quantity");
        slotElement.Add(_quantityLabel);

        slotElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
        slotElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        slotElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private bool _isDragging;
    private Vector2 _dragStartPosition;
    private int _capturedPointerId = -1;

    private void OnPointerDown(PointerDownEvent evt) {
        if (_inventoryUI == null || _slotElement == null) {
            return;
        }

        if (evt.button == 1) {
            _inventoryUI.DropItem(_slotIndex);
            return;
        }

        Inventory inventory = _inventoryUI.GetInventory();
        if (inventory == null) {
            return;
        }

        InventorySlot slot = inventory.GetSlot(_slotIndex);
        if (slot == null || slot.IsEmpty()) {
            return;
        }

        _isDragging = true;
        _dragStartPosition = evt.position;
        _capturedPointerId = evt.pointerId;
        _slotElement.AddToClassList("dragging");
        _slotElement.CapturePointer(evt.pointerId);

        if (_slotElement.panel != null) {
            VisualElement root = _slotElement.panel.visualTree;
            if (root != null) {
                root.RegisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
                root.RegisterCallback<PointerUpEvent>(OnGlobalPointerUp);
            }
        }

        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt) {
        if (_isDragging && evt.pointerId == _capturedPointerId && _slotElement != null) {
            HandleDragMove(evt.position, _slotElement.panel);
        }
    }

    private void OnGlobalPointerMove(PointerMoveEvent evt) {
        if (_isDragging && evt.pointerId == _capturedPointerId && _slotElement != null) {
            HandleDragMove(evt.position, _slotElement.panel);
        }
    }

    private void HandleDragMove(Vector2 position, IPanel panel) {
        if (panel == null) {
            return;
        }

        VisualElement elementUnderPointer = panel.Pick(position);
        ClearDragOverStates();

        if (elementUnderPointer != null) {
            VisualElement slotElement = FindSlotElement(elementUnderPointer);
            if (slotElement != null && slotElement != _slotElement) {
                slotElement.AddToClassList("drag-over");
            }
        }
    }

    private void OnPointerUp(PointerUpEvent evt) {
        if (_isDragging && evt.pointerId == _capturedPointerId && _slotElement != null) {
            HandleDragEnd(evt.position, _slotElement.panel);
        }
    }

    private void OnGlobalPointerUp(PointerUpEvent evt) {
        if (_isDragging && evt.pointerId == _capturedPointerId && _slotElement != null) {
            HandleDragEnd(evt.position, _slotElement.panel);
        }
    }

    private void HandleDragEnd(Vector2 position, IPanel panel) {
        if (panel == null || _inventoryUI == null) {
            ClearDragState();
            return;
        }

        VisualElement targetElement = panel.Pick(position);
        VisualElement targetSlot = FindSlotElement(targetElement);

        int targetSlotIndex = -1;
        if (targetSlot != null) {
            targetSlotIndex = _inventoryUI.GetSlotIndexFromElement(targetSlot);
        }

        Inventory inventory = _inventoryUI.GetInventory();
        if (inventory != null) {
            if (targetSlotIndex >= 0 && targetSlotIndex != _slotIndex) {
                inventory.MoveItem(_slotIndex, targetSlotIndex);
            } else if (targetSlotIndex < 0) {
                _inventoryUI.DropItemStack(_slotIndex);
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
        if (_slotElement == null) {
            return;
        }

        VisualElement grid = FindInventoryGrid();
        if (grid != null) {
            grid.Query(className: "drag-over").ForEach(elem => elem.RemoveFromClassList("drag-over"));
        }
    }

    private VisualElement FindInventoryGrid() {
        if (_slotElement == null) {
            return null;
        }

        VisualElement current = _slotElement;
        while (current != null) {
            if (current.ClassListContains("inventory-grid")) {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    private void ClearDragState() {
        _isDragging = false;

        if (_slotElement != null) {
            _slotElement.RemoveFromClassList("dragging");

            if (_slotElement.panel != null) {
                VisualElement root = _slotElement.panel.visualTree;
                if (root != null) {
                    root.UnregisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
                    root.UnregisterCallback<PointerUpEvent>(OnGlobalPointerUp);
                }
            }

            if (_capturedPointerId >= 0 && _slotElement.panel != null) {
                try {
                    _slotElement.ReleasePointer(_capturedPointerId);
                } catch { }
            }
        }

        _capturedPointerId = -1;
        ClearDragOverStates();
    }

    public void UpdateSlot(InventorySlot slot) {
        if (_slotElement == null || _iconElement == null || _quantityLabel == null) {
            return;
        }

        if (slot == null || slot.IsEmpty()) {
            _slotElement.AddToClassList("empty");
            _iconElement.style.display = DisplayStyle.None;
            _quantityLabel.text = "";
        } else {
            _slotElement.RemoveFromClassList("empty");
            _iconElement.style.display = DisplayStyle.Flex;
            _iconElement.style.backgroundColor = slot.Item.Color;

            if (slot.Item.Stackable) {
                _quantityLabel.text = $"{slot.Quantity}/{slot.Item.MaxStack}";
                _quantityLabel.style.display = DisplayStyle.Flex;
            } else {
                _quantityLabel.style.display = DisplayStyle.None;
            }
        }
    }
}