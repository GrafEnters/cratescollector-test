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
        container.Register<IConfigProvider>(_configProvider);
        container.Register<IItemFactory>(_itemFactory);
        container.Register<IInventoryStateProvider>(_inventoryStateProvider);
        container.Register<IItemDetector>(_itemDetector);
        container.Register<IItemOutlineManager>(_itemOutlineManager);
        container.Register(_itemPool);
    }
}