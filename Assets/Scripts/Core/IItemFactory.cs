using UnityEngine;

public interface IItemFactory {
    GameObject CreateItem(ItemData itemData, Vector3 position);
}
