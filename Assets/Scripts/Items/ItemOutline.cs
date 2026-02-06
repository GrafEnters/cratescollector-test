using UnityEngine;

public class ItemOutline : MonoBehaviour {
    private bool _isOutlined;

    public bool IsOutlined() {
        return _isOutlined;
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
        if (mainCamera == null) {
            mainCamera = FindAnyObjectByType<Camera>();
        }

        if (mainCamera != null && mainCamera.GetComponent<OutlineRenderer>() == null) {
            mainCamera.gameObject.AddComponent<OutlineRenderer>();
        }
    }

    private void OnDestroy() {
        HideOutline();
    }
}