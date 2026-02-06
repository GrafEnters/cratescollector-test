using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryFullNotification : MonoBehaviour {
    private VisualElement _notificationElement;
    private Label _notificationLabel;
    private UIDocument _uiDocument;
    private Coroutine _hideCoroutine;
    private bool _uiReady;

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
            GameObject uiObject = new("NotificationUI");
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

        _notificationElement = root.Q<VisualElement>("InventoryFullNotification");
        if (_notificationElement == null) {
            _notificationElement = new VisualElement {
                name = "InventoryFullNotification",
                style = {
                    position = Position.Absolute,
                    width = 400,
                    height = 60,
                    backgroundColor = new Color(0.2f, 0.1f, 0.1f, 0.95f),
                    display = DisplayStyle.None,
                    borderTopWidth = 2,
                    borderBottomWidth = 2,
                    borderLeftWidth = 2,
                    borderRightWidth = 2,
                    borderTopColor = new Color(1f, 0.3f, 0.3f, 1f),
                    borderBottomColor = new Color(1f, 0.3f, 0.3f, 1f),
                    borderLeftColor = new Color(1f, 0.3f, 0.3f, 1f),
                    borderRightColor = new Color(1f, 0.3f, 0.3f, 1f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    top = 50,
                    left = 0
                }
            };

            _notificationLabel = new Label("Инвентарь заполнен") {
                style = {
                    fontSize = 28,
                    color = new Color(1f, 0.8f, 0.8f, 1f),
                    unityTextAlign = TextAnchor.MiddleCenter,
                    width = Length.Percent(100),
                    height = Length.Percent(100),
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0
                }
            };

            _notificationElement.Add(_notificationLabel);
            root.Add(_notificationElement);
        } else {
            _notificationLabel = _notificationElement.Q<Label>();
            if (_notificationLabel == null) {
                _notificationLabel = new Label("Инвентарь заполнен") {
                    style = {
                        fontSize = 28,
                        color = new Color(1f, 0.8f, 0.8f, 1f),
                        unityTextAlign = TextAnchor.MiddleCenter,
                        width = Length.Percent(100),
                        height = Length.Percent(100)
                    }
                };
                _notificationElement.Add(_notificationLabel);
            }
        }

        _uiReady = true;
    }

    public void Show() {
        if (!_uiReady || _notificationElement == null || _uiDocument == null || _uiDocument.rootVisualElement == null) {
            return;
        }

        if (_hideCoroutine != null) {
            StopCoroutine(_hideCoroutine);
        }

        VisualElement root = _uiDocument.rootVisualElement;
        float panelWidth = root.resolvedStyle.width;
        if (panelWidth <= 0) {
            panelWidth = Screen.width;
        }

        float elementWidth = _notificationElement.resolvedStyle.width;
        if (elementWidth == 0) {
            elementWidth = 400;
        }

        float x = (panelWidth - elementWidth) * 0.5f;
        _notificationElement.style.left = x;
        _notificationElement.style.top = 50;
        _notificationElement.style.display = DisplayStyle.Flex;
        _notificationElement.style.opacity = 0f;

        StartCoroutine(FadeIn());

        _hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator FadeIn() {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            _notificationElement.style.opacity = alpha;
            yield return null;
        }

        _notificationElement.style.opacity = 1f;
    }

    private IEnumerator HideAfterDelay() {
        yield return new WaitForSeconds(2.5f);

        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = _notificationElement.resolvedStyle.opacity;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            _notificationElement.style.opacity = alpha;
            yield return null;
        }

        _notificationElement.style.display = DisplayStyle.None;
        _notificationElement.style.opacity = 1f;
        _hideCoroutine = null;
    }
}