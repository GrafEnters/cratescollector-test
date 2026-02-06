using UnityEngine;

public class CollectableItem : MonoBehaviour {
    private ItemData _itemData;
    private ItemPool _itemPool;
    private Material _cachedMaterial;

    private void Awake() {
        DIContainer.Instance.TryGet<ItemPool>(out _itemPool);
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
            if (_cachedMaterial == null) {
                _cachedMaterial = new Material(Shader.Find("Standard"));
                meshRenderer.material = _cachedMaterial;
            }

            _cachedMaterial.color = _itemData.Color;
        }
    }

    public void Pickup() {
        if (_itemPool != null) {
            _itemPool.Return(gameObject);
        } else {
            if (_cachedMaterial != null) {
                Destroy(_cachedMaterial);
                _cachedMaterial = null;
            }
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if (_cachedMaterial != null) {
            Destroy(_cachedMaterial);
            _cachedMaterial = null;
        }
    }
}