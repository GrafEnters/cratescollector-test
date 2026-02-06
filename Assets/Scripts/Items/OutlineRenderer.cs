using UnityEngine;
using UnityEngine.Rendering;

public class OutlineRenderer : MonoBehaviour
{
    private Camera mainCamera;
    private RenderTexture outlineTexture;
    private Material outlineMaterial;
    private Material edgeDetectionMaterial;
    private CommandBuffer commandBuffer;
    
    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        Shader outlineShader = Shader.Find("Custom/Outline");
        if (outlineShader != null)
        {
            outlineMaterial = new Material(outlineShader);
        }
        
        Shader edgeShader = Shader.Find("Custom/OutlinePostProcess");
        if (edgeShader != null)
        {
            edgeDetectionMaterial = new Material(edgeShader);
            UpdateOutlineProperties();
        }
        
        SetupCommandBuffer();
    }
    
    private void SetupCommandBuffer()
    {
        if (commandBuffer != null)
        {
            mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
            commandBuffer.Release();
        }
        
        commandBuffer = new CommandBuffer();
        commandBuffer.name = "Outline Render";
        
        if (outlineTexture != null)
        {
            outlineTexture.Release();
        }
        
        outlineTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        outlineTexture.name = "OutlineTexture";
        
        commandBuffer.SetRenderTarget(outlineTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);
        
        mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
    }
    
    private void UpdateCommandBuffer()
    {
        if (commandBuffer == null || outlineMaterial == null) return;
        
        commandBuffer.Clear();
        commandBuffer.SetRenderTarget(outlineTexture);
        commandBuffer.ClearRenderTarget(true, true, Color.clear);
        
        ItemOutline[] outlinedItems = FindObjectsOfType<ItemOutline>();
        foreach (ItemOutline item in outlinedItems)
        {
            if (item.IsOutlined())
            {
                MeshFilter filter = item.GetComponent<MeshFilter>();
                if (filter != null)
                {
                    commandBuffer.DrawMesh(filter.sharedMesh, item.transform.localToWorldMatrix, outlineMaterial, 0, 0);
                }
            }
        }
    }
    
    private void UpdateOutlineProperties()
    {
        if (edgeDetectionMaterial != null && ConfigManager.Config != null)
        {
            edgeDetectionMaterial.SetColor("_OutlineColor", ConfigManager.Config.outlineColor);
            edgeDetectionMaterial.SetFloat("_OutlineWidth", ConfigManager.Config.outlineWidth);
        }
    }
    
    private void OnPreRender()
    {
        if (outlineTexture == null || outlineTexture.width != Screen.width || outlineTexture.height != Screen.height)
        {
            SetupCommandBuffer();
        }
        UpdateCommandBuffer();
    }
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (edgeDetectionMaterial == null || outlineTexture == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        UpdateOutlineProperties();
        edgeDetectionMaterial.SetTexture("_OutlineTex", outlineTexture);
        Graphics.Blit(source, destination, edgeDetectionMaterial);
    }
    
    private void OnDestroy()
    {
        if (commandBuffer != null)
        {
            if (mainCamera != null)
            {
                mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
            }
            commandBuffer.Release();
        }
        
        if (outlineTexture != null)
        {
            outlineTexture.Release();
            Destroy(outlineTexture);
        }
    }
}
