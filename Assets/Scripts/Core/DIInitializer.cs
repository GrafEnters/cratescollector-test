using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class DIInitializer : MonoBehaviour {
    [SerializeField]
    private ConfigProvider _configProvider;

    [SerializeField]
    private ItemVisualFactory _itemFactory;

    [SerializeField]
    private InventoryStateProvider _inventoryStateProvider;

    [SerializeField]
    private ItemDetector _itemDetector;

    [SerializeField]
    private ItemOutlineManager _itemOutlineManager;

    [SerializeField]
    private ItemPool _itemPool;

    private void Awake() {
        DIContainer container = DIContainer.Instance;

        if (_configProvider != null) {
            container.Register<IConfigProvider>(_configProvider);
        }

        if (_itemFactory != null) {
            container.Register<IItemFactory>(_itemFactory);
        }

        if (_inventoryStateProvider != null) {
            container.Register<IInventoryStateProvider>(_inventoryStateProvider);
        }

        if (_itemDetector != null) {
            container.Register<IItemDetector>(_itemDetector);
        }

        if (_itemOutlineManager != null) {
            container.Register<IItemOutlineManager>(_itemOutlineManager);
        }

        if (_itemPool != null) {
            container.Register(_itemPool);
        }
    }
}