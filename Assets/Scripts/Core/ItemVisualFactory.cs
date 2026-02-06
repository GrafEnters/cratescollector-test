using UnityEngine;

public class ItemVisualFactory : MonoBehaviour, IItemFactory {
    private IConfigProvider _configProvider;
    private ItemPool _itemPool;

    private void Awake() {
        if (!DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider)) {
            Debug.LogError("IConfigProvider not found in DI container");
        }
    }

    private void Start() {
        DIContainer.Instance.TryGet<ItemPool>(out _itemPool);
    }

    public GameObject CreateItem(ItemData itemData, Vector3 position) {
        if (_itemPool == null) {
            DIContainer.Instance.TryGet<ItemPool>(out _itemPool);
        }

        GameObject itemObject = _itemPool != null ? _itemPool.Get() : CreateItemFallback(itemData, position);
        itemObject.transform.position = position;
        itemObject.name = itemData.Name;

        CollectableItem collectable = itemObject.GetComponent<CollectableItem>();
        collectable.SetItemData(itemData);

        return itemObject;
    }

    private GameObject CreateItemFallback(ItemData itemData, Vector3 position) {
        GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        itemObject.transform.position = position;

        MainGameConfig config = _configProvider?.GetConfig();
        float itemScale = config != null ? config.ItemScale : 0.5f;
        itemObject.transform.localScale = Vector3.one * itemScale;
        itemObject.name = itemData.Name;

        Collider collider = itemObject.GetComponent<Collider>();
        collider.isTrigger = true;

        CollectableItem collectable = itemObject.AddComponent<CollectableItem>();
        collectable.SetItemData(itemData);

        itemObject.AddComponent<ItemOutline>();

        return itemObject;
    }
}
