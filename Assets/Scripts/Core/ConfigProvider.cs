using UnityEngine;

public class ConfigProvider : MonoBehaviour, IConfigProvider {
    [SerializeField]
    private MainGameConfig _config;

    public MainGameConfig GetConfig() {
        if (_config == null) {
            _config = ConfigManager.Config;
        }

        return _config;
    }
}
