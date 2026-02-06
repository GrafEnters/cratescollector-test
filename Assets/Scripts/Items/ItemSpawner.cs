using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour {
    private readonly List<ItemData> _itemsData = new();
    private ItemVisualFactory _itemFactory;
    private ConfigProvider _configProvider;

    private void Awake() {
        _itemFactory = DIContainer.Instance.Get<IItemFactory>() as ItemVisualFactory;
        _configProvider = DIContainer.Instance.Get<IConfigProvider>() as ConfigProvider;
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

        List<Vector3> spawnedPositions = new();
        int spawnedCount = 0;
        MainGameConfig config = _configProvider.GetConfig();
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
        MainGameConfig config = _configProvider.GetConfig();
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
        _itemFactory.CreateItem(itemData, position);
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
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) {
            return player.transform.position;
        }

        return null;
    }

    private List<Vector3> GetAllItemPositions() {
        List<Vector3> positions = new();
        CollectableItem[] items = FindObjectsOfType<CollectableItem>();
        foreach (CollectableItem item in items) {
            positions.Add(item.transform.position);
        }

        return positions;
    }
}