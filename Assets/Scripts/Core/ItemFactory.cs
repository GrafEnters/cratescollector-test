using UnityEngine;

public class ItemFactory {
    private readonly IItemFactory _visualFactory;

    public ItemFactory(IItemFactory visualFactory) {
        _visualFactory = visualFactory;
    }

    public GameObject CreateItem(ItemData itemData, Vector3 position) {
        return _visualFactory.CreateItem(itemData, position);
    }
}
