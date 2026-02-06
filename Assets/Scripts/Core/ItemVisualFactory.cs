using UnityEngine;

public class ItemVisualFactory : MonoBehaviour, IItemFactory {
    private ConfigProvider _configProvider;

    private void Awake() {
        _configProvider = DIContainer.Instance.Get<IConfigProvider>() as ConfigProvider;
    }

    public GameObject CreateItem(ItemData itemData, Vector3 position) {
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
