using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemPickupHintUI : MonoBehaviour {
    private VisualElement _hintElement;
    private Label _hintLabel;
    private UIDocument _uiDocument;
    private bool _uiReady;
    private IConfigProvider _configProvider;
    private IInventoryStateProvider _inventoryStateProvider;
    private MainGameConfig _cachedConfig;
    private float _cachedHintHeight;
    private bool _cachedIsInventoryBlockingView;

    private void Awake() {
        if (!DIContainer.Instance.TryGet<IConfigProvider>(out _configProvider)) {
            Debug.LogError("IConfigProvider not found in DI container");
        }

        _inventoryStateProvider = DIContainer.Instance.Get<IInventoryStateProvider>();
    }

    private void Start() {
        StartCoroutine(SetupUICoroutine());
    }

    private IEnumerator SetupUICoroutine() {
        yield return null;
        yield return null;

        UIDocument[] allUIDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        foreach (UIDocument doc in allUIDocuments) {
            if (doc.rootVisualElement != null) {
                _uiDocument = doc;
                break;
            }
        }

        if (_uiDocument == null) {
            GameObject uiObject = new("PickupHintUI");
            _uiDocument = uiObject.AddComponent<UIDocument>();
            yield return null;
            yield return null;
        }

        int attempts = 0;
        while (_uiDocument.rootVisualElement == null && attempts < 10) {
            yield return null;
            attempts++;
        }

        if (_uiDocument.rootVisualElement == null) {
            yield break;
        }

        VisualElement root = _uiDocument.rootVisualElement;

        _hintElement = root.Q<VisualElement>("PickupHint");
        if (_hintElement == null) {
            _hintElement = new VisualElement {
                name = "PickupHint",
                style = {
                    position = Position.Absolute,
                    width = 200,
                    height = 50,
                    backgroundColor = new Color(0, 0, 0, 0.9f),
                    display = DisplayStyle.None,
                    borderTopWidth = 2,
                    borderBottomWidth = 2,
                    borderLeftWidth = 2,
                    borderRightWidth = 2,
                    borderTopColor = new Color(1, 1, 1, 0.8f),
                    borderBottomColor = new Color(1, 1, 1, 0.8f),
                    borderLeftColor = new Color(1, 1, 1, 0.8f),
                    borderRightColor = new Color(1, 1, 1, 0.8f),
                    borderTopLeftRadius = 5,
                    borderTopRightRadius = 5,
                    borderBottomLeftRadius = 5,
                    borderBottomRightRadius = 5
                }
            };

            _hintLabel = new Label("Нажмите E") {
                style = {
                    fontSize = 24,
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    width = Length.Percent(100),
                    height = Length.Percent(100),
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0
                }
            };

            _hintElement.Add(_hintLabel);
            root.Add(_hintElement);
        } else {
            _hintLabel = _hintElement.Q<Label>();
            if (_hintLabel == null) {
                _hintLabel = new Label("Нажмите E") {
                    style = {
                        fontSize = 24,
                        color = Color.white,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        width = Length.Percent(100),
                        height = Length.Percent(100)
                    }
                };
                _hintElement.Add(_hintLabel);
            }
        }

        _uiReady = true;

        _cachedConfig = _configProvider.GetConfig();
        if (_cachedConfig != null) {
            _cachedHintHeight = _cachedConfig.PickupHintHeight;
            _cachedIsInventoryBlockingView = _cachedConfig.IsInventoryBlockingView;
        }
    }

    public void UpdateHint(CollectableItem nearbyItem) {
        if (_hintElement == null || !_uiReady || _uiDocument == null) {
            return;
        }

        if (_cachedIsInventoryBlockingView) {
            if (_inventoryStateProvider != null && _inventoryStateProvider.IsInventoryOpen()) {
                _hintElement.style.display = DisplayStyle.None;
                return;
            }
        }

        if (nearbyItem != null && Camera.main != null) {
            Vector3 worldPosition = nearbyItem.transform.position + Vector3.up * _cachedHintHeight;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            if (screenPosition.z > 0) {
                VisualElement root = _uiDocument.rootVisualElement;

                float elementWidth = _hintElement.resolvedStyle.width;
                float elementHeight = _hintElement.resolvedStyle.height;

                if (elementWidth == 0) {
                    elementWidth = 200;
                }

                if (elementHeight == 0) {
                    elementHeight = 50;
                }

                float panelWidth = root.resolvedStyle.width;
                float panelHeight = root.resolvedStyle.height;

                if (panelWidth <= 0) {
                    panelWidth = Screen.width;
                }

                if (panelHeight <= 0) {
                    panelHeight = Screen.height;
                }

                float scaleX = panelWidth / Screen.width;
                float scaleY = panelHeight / Screen.height;

                float screenX = screenPosition.x;
                float screenY = Screen.height - screenPosition.y;

                float x = screenX * scaleX - elementWidth * 0.5f;
                float y = screenY * scaleY - elementHeight;

                _hintElement.style.display = DisplayStyle.Flex;
                _hintElement.style.left = x;
                _hintElement.style.top = y;
            } else {
                _hintElement.style.display = DisplayStyle.None;
            }
        } else {
            _hintElement.style.display = DisplayStyle.None;
        }
    }
}
