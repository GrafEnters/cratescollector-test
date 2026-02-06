using UnityEngine;
using UnityEngine.Rendering;

public class OutlineRenderer : MonoBehaviour {
    private Camera _mainCamera;
    private RenderTexture _outlineTexture;
    private Material _outlineMaterial;
    private Material _edgeDetectionMaterial;
    private CommandBuffer _commandBuffer;
    private IConfigProvider _configProvider;
    private Color _cachedOutlineColor;
    private float _cachedOutlineWidth;

    private void Awake() {
        if (!DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider)) {
            Debug.LogError("IConfigProvider not found in DI container");
        }
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
            CacheOutlineProperties();
            UpdateOutlineProperties();
        }

        SetupCommandBuffer();
    }

    private void CacheOutlineProperties() {
        if (_configProvider == null) {
            return;
        }

        MainGameConfig config = _configProvider.GetConfig();
        if (config != null) {
            _cachedOutlineColor = config.OutlineColor;
            _cachedOutlineWidth = config.OutlineWidth;
        }
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

        var outlinedItems = ItemOutline.GetAllOutlines();
        foreach (ItemOutline item in outlinedItems) {
            if (item != null && item.IsOutlined()) {
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

        _edgeDetectionMaterial.SetColor("_OutlineColor", _cachedOutlineColor);
        _edgeDetectionMaterial.SetFloat("_OutlineWidth", _cachedOutlineWidth);
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