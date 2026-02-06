using UnityEngine;

public class ItemOutline : MonoBehaviour
{
    private bool isOutlined = false;
    
    public bool IsOutlined()
    {
        return isOutlined;
    }
    
    public void ShowOutline()
    {
        if (isOutlined) return;
        
        isOutlined = true;
        EnsureOutlineRenderer();
    }
    
    public void HideOutline()
    {
        if (!isOutlined) return;
        
        isOutlined = false;
    }
    
    private void EnsureOutlineRenderer()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null && mainCamera.GetComponent<OutlineRenderer>() == null)
        {
            mainCamera.gameObject.AddComponent<OutlineRenderer>();
        }
    }
    
    private void OnDestroy()
    {
        HideOutline();
    }
}
