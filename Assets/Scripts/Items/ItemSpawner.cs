using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
    private readonly List<ItemData> _itemsData = new();
    private readonly List<CollectableItem> _activeItems = new();
    private IItemFactory _itemFactory;
    private IConfigProvider _configProvider;
    private Transform _playerTransform;

    private void Awake() {
        if (!DIContainer.Instance.TryGet<IItemFactory>(out _itemFactory)) {
            Debug.LogError("IItemFactory not found in DI container");
        }

        if (!DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider)) {
            Debug.LogError("IConfigProvider not found in DI container");
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            _playerTransform = player.transform;
        }
    }

    private void Start() {
        LoadItemsData();
        SpawnItems();
    }

    private void LoadItemsData() {
        TextAsset jsonFile = Resources.Load<TextAsset>("ItemsData");
        if (jsonFile == null) {
            Debug.LogError("ItemsData.json not found in Resources folder!");
            return;
        }

        ItemsDataContainer container = JsonUtility.FromJson<ItemsDataContainer>(jsonFile.text);
        if (container == null || container.Items == null) {
            Debug.LogError("Failed to parse ItemsData.json!");
            return;
        }

        foreach (ItemDataJson itemJson in container.Items) {
            _itemsData.Add(itemJson.ToItemData());
        }
    }

    private void SpawnItems() {
        if (_itemsData.Count == 0) {
            Debug.LogError("No items data loaded!");
            return;
        }

        if (_configProvider == null) {
            Debug.LogError("ConfigProvider is null");
            return;
        }

        List<Vector3> spawnedPositions = new();
        int spawnedCount = 0;
        MainGameConfig config = _configProvider.GetConfig();
        if (config == null) {
            Debug.LogError("MainGameConfig is null");
            return;
        }
        int itemCount = config.ItemSpawnerItemCount;

        while (spawnedCount < itemCount && spawnedCount < _itemsData.Count) {
            Vector3 position = GetRandomPosition(spawnedPositions);
            if (position != Vector3.zero) {
                ItemData itemData = _itemsData[spawnedCount % _itemsData.Count];
                SpawnItem(itemData, position);
                spawnedPositions.Add(position);
                spawnedCount++;
            } else {
                break;
            }
        }
    }

    private Vector3 GetRandomPosition(List<Vector3> existingPositions, Vector3? playerPosition = null) {
        if (_configProvider == null) {
            return Vector3.zero;
        }

        MainGameConfig config = _configProvider.GetConfig();
        if (config == null) {
            return Vector3.zero;
        }

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
        GameObject itemObject = _itemFactory.CreateItem(itemData, position);
        if (itemObject != null) {
            CollectableItem collectable = itemObject.GetComponent<CollectableItem>();
            if (collectable != null) {
                _activeItems.Add(collectable);
            }
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
        if (_playerTransform != null) {
            return _playerTransform.position;
        }

        return null;
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