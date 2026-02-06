using UnityEngine;

public interface IItemDetector {
    CollectableItem FindNearestItem(Vector3 position, float distance, LayerMask layerMask);
}
