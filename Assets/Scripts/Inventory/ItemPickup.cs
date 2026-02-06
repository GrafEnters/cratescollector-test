using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private LayerMask itemLayer = -1;

    private InputAction interactAction;
    private CollectableItem nearbyItem;
    private VisualElement hintElement;
    private Label hintLabel;
    private Inventory inventory;
    private UIDocument uiDocument;
    private ItemSpawner itemSpawner;
    private InventoryFullNotification notification;
    private bool uiReady = false;

    private void Awake()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            inventory = FindObjectOfType<Inventory>();
        }

        itemSpawner = FindObjectOfType<ItemSpawner>();

        notification = GetComponent<InventoryFullNotification>();
        if (notification == null)
        {
            GameObject notificationObject = new GameObject("InventoryFullNotification");
            notificationObject.transform.SetParent(transform);
            notification = notificationObject.AddComponent<InventoryFullNotification>();
        }

        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
    }

    private void Start()
    {
        StartCoroutine(SetupUICoroutine());
    }

    private IEnumerator SetupUICoroutine()
    {
        yield return null;
        yield return null;

        UIDocument[] allUIDocuments = FindObjectsOfType<UIDocument>();
        foreach (UIDocument doc in allUIDocuments)
        {
            if (doc.rootVisualElement != null)
            {
                uiDocument = doc;
                break;
            }
        }

        if (uiDocument == null)
        {
            GameObject uiObject = new GameObject("PickupHintUI");
            uiDocument = uiObject.AddComponent<UIDocument>();
            yield return null;
            yield return null;
        }

        int attempts = 0;
        while (uiDocument.rootVisualElement == null && attempts < 10)
        {
            yield return null;
            attempts++;
        }

        if (uiDocument.rootVisualElement == null)
        {
            yield break;
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
            hintElement.style.backgroundColor = new Color(0, 0, 0, 0.9f);
            hintElement.style.display = DisplayStyle.None;
            hintElement.style.borderTopWidth = 2;
            hintElement.style.borderBottomWidth = 2;
            hintElement.style.borderLeftWidth = 2;
            hintElement.style.borderRightWidth = 2;
            hintElement.style.borderTopColor = new Color(1, 1, 1, 0.8f);
            hintElement.style.borderBottomColor = new Color(1, 1, 1, 0.8f);
            hintElement.style.borderLeftColor = new Color(1, 1, 1, 0.8f);
            hintElement.style.borderRightColor = new Color(1, 1, 1, 0.8f);
            hintElement.style.borderTopLeftRadius = 5;
            hintElement.style.borderTopRightRadius = 5;
            hintElement.style.borderBottomLeftRadius = 5;
            hintElement.style.borderBottomRightRadius = 5;

            hintLabel = new Label("Нажмите E");
            hintLabel.style.fontSize = 24;
            hintLabel.style.color = Color.white;
            hintLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            hintLabel.style.width = Length.Percent(100);
            hintLabel.style.height = Length.Percent(100);
            hintLabel.style.marginTop = 0;
            hintLabel.style.marginBottom = 0;
            hintLabel.style.marginLeft = 0;
            hintLabel.style.marginRight = 0;

            hintElement.Add(hintLabel);
            root.Add(hintElement);
        }
        else
        {
            hintLabel = hintElement.Q<Label>();
            if (hintLabel == null)
            {
                hintLabel = new Label("Нажмите E");
                hintLabel.style.fontSize = 24;
                hintLabel.style.color = Color.white;
                hintLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                hintLabel.style.width = Length.Percent(100);
                hintLabel.style.height = Length.Percent(100);
                hintElement.Add(hintLabel);
            }
        }

        uiReady = true;
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
        if (!uiReady) return;

        CheckForNearbyItems();
        UpdateHint();
    }

    private void CheckForNearbyItems()
    {
        float pickupDistance = ConfigManager.Config != null ? ConfigManager.Config.pickupDistance : 2f;
        
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

        CollectableItem newNearbyItem = closestItem;
        
        if (newNearbyItem != nearbyItem)
        {
            if (nearbyItem != null)
            {
                ItemOutline outline = nearbyItem.GetComponent<ItemOutline>();
                if (outline != null)
                {
                    outline.HideOutline();
                }
            }
            
            if (newNearbyItem != null)
            {
                ItemOutline outline = newNearbyItem.GetComponent<ItemOutline>();
                if (outline == null)
                {
                    outline = newNearbyItem.gameObject.AddComponent<ItemOutline>();
                }
                outline.ShowOutline();
            }
            
            nearbyItem = newNearbyItem;
        }
    }

    private void UpdateHint()
    {
        if (hintElement == null || !uiReady || uiDocument == null) return;

        bool isInventoryBlocking = ConfigManager.Config != null && ConfigManager.Config.IsInventoryBlockingView;
        if (isInventoryBlocking)
        {
            InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
            if (inventoryUI != null && inventoryUI.IsOpen())
            {
                hintElement.style.display = DisplayStyle.None;
                return;
            }
        }

        if (nearbyItem != null && Camera.main != null)
        {
            float hintHeight = ConfigManager.Config != null ? ConfigManager.Config.pickupHintHeight : 0.75f;
            Vector3 worldPosition = nearbyItem.transform.position + Vector3.up * hintHeight;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            
            if (screenPosition.z > 0)
            {
                VisualElement root = uiDocument.rootVisualElement;
                
                float elementWidth = hintElement.resolvedStyle.width;
                float elementHeight = hintElement.resolvedStyle.height;
                
                if (elementWidth == 0) elementWidth = 200;
                if (elementHeight == 0) elementHeight = 50;
                
                float panelWidth = root.resolvedStyle.width;
                float panelHeight = root.resolvedStyle.height;
                
                if (panelWidth <= 0) panelWidth = Screen.width;
                if (panelHeight <= 0) panelHeight = Screen.height;
                
                float scaleX = panelWidth / Screen.width;
                float scaleY = panelHeight / Screen.height;
                
                float screenX = screenPosition.x;
                float screenY = Screen.height - screenPosition.y;
                
                float x = screenX * scaleX - elementWidth * 0.5f;
                float y = screenY * scaleY - elementHeight;
                
                hintElement.style.display = DisplayStyle.Flex;
                hintElement.style.left = x;
                hintElement.style.top = y;
            }
            else
            {
                hintElement.style.display = DisplayStyle.None;
            }
        }
        else
        {
            hintElement.style.display = DisplayStyle.None;
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
                    if (nearbyItem != null)
                    {
                        ItemOutline outline = nearbyItem.GetComponent<ItemOutline>();
                        if (outline != null)
                        {
                            outline.HideOutline();
                        }
                    }
                    
                    nearbyItem.Pickup();
                    
                    if (itemSpawner != null)
                    {
                        itemSpawner.SpawnItemAtRandomPosition(itemData);
                    }
                    
                    nearbyItem = null;
                }
                else
                {
                    if (notification != null)
                    {
                        notification.Show();
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        float pickupDistance = ConfigManager.Config != null ? ConfigManager.Config.pickupDistance : 2f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}
