using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset inventoryWindowAsset;
    [SerializeField] private StyleSheet inventoryWindowStyle;

    private VisualElement inventoryWindow;
    private InventorySlotUI[] slotUIs;
    private Inventory inventory;
    private bool isOpen = true;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
        }

        if (uiDocument == null)
        {
            uiDocument = gameObject.AddComponent<UIDocument>();
        }

        if (inventoryWindowAsset == null)
        {
            inventoryWindowAsset = Resources.Load<VisualTreeAsset>("InventoryWindow");
        }

        if (inventoryWindowStyle == null)
        {
            inventoryWindowStyle = Resources.Load<StyleSheet>("InventoryWindow");
        }
    }

    private void Start()
    {
        StartCoroutine(SetupUICoroutine());
        if (inventory != null)
        {
            inventory.OnSlotChanged += OnSlotChanged;
        }
    }

    private System.Collections.IEnumerator SetupUICoroutine()
    {
        yield return null;
        
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component not found!");
            yield break;
        }

        VisualTreeAsset asset = inventoryWindowAsset;
        if (asset == null)
        {
            asset = Resources.Load<VisualTreeAsset>("InventoryWindow");
        }

        if (asset == null)
        {
            Debug.LogError("Failed to load InventoryWindow UXML from Resources!");
            yield break;
        }

        uiDocument.visualTreeAsset = asset;

        yield return null;
        yield return null;

        if (uiDocument.rootVisualElement == null)
        {
            Debug.LogError("Failed to create root visual element after setting visualTreeAsset!");
            yield break;
        }

        SetupUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void SetupUI()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            Debug.LogError("UIDocument or rootVisualElement is null!");
            return;
        }

        StyleSheet style = inventoryWindowStyle;
        if (style == null)
        {
            style = Resources.Load<StyleSheet>("InventoryWindow");
        }

        if (style != null)
        {
            uiDocument.rootVisualElement.styleSheets.Add(style);
        }

        inventoryWindow = uiDocument.rootVisualElement.Q<VisualElement>("InventoryWindow");
        if (inventoryWindow == null)
        {
            if (uiDocument.rootVisualElement.childCount > 0)
            {
                inventoryWindow = uiDocument.rootVisualElement[0] as VisualElement;
                if (inventoryWindow != null && inventoryWindow.name != "InventoryWindow")
                {
                    inventoryWindow.name = "InventoryWindow";
                }
            }
            
            if (inventoryWindow == null)
            {
                Debug.LogError("InventoryWindow not found in UXML! Root element: " + uiDocument.rootVisualElement.name);
                if (uiDocument.rootVisualElement.childCount > 0)
                {
                    Debug.LogError("Available children: " + string.Join(", ", uiDocument.rootVisualElement.Children().Select(c => c.name)));
                }
                return;
            }
        }

        VisualElement grid = inventoryWindow.Q<VisualElement>("InventoryGrid");
        if (grid == null)
        {
            Debug.LogError("InventoryGrid not found in UXML!");
            return;
        }

        int slotCount = inventory != null ? inventory.GetSlotCount() : 12;
        slotUIs = new InventorySlotUI[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            VisualElement slotElement = grid.Q<VisualElement>($"Slot{i}");
            if (slotElement != null)
            {
                slotUIs[i] = new InventorySlotUI(slotElement, i, this);
            }
        }

        UpdateAllSlots();
        
        if (inventoryWindow != null)
        {
            inventoryWindow.AddToClassList("visible");
        }
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;
        if (inventoryWindow != null)
        {
            if (isOpen)
            {
                inventoryWindow.AddToClassList("visible");
            }
            else
            {
                inventoryWindow.RemoveFromClassList("visible");
            }
        }
    }

    private void OnSlotChanged(int slotIndex)
    {
        if (slotUIs != null && slotIndex >= 0 && slotIndex < slotUIs.Length)
        {
            if (inventory != null)
            {
                InventorySlot slot = inventory.GetSlot(slotIndex);
                slotUIs[slotIndex].UpdateSlot(slot);
            }
        }
    }

    private void UpdateAllSlots()
    {
        if (inventory == null || slotUIs == null) return;

        for (int i = 0; i < slotUIs.Length; i++)
        {
            InventorySlot slot = inventory.GetSlot(i);
            slotUIs[i].UpdateSlot(slot);
        }
    }

    public Inventory GetInventory()
    {
        return inventory;
    }

    public int GetSlotIndexFromElement(VisualElement element)
    {
        if (element == null) return -1;

        string elementName = element.name;
        if (elementName.StartsWith("Slot"))
        {
            string indexStr = elementName.Substring(4);
            if (int.TryParse(indexStr, out int index))
            {
                return index;
            }
        }

        VisualElement parent = element.parent;
        while (parent != null)
        {
            string parentName = parent.name;
            if (parentName.StartsWith("Slot"))
            {
                string indexStr = parentName.Substring(4);
                if (int.TryParse(indexStr, out int index))
                {
                    return index;
                }
            }
            parent = parent.parent;
        }

        return -1;
    }

    public void DropItem(int slotIndex)
    {
        if (inventory == null) return;

        InventorySlot slot = inventory.GetSlot(slotIndex);
        if (slot.IsEmpty()) return;

        Transform playerTransform = inventory.transform;
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (playerTransform == null) return;

        Camera mainCamera = Camera.main;
        Vector3 forwardDirection = Vector3.forward;
        
        if (mainCamera != null)
        {
            forwardDirection = mainCamera.transform.forward;
            forwardDirection.y = 0f;
            forwardDirection.Normalize();
        }
        else
        {
            forwardDirection = playerTransform.forward;
        }

        Vector3 dropPosition = playerTransform.position + forwardDirection * 2f + Vector3.up * 0.5f;
        inventory.DropItem(slotIndex, dropPosition);
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnSlotChanged -= OnSlotChanged;
        }
    }
}
