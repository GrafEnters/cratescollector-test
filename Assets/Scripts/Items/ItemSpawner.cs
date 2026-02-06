using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private int itemCount = 6;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float minDistance = 2f;

    private List<ItemData> itemsData = new List<ItemData>();

    private void Start()
    {
        LoadItemsData();
        SpawnItems();
    }

    private void LoadItemsData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("ItemsData");
        if (jsonFile == null)
        {
            Debug.LogError("ItemsData.json not found in Resources folder!");
            return;
        }

        ItemsDataContainer container = JsonUtility.FromJson<ItemsDataContainer>(jsonFile.text);
        if (container == null || container.items == null)
        {
            Debug.LogError("Failed to parse ItemsData.json!");
            return;
        }

        foreach (var itemJson in container.items)
        {
            itemsData.Add(itemJson.ToItemData());
        }
    }

    private void SpawnItems()
    {
        if (itemsData.Count == 0)
        {
            Debug.LogError("No items data loaded!");
            return;
        }

        List<Vector3> spawnedPositions = new List<Vector3>();
        int spawnedCount = 0;

        while (spawnedCount < itemCount && spawnedCount < itemsData.Count)
        {
            Vector3 position = GetRandomPosition(spawnedPositions);
            if (position != Vector3.zero)
            {
                ItemData itemData = itemsData[spawnedCount % itemsData.Count];
                SpawnItem(itemData, position);
                spawnedPositions.Add(position);
                spawnedCount++;
            }
            else
            {
                break;
            }
        }
    }

    private Vector3 GetRandomPosition(List<Vector3> existingPositions)
    {
        int attempts = 50;
        for (int i = 0; i < attempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 position = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);

            bool tooClose = false;
            foreach (Vector3 existingPos in existingPositions)
            {
                if (Vector3.Distance(position, existingPos) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                return position;
            }
        }

        return Vector3.zero;
    }

    private void SpawnItem(ItemData itemData, Vector3 position)
    {
        GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        itemObject.transform.position = position;
        itemObject.transform.localScale = Vector3.one * 0.5f;
        itemObject.name = itemData.name;

        MeshRenderer renderer = itemObject.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = itemData.color;
        renderer.material = mat;

        Collider collider = itemObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        CollectableItem collectable = itemObject.AddComponent<CollectableItem>();
        collectable.SetItemData(itemData);
    }

    public void SpawnItemAtRandomPosition(ItemData itemData)
    {
        List<Vector3> existingPositions = GetAllItemPositions();
        Vector3 position = GetRandomPosition(existingPositions);
        if (position != Vector3.zero)
        {
            SpawnItem(itemData, position);
        }
    }

    private List<Vector3> GetAllItemPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        CollectableItem[] items = FindObjectsOfType<CollectableItem>();
        foreach (CollectableItem item in items)
        {
            positions.Add(item.transform.position);
        }
        return positions;
    }
}
