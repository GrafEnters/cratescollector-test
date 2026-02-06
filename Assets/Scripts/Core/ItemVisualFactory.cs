using UnityEngine;

public class ItemVisualFactory : MonoBehaviour, IItemFactory {
    private ConfigProvider _configProvider;
    private ItemPool _itemPool;

    private void Awake() {
        _configProvider = DIContainer.Instance.Get<IConfigProvider>() as ConfigProvider;
    }

    private void Start() {
        _itemPool = DIContainer.Instance.Get<ItemPool>();
    }

    public GameObject CreateItem(ItemData itemData, Vector3 position) {
        if (_itemPool == null) {
            _itemPool = DIContainer.Instance.Get<ItemPool>();
        }

        GameObject itemObject = _itemPool != null ? _itemPool.Get() : CreateItemFallback(itemData, position);
        itemObject.transform.position = position;
        itemObject.name = itemData.Name;

        MeshRenderer renderer = itemObject.GetComponent<MeshRenderer>();
        if (renderer.material == null) {
            Material mat = new(Shader.Find("Standard")) {
                color = itemData.Color
            };
            renderer.material = mat;
        } else {
            renderer.material.color = itemData.Color;
        }

        CollectableItem collectable = itemObject.GetComponent<CollectableItem>();
        collectable.SetItemData(itemData);

        return itemObject;
    }

    private GameObject CreateItemFallback(ItemData itemData, Vector3 position) {
        GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        itemObject.transform.position = position;

        MainGameConfig config = _configProvider.GetConfig();
        float itemScale = config.ItemScale;
        itemObject.transform.localScale = Vector3.one * itemScale;
        itemObject.name = itemData.Name;

        MeshRenderer renderer = itemObject.GetComponent<MeshRenderer>();
        Material mat = new(Shader.Find("Standard")) {
            color = itemData.Color
        };
        renderer.material = mat;

        Collider collider = itemObject.GetComponent<Collider>();
        collider.isTrigger = true;

        CollectableItem collectable = itemObject.AddComponent<CollectableItem>();
        collectable.SetItemData(itemData);

        itemObject.AddComponent<ItemOutline>();

        return itemObject;
    }
}
