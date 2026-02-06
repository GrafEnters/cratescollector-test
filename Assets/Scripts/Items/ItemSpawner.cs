using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
    private readonly List<ItemData> _itemsData = new();
    private readonly List<CollectableItem> _activeItems = new();
    private IItemFactory _itemFactory;
    private IConfigProvider _configProvider;
    private Transform _playerTransform;

    private void Awake() {
        DIContainer.Instance.TryGet<IItemFactory>(out _itemFactory);
        DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider);
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start() {
        LoadItemsData();
        SpawnItems();
    }

    private void LoadItemsData() {
        TextAsset jsonFile = Resources.Load<TextAsset>("ItemsData");
        if (jsonFile == null) return;

        ItemsDataContainer container = JsonUtility.FromJson<ItemsDataContainer>(jsonFile.text);
        if (container?.Items == null) return;

        foreach (ItemDataJson itemJson in container.Items) {
            _itemsData.Add(itemJson.ToItemData());
        }
    }

    private void SpawnItems() {
        if (_itemsData.Count == 0 || _configProvider == null) return;

        MainGameConfig config = _configProvider.GetConfig();
        if (config == null) return;

        List<Vector3> spawnedPositions = new();
        int itemCount = config.ItemSpawnerItemCount;

        for (int spawnedCount = 0; spawnedCount < itemCount && spawnedCount < _itemsData.Count; spawnedCount++) {
            Vector3 position = GetRandomPosition(spawnedPositions);
            if (position == Vector3.zero) break;

            SpawnItem(_itemsData[spawnedCount % _itemsData.Count], position);
            spawnedPositions.Add(position);
        }
    }

    private Vector3 GetRandomPosition(List<Vector3> existingPositions, Vector3? playerPosition = null) {
        MainGameConfig config = _configProvider?.GetConfig();
        if (config == null) return Vector3.zero;

        float spawnRadius = config.ItemSpawnerRadius;
        float minDistance = config.ItemSpawnerMinDistance;
        float minDistanceFromPlayer = config.ItemSpawnerMinDistanceFromPlayer;
        float spawnHeight = config.ItemSpawnerHeight;
        int attempts = config.ItemSpawnerMaxAttempts;

        for (int i = 0; i < attempts; i++) {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 position = transform.position + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);

            bool tooClose = false;
            foreach (Vector3 existingPos in existingPositions) {
                if (Vector3.Distance(position, existingPos) < minDistance) {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose && playerPosition.HasValue) {
                Vector3 playerPos = playerPosition.Value;
                playerPos.y = position.y;
                if (Vector3.Distance(position, playerPos) < minDistanceFromPlayer) {
                    tooClose = true;
                }
            }

            if (!tooClose) {
                return position;
            }
        }

        return Vector3.zero;
    }

    private void SpawnItem(ItemData itemData, Vector3 position) {
        CollectableItem collectable = _itemFactory?.CreateItem(itemData, position)?.GetComponent<CollectableItem>();
        if (collectable != null) {
            _activeItems.Add(collectable);
        }
    }

    public void SpawnItemAtRandomPosition(ItemData itemData) {
        List<Vector3> existingPositions = GetAllItemPositions();
        Vector3? playerPosition = GetPlayerPosition();
        Vector3 position = GetRandomPosition(existingPositions, playerPosition);
        if (position != Vector3.zero) {
            SpawnItem(itemData, position);
        }
    }

    private Vector3? GetPlayerPosition() {
        return _playerTransform?.position;
    }

    private List<Vector3> GetAllItemPositions() {
        UpdateActiveItemsCache();
        List<Vector3> positions = new();
        foreach (CollectableItem item in _activeItems) {
            if (item != null && item.gameObject.activeInHierarchy) {
                positions.Add(item.transform.position);
            }
        }

        return positions;
    }

    private void UpdateActiveItemsCache() {
        _activeItems.RemoveAll(item => item == null || !item.gameObject.activeInHierarchy);
    }
}