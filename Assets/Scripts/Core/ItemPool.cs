using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-999)]
public class ItemPool : MonoBehaviour {
    private readonly Queue<GameObject> _pool = new();
    private ConfigProvider _configProvider;
    private Transform _poolParent;
    private int _poolSize;

    private void Awake() {
        _configProvider = DIContainer.Instance.Get<IConfigProvider>() as ConfigProvider;
        MainGameConfig config = _configProvider.GetConfig();
        _poolSize = config.ItemPoolSize;

        GameObject poolParentObject = new("ItemPool");
        _poolParent = poolParentObject.transform;
        _poolParent.SetParent(transform);
        _poolParent.gameObject.SetActive(false);

        InitializePool();
    }

    private void InitializePool() {
        MainGameConfig config = _configProvider.GetConfig();
        float itemScale = config.ItemScale;

        for (int i = 0; i < _poolSize; i++) {
            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.transform.localScale = Vector3.one * itemScale;
            itemObject.transform.SetParent(_poolParent);
            itemObject.SetActive(false);

            Collider collider = itemObject.GetComponent<Collider>();
            collider.isTrigger = true;

            itemObject.AddComponent<CollectableItem>();
            itemObject.AddComponent<ItemOutline>();

            _pool.Enqueue(itemObject);
        }
    }

    public GameObject Get() {
        GameObject itemObject;
        if (_pool.Count > 0) {
            itemObject = _pool.Dequeue();
        } else {
            MainGameConfig config = _configProvider.GetConfig();
            float itemScale = config.ItemScale;

            itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.transform.localScale = Vector3.one * itemScale;

            Collider collider = itemObject.GetComponent<Collider>();
            collider.isTrigger = true;

            itemObject.AddComponent<CollectableItem>();
            itemObject.AddComponent<ItemOutline>();
        }

        itemObject.SetActive(true);
        itemObject.transform.SetParent(null);
        return itemObject;
    }

    public void Return(GameObject itemObject) {
        if (itemObject == null) {
            return;
        }

        itemObject.SetActive(false);
        itemObject.transform.SetParent(_poolParent);
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.localRotation = Quaternion.identity;
        itemObject.name = "PooledItem";

        CollectableItem collectable = itemObject.GetComponent<CollectableItem>();
        if (collectable != null) {
            collectable.SetItemData(null);
        }

        ItemOutline outline = itemObject.GetComponent<ItemOutline>();
        if (outline != null) {
            outline.HideOutline();
        }

        if (_pool.Count < _poolSize) {
            _pool.Enqueue(itemObject);
        } else {
            Destroy(itemObject);
        }
    }
}
