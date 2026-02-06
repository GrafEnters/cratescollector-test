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
    private Vector2 _dragOffset;
    private int _capturedPointerId = -1;
    private VisualElement _dragGhost;

    private void OnPointerDown(PointerDownEvent evt) {
        if (evt.button == 1) {
            _inventoryUI?.DropItem(_slotIndex);
            return;
        }

        InventorySlot slot = _inventoryUI?.GetInventory()?.GetSlot(_slotIndex);
        if (slot?.IsEmpty() != false) return;

        _isDragging = true;
        _dragStartPosition = evt.position;
        _capturedPointerId = evt.pointerId;
        
        Rect slotWorldBound = _slotElement.worldBound;
        Vector2 slotCenterWorld = new Vector2(slotWorldBound.x + slotWorldBound.width * 0.5f, slotWorldBound.y + slotWorldBound.height * 0.5f);
        Vector2 panelPos = evt.position;
        _dragOffset = panelPos - slotCenterWorld;
        
        CreateDragGhost(evt.position);
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
        if (panel == null) return;

        UpdateDragGhostPosition(position);
        VisualElement elementUnderPointer = PickIgnoringGhost(position, panel);
        ClearDragOverStates();

        VisualElement slotElement = FindSlotElement(elementUnderPointer);
        if (slotElement != null && slotElement != _slotElement) {
            slotElement.AddToClassList("drag-over");
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

        VisualElement targetElement = PickIgnoringGhost(position, panel);
        VisualElement targetSlot = FindSlotElement(targetElement);
        int targetSlotIndex = targetSlot != null ? _inventoryUI.GetSlotIndexFromElement(targetSlot) : -1;

        Inventory inventory = _inventoryUI.GetInventory();
        if (inventory != null) {
            if (targetSlotIndex >= 0 && targetSlotIndex != _slotIndex) {
                inventory.MoveItem(_slotIndex, targetSlotIndex);
            } else if (targetSlotIndex < 0 && !_inventoryUI.IsElementInsideInventory(targetElement)) {
                _inventoryUI.DropItemStack(_slotIndex);
            }
        }

        ClearDragState();
    }

    private VisualElement PickIgnoringGhost(Vector2 position, IPanel panel) {
        if (panel == null) return null;
        if (_dragGhost == null) return panel.Pick(position);

        StyleLength savedLeft = _dragGhost.style.left;
        StyleLength savedTop = _dragGhost.style.top;
        _dragGhost.style.left = -10000f;
        _dragGhost.style.top = -10000f;

        VisualElement picked = panel.Pick(position);

        _dragGhost.style.left = savedLeft;
        _dragGhost.style.top = savedTop;
        return picked;
    }

    private VisualElement FindSlotElement(VisualElement element) {
        while (element != null) {
            if (element.ClassListContains("inventory-slot")) return element;
            element = element.parent;
        }
        return null;
    }

    private void ClearDragOverStates() {
        FindInventoryGrid()?.Query(className: "drag-over").ForEach(elem => elem.RemoveFromClassList("drag-over"));
    }

    private VisualElement FindInventoryGrid() {
        VisualElement current = _slotElement;
        while (current != null) {
            if (current.ClassListContains("inventory-grid")) return current;
            current = current.parent;
        }
        return null;
    }

    private void ClearDragState() {
        _isDragging = false;
        DestroyDragGhost();

        _slotElement?.RemoveFromClassList("dragging");
        VisualElement root = _slotElement?.panel?.visualTree;
        if (root != null) {
            root.UnregisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
            root.UnregisterCallback<PointerUpEvent>(OnGlobalPointerUp);
        }

        if (_capturedPointerId >= 0) {
            try {
                _slotElement?.ReleasePointer(_capturedPointerId);
            } catch { }
        }

        _capturedPointerId = -1;
        ClearDragOverStates();
    }

    private void CreateDragGhost(Vector2 position) {
        VisualElement root = _slotElement?.panel?.visualTree;
        if (root == null) return;

        InventorySlot slot = _inventoryUI?.GetInventory()?.GetSlot(_slotIndex);
        if (slot?.IsEmpty() != false) return;

        float iconWidth = _iconElement.resolvedStyle.width;
        float iconHeight = _iconElement.resolvedStyle.height;
        
        if (iconWidth <= 0) {
            Rect iconRect = _iconElement.worldBound;
            iconWidth = iconRect.width > 0 ? iconRect.width : 50f;
        }
        if (iconHeight <= 0) {
            Rect iconRect = _iconElement.worldBound;
            iconHeight = iconRect.height > 0 ? iconRect.height : 50f;
        }

        _dragGhost = new VisualElement {
            name = "DragGhost"
        };
        _dragGhost.AddToClassList("slot-icon");
        _dragGhost.style.position = Position.Absolute;
        _dragGhost.style.backgroundColor = slot.Item.Color;
        _dragGhost.style.width = iconWidth * 0.75f;
        _dragGhost.style.height = iconHeight * 0.75f;
        _dragGhost.style.opacity = 0.6f;
        _dragGhost.pickingMode = PickingMode.Ignore;

        if (slot.Item.Stackable) {
            Label quantityLabel = new Label {
                text = $"{slot.Quantity}/{slot.Item.MaxStack}"
            };
            quantityLabel.AddToClassList("slot-quantity");
            _dragGhost.Add(quantityLabel);
        }

        StyleKeyword positionKeyword = root.style.position.keyword;
        if (positionKeyword == StyleKeyword.Auto || positionKeyword == StyleKeyword.None) {
            root.style.position = Position.Relative;
        }
        
        root.Add(_dragGhost);
        UpdateDragGhostPosition(position);
        
        _iconElement.style.opacity = 0.3f;
    }

    private void UpdateDragGhostPosition(Vector2 position) {
        if (_dragGhost == null) return;
        Vector2 ghostPosition = position - _dragOffset;
        _dragGhost.style.left = ghostPosition.x;
        _dragGhost.style.top = ghostPosition.y;
    }

    private void DestroyDragGhost() {
        _dragGhost?.parent?.Remove(_dragGhost);
        _dragGhost = null;
        _iconElement.style.opacity = 1f;
    }

    public void UpdateSlot(InventorySlot slot) {
        if (slot?.IsEmpty() != false) {
            _slotElement.AddToClassList("empty");
            _iconElement.style.display = DisplayStyle.None;
            _quantityLabel.text = "";
        } else {
            _slotElement.RemoveFromClassList("empty");
            _iconElement.style.display = DisplayStyle.Flex;
            _iconElement.style.backgroundColor = slot.Item.Color;
            _quantityLabel.text = slot.Item.Stackable ? $"{slot.Quantity}/{slot.Item.MaxStack}" : "";
            _quantityLabel.style.display = slot.Item.Stackable ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}