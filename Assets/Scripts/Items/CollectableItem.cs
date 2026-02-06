using UnityEngine;

public class CollectableItem : MonoBehaviour {
    private ItemData _itemData;
    private ItemPool _itemPool;

    private void Awake() {
        _itemPool = DIContainer.Instance.Get<ItemPool>();
    }

    public ItemData GetItemData() {
        return _itemData;
    }

    public void SetItemData(ItemData data) {
        _itemData = data;
        UpdateVisuals();
    }

    private void UpdateVisuals() {
        if (_itemData == null) {
            return;
        }

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) {
            if (meshRenderer.material == null) {
                Material mat = new(Shader.Find("Standard"));
                meshRenderer.material = mat;
            }

            meshRenderer.material.color = _itemData.Color;
        }
    }

    public void Pickup() {
        if (_itemPool != null) {
            _itemPool.Return(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}