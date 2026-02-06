using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour {
    private List<ItemData> itemsData = new();

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
        if (container == null || container.items == null) {
            Debug.LogError("Failed to parse ItemsData.json!");
            return;
        }

        foreach (ItemDataJson itemJson in container.items) {
            itemsData.Add(itemJson.ToItemData());
        }
    }

    private void SpawnItems() {
        if (itemsData.Count == 0) {
            Debug.LogError("No items data loaded!");
            return;
        }

        List<Vector3> spawnedPositions = new();
        int spawnedCount = 0;
        MainGameConfig config = ConfigManager.Config;
        int itemCount = config != null ? config.itemSpawnerItemCount : 6;

        while (spawnedCount < itemCount && spawnedCount < itemsData.Count) {
            Vector3 position = GetRandomPosition(spawnedPositions);
            if (position != Vector3.zero) {
                ItemData itemData = itemsData[spawnedCount % itemsData.Count];
                SpawnItem(itemData, position);
                spawnedPositions.Add(position);
                spawnedCount++;
            } else {
                break;
            }
        }
    }

    private Vector3 GetRandomPosition(List<Vector3> existingPositions, Vector3? playerPosition = null) {
        MainGameConfig config = ConfigManager.Config;
        float spawnRadius = config != null ? config.itemSpawnerRadius : 10f;
        float minDistance = config != null ? config.itemSpawnerMinDistance : 2f;
        float minDistanceFromPlayer = config != null ? config.itemSpawnerMinDistanceFromPlayer : 3f;
        float spawnHeight = config != null ? config.itemSpawnerHeight : 0.5f;
        int attempts = config != null ? config.itemSpawnerMaxAttempts : 50;

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
        GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        itemObject.transform.position = position;
        MainGameConfig config = ConfigManager.Config;
        float itemScale = config != null ? config.itemScale : 0.5f;
        itemObject.transform.localScale = Vector3.one * itemScale;
        itemObject.name = itemData.name;

        MeshRenderer renderer = itemObject.GetComponent<MeshRenderer>();
        Material mat = new(Shader.Find("Standard"));
        mat.color = itemData.color;
        renderer.material = mat;

        Collider collider = itemObject.GetComponent<Collider>();
        collider.isTrigger = true;

        CollectableItem collectable = itemObject.AddComponent<CollectableItem>();
        collectable.SetItemData(itemData);

        itemObject.AddComponent<ItemOutline>();
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