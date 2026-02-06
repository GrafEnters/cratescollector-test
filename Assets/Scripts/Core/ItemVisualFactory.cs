using UnityEngine;

public class ItemVisualFactory : MonoBehaviour, IItemFactory {
    private IConfigProvider _configProvider;
    private ItemPool _itemPool;

    private void Awake() {
        DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider);
    }

    private void Start() {
        DIContainer.Instance.TryGet<ItemPool>(out _itemPool);
    }

    public GameObject CreateItem(ItemData itemData, Vector3 position) {
        if (_itemPool == null) {
            DIContainer.Instance.TryGet<ItemPool>(out _itemPool);
        }

        GameObject itemObject = _itemPool?.Get() ?? CreateItemFallback(itemData, position);
        itemObject.transform.position = position;
        itemObject.name = itemData.Name;
        itemObject.GetComponent<CollectableItem>().SetItemData(itemData);
        return itemObject;
    }

    private GameObject CreateItemFallback(ItemData itemData, Vector3 position) {
        GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        itemObject.transform.position = position;
        itemObject.transform.localScale = Vector3.one * (_configProvider?.GetConfig()?.ItemScale ?? 0.5f);
        itemObject.name = itemData.Name;
        itemObject.GetComponent<Collider>().isTrigger = true;
        itemObject.AddComponent<CollectableItem>().SetItemData(itemData);
        itemObject.AddComponent<ItemOutline>();
        return itemObject;
    }
}
