using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-999)]
public class ItemPool : MonoBehaviour {
    private readonly Queue<GameObject> _pool = new();
    private IConfigProvider _configProvider;
    private Transform _poolParent;
    private int _poolSize;

    private void Awake() {
        DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider);
        _poolSize = _configProvider?.GetConfig()?.ItemPoolSize ?? 10;

        GameObject poolParentObject = new("ItemPool");
        _poolParent = poolParentObject.transform;
        _poolParent.SetParent(transform);
        _poolParent.gameObject.SetActive(false);

        InitializePool();
    }

    private void InitializePool() {
        for (int i = 0; i < _poolSize; i++) {
            GameObject itemObject = CreateItemObject();
            itemObject.transform.SetParent(_poolParent);
            itemObject.SetActive(false);
            _pool.Enqueue(itemObject);
        }
    }

    public GameObject Get() {
        GameObject itemObject;
        if (_pool.Count > 0) {
            itemObject = _pool.Dequeue();
        } else {
            itemObject = CreateItemObject();
        }

        itemObject.SetActive(true);
        itemObject.transform.SetParent(null);
        return itemObject;
    }

    private GameObject CreateItemObject() {
        float itemScale = _configProvider?.GetConfig()?.ItemScale ?? 0.5f;

        GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        itemObject.transform.localScale = Vector3.one * itemScale;
        itemObject.GetComponent<Collider>().isTrigger = true;
        itemObject.AddComponent<CollectableItem>();
        itemObject.AddComponent<ItemOutline>();

        return itemObject;
    }

    public void Return(GameObject itemObject) {
        if (itemObject == null) return;

        itemObject.SetActive(false);
        itemObject.transform.SetParent(_poolParent);
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.localRotation = Quaternion.identity;
        itemObject.name = "PooledItem";

        itemObject.GetComponent<CollectableItem>()?.SetItemData(null);
        itemObject.GetComponent<ItemOutline>()?.HideOutline();

        if (_pool.Count < _poolSize) {
            _pool.Enqueue(itemObject);
        } else {
            Destroy(itemObject);
        }
    }
}
