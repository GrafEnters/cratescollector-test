using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private float pickupDistance = 2f;
    [SerializeField] private LayerMask itemLayer = -1;

    private InputAction interactAction;
    private CollectableItem nearbyItem;
    private VisualElement hintElement;
    private Label hintLabel;
    private Inventory inventory;
    private UIDocument uiDocument;
    private ItemSpawner itemSpawner;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
        }

        itemSpawner = FindObjectOfType<ItemSpawner>();

        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");

        SetupUI();
    }

    private void SetupUI()
    {
        uiDocument = FindObjectOfType<UIDocument>();
        if (uiDocument == null)
        {
            GameObject uiObject = new GameObject("UI");
            uiDocument = uiObject.AddComponent<UIDocument>();
        }

        VisualElement root = uiDocument.rootVisualElement;

        hintElement = root.Q<VisualElement>("PickupHint");
        if (hintElement == null)
        {
            hintElement = new VisualElement();
            hintElement.name = "PickupHint";
            hintElement.style.position = Position.Absolute;
            hintElement.style.width = 200;
            hintElement.style.height = 50;
            hintElement.style.backgroundColor = new Color(0, 0, 0, 0.7f);
            hintElement.style.display = DisplayStyle.None;

            hintLabel = new Label("Нажмите E");
            hintLabel.style.fontSize = 24;
            hintLabel.style.color = Color.white;
            hintLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            hintLabel.style.width = Length.Percent(100);
            hintLabel.style.height = Length.Percent(100);

            hintElement.Add(hintLabel);
            root.Add(hintElement);
        }
        else
        {
            hintLabel = hintElement.Q<Label>();
        }
    }

    private void OnEnable()
    {
        interactAction?.Enable();
        interactAction.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.performed -= OnInteract;
        interactAction?.Disable();
    }

    private void Update()
    {
        CheckForNearbyItems();
        UpdateHint();
    }

    private void CheckForNearbyItems()
    {
        int layerMask = itemLayer.value;
        if (layerMask == 0)
        {
            layerMask = -1;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupDistance, layerMask);
        CollectableItem closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            CollectableItem item = col.GetComponent<CollectableItem>();
            if (item != null)
            {
                float distance = Vector3.Distance(transform.position, item.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = item;
                }
            }
        }

        nearbyItem = closestItem;
    }

    private void UpdateHint()
    {
        if (nearbyItem != null && hintElement != null)
        {
            hintElement.style.display = DisplayStyle.Flex;
            Vector3 worldPosition = nearbyItem.transform.position + Vector3.up * 1.5f;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            hintElement.style.left = screenPosition.x - 100;
            hintElement.style.top = Screen.height - screenPosition.y - 25;
        }
        else
        {
            if (hintElement != null)
            {
                hintElement.style.display = DisplayStyle.None;
            }
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (nearbyItem != null && inventory != null)
        {
            ItemData itemData = nearbyItem.GetItemData();
            if (itemData != null)
            {
                if (inventory.AddItem(itemData, 1))
                {
                    nearbyItem.Pickup();
                    
                    if (itemSpawner != null)
                    {
                        itemSpawner.SpawnItemAtRandomPosition(itemData);
                    }
                    
                    nearbyItem = null;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}
