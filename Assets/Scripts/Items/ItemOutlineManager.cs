using UnityEngine;

public class ItemOutlineManager : MonoBehaviour, IItemOutlineManager {
    public void ShowOutline(CollectableItem item) {
        if (item == null) return;
        (item.GetComponent<ItemOutline>() ?? item.gameObject.AddComponent<ItemOutline>()).ShowOutline();
    }

    public void HideOutline(CollectableItem item) {
        item?.GetComponent<ItemOutline>()?.HideOutline();
    }
}
