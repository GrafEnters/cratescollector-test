using UnityEngine;
using UnityEngine.Rendering;

public class OutlineRenderer : MonoBehaviour {
    private Camera _mainCamera;
    private RenderTexture _outlineTexture;
    private Material _outlineMaterial;
    private Material _edgeDetectionMaterial;
    private CommandBuffer _commandBuffer;
    private ConfigProvider _configProvider;

    private void Awake() {
        _configProvider = DIContainer.Instance.Get<IConfigProvider>() as ConfigProvider;
    }

    private void Start() {
        _mainCamera = GetComponent<Camera>();
        if (_mainCamera == null) {
            _mainCamera = Camera.main;
        }

        Shader outlineShader = Shader.Find("Custom/Outline");
        if (outlineShader != null) {
            _outlineMaterial = new Material(outlineShader);
        }

        Shader edgeShader = Shader.Find("Custom/OutlinePostProcess");
        if (edgeShader != null) {
            _edgeDetectionMaterial = new Material(edgeShader);
            UpdateOutlineProperties();
        }

        SetupCommandBuffer();
    }

    private void SetupCommandBuffer() {
        if (_commandBuffer != null) {
            _mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
            _commandBuffer.Release();
        }

        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "Outline Render";

        if (_outlineTexture != null) {
            _outlineTexture.Release();
        }

        _outlineTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32) {
            name = "OutlineTexture"
        };

        _commandBuffer.SetRenderTarget(_outlineTexture);
        _commandBuffer.ClearRenderTarget(true, true, Color.clear);

        _mainCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
    }

    private void UpdateCommandBuffer() {
        if (_commandBuffer == null || _outlineMaterial == null) {
            return;
        }

        _commandBuffer.Clear();
        _commandBuffer.SetRenderTarget(_outlineTexture);
        _commandBuffer.ClearRenderTarget(true, true, Color.clear);

        ItemOutline[] outlinedItems = FindObjectsOfType<ItemOutline>();
        foreach (ItemOutline item in outlinedItems) {
            if (item.IsOutlined()) {
                MeshFilter filter = item.GetComponent<MeshFilter>();
                if (filter != null) {
                    _commandBuffer.DrawMesh(filter.sharedMesh, item.transform.localToWorldMatrix, _outlineMaterial, 0, 0);
                }
            }
        }
    }

    private void UpdateOutlineProperties() {
        if (_edgeDetectionMaterial == null) {
            return;
        }

        MainGameConfig config = _configProvider.GetConfig();
        _edgeDetectionMaterial.SetColor("_OutlineColor", config.OutlineColor);
        _edgeDetectionMaterial.SetFloat("_OutlineWidth", config.OutlineWidth);
    }

    private void OnPreRender() {
        if (_outlineTexture == null || _outlineTexture.width != Screen.width || _outlineTexture.height != Screen.height) {
            SetupCommandBuffer();
        }

        UpdateCommandBuffer();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (_edgeDetectionMaterial == null || _outlineTexture == null) {
            Graphics.Blit(source, destination);
            return;
        }

        UpdateOutlineProperties();
        _edgeDetectionMaterial.SetTexture("_OutlineTex", _outlineTexture);
        Graphics.Blit(source, destination, _edgeDetectionMaterial);
    }

    private void OnDestroy() {
        if (_commandBuffer != null) {
            if (_mainCamera != null) {
                _mainCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, _commandBuffer);
            }

            _commandBuffer.Release();
        }

        if (_outlineTexture != null) {
            _outlineTexture.Release();
            Destroy(_outlineTexture);
        }
    }
}