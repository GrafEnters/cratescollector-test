using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    private ItemData itemData;

    public ItemData GetItemData()
    {
        return itemData;
    }

    public void SetItemData(ItemData data)
    {
        itemData = data;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (itemData == null) return;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            if (renderer.material == null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                renderer.material = mat;
            }
            renderer.material.color = itemData.color;
        }
    }

    public void Pickup()
    {
        Destroy(gameObject);
    }
}
