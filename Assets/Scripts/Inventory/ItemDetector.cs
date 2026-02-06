using UnityEngine;

public class ItemDetector : MonoBehaviour, IItemDetector {
    public CollectableItem FindNearestItem(Vector3 position, float distance, LayerMask layerMask) {
        Collider[] colliders = Physics.OverlapSphere(position, distance, layerMask.value != 0 ? layerMask.value : -1);

        CollectableItem closestItem = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders) {
            CollectableItem item = col.GetComponent<CollectableItem>();
            if (item != null) {
                float itemDistance = Vector3.Distance(position, item.transform.position);
                if (itemDistance < closestDistance) {
                    closestDistance = itemDistance;
                    closestItem = item;
                }
            }
        }

        return closestItem;
    }
}
