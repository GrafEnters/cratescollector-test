using UnityEngine;

public class ItemOutlineManager : MonoBehaviour, IItemOutlineManager {
    public void ShowOutline(CollectableItem item) {
        if (item == null) {
            return;
        }

        ItemOutline outline = item.GetComponent<ItemOutline>();
        if (outline == null) {
            outline = item.gameObject.AddComponent<ItemOutline>();
        }

        outline.ShowOutline();
    }

    public void HideOutline(CollectableItem item) {
        if (item == null) {
            return;
        }

        ItemOutline outline = item.GetComponent<ItemOutline>();
        if (outline != null) {
            outline.HideOutline();
        }
    }
}
