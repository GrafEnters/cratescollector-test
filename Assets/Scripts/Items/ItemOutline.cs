using UnityEngine;

public class ItemOutline : MonoBehaviour {
    private bool _isOutlined;
    private static System.Collections.Generic.List<ItemOutline> _allOutlines = new();

    public static System.Collections.Generic.IReadOnlyList<ItemOutline> GetAllOutlines() {
        return _allOutlines;
    }

    public bool IsOutlined() {
        return _isOutlined;
    }

    private void Awake() {
        if (!_allOutlines.Contains(this)) {
            _allOutlines.Add(this);
        }
    }

    public void ShowOutline() {
        if (_isOutlined) {
            return;
        }

        _isOutlined = true;
        EnsureOutlineRenderer();
    }

    public void HideOutline() {
        if (!_isOutlined) {
            return;
        }

        _isOutlined = false;
    }

    private void EnsureOutlineRenderer() {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.GetComponent<OutlineRenderer>() == null) {
            mainCamera.gameObject.AddComponent<OutlineRenderer>();
        }
    }

    private void OnDestroy() {
        HideOutline();
        _allOutlines.Remove(this);
    }
}