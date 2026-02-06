using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class InventoryFullNotification : MonoBehaviour
{
    private VisualElement notificationElement;
    private Label notificationLabel;
    private UIDocument uiDocument;
    private Coroutine hideCoroutine;
    private bool uiReady = false;

    private void Start()
    {
        StartCoroutine(SetupUICoroutine());
    }

    private IEnumerator SetupUICoroutine()
    {
        yield return null;
        yield return null;

        UIDocument[] allUIDocuments = FindObjectsOfType<UIDocument>();
        foreach (UIDocument doc in allUIDocuments)
        {
            if (doc.rootVisualElement != null)
            {
                uiDocument = doc;
                break;
            }
        }

        if (uiDocument == null)
        {
            GameObject uiObject = new GameObject("NotificationUI");
            uiDocument = uiObject.AddComponent<UIDocument>();
            yield return null;
            yield return null;
        }

        int attempts = 0;
        while (uiDocument.rootVisualElement == null && attempts < 10)
        {
            yield return null;
            attempts++;
        }

        if (uiDocument.rootVisualElement == null)
        {
            yield break;
        }

        VisualElement root = uiDocument.rootVisualElement;

        notificationElement = root.Q<VisualElement>("InventoryFullNotification");
        if (notificationElement == null)
        {
            notificationElement = new VisualElement();
            notificationElement.name = "InventoryFullNotification";
            notificationElement.style.position = Position.Absolute;
            notificationElement.style.width = 400;
            notificationElement.style.height = 60;
            notificationElement.style.backgroundColor = new Color(0.2f, 0.1f, 0.1f, 0.95f);
            notificationElement.style.display = DisplayStyle.None;
            notificationElement.style.borderTopWidth = 2;
            notificationElement.style.borderBottomWidth = 2;
            notificationElement.style.borderLeftWidth = 2;
            notificationElement.style.borderRightWidth = 2;
            notificationElement.style.borderTopColor = new Color(1f, 0.3f, 0.3f, 1f);
            notificationElement.style.borderBottomColor = new Color(1f, 0.3f, 0.3f, 1f);
            notificationElement.style.borderLeftColor = new Color(1f, 0.3f, 0.3f, 1f);
            notificationElement.style.borderRightColor = new Color(1f, 0.3f, 0.3f, 1f);
            notificationElement.style.borderTopLeftRadius = 8;
            notificationElement.style.borderTopRightRadius = 8;
            notificationElement.style.borderBottomLeftRadius = 8;
            notificationElement.style.borderBottomRightRadius = 8;
            notificationElement.style.top = 50;
            notificationElement.style.left = 0;

            notificationLabel = new Label("Инвентарь заполнен");
            notificationLabel.style.fontSize = 28;
            notificationLabel.style.color = new Color(1f, 0.8f, 0.8f, 1f);
            notificationLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            notificationLabel.style.width = Length.Percent(100);
            notificationLabel.style.height = Length.Percent(100);
            notificationLabel.style.marginTop = 0;
            notificationLabel.style.marginBottom = 0;
            notificationLabel.style.marginLeft = 0;
            notificationLabel.style.marginRight = 0;

            notificationElement.Add(notificationLabel);
            root.Add(notificationElement);
        }
        else
        {
            notificationLabel = notificationElement.Q<Label>();
            if (notificationLabel == null)
            {
                notificationLabel = new Label("Инвентарь заполнен");
                notificationLabel.style.fontSize = 28;
                notificationLabel.style.color = new Color(1f, 0.8f, 0.8f, 1f);
                notificationLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                notificationLabel.style.width = Length.Percent(100);
                notificationLabel.style.height = Length.Percent(100);
                notificationElement.Add(notificationLabel);
            }
        }

        uiReady = true;
    }

    public void Show()
    {
        if (!uiReady || notificationElement == null || uiDocument == null || uiDocument.rootVisualElement == null) return;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        VisualElement root = uiDocument.rootVisualElement;
        float panelWidth = root.resolvedStyle.width;
        if (panelWidth <= 0) panelWidth = Screen.width;

        float elementWidth = notificationElement.resolvedStyle.width;
        if (elementWidth == 0) elementWidth = 400;

        float x = (panelWidth - elementWidth) * 0.5f;
        notificationElement.style.left = x;
        notificationElement.style.top = 50;
        notificationElement.style.display = DisplayStyle.Flex;
        notificationElement.style.opacity = 0f;

        StartCoroutine(FadeIn());

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator FadeIn()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            notificationElement.style.opacity = alpha;
            yield return null;
        }

        notificationElement.style.opacity = 1f;
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(2.5f);

        float duration = 0.3f;
        float elapsed = 0f;
        float startAlpha = notificationElement.resolvedStyle.opacity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            notificationElement.style.opacity = alpha;
            yield return null;
        }

        notificationElement.style.display = DisplayStyle.None;
        notificationElement.style.opacity = 1f;
        hideCoroutine = null;
    }
}
