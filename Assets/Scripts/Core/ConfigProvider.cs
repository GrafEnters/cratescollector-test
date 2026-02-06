using UnityEngine;

public class ConfigProvider : MonoBehaviour, IConfigProvider {
    [SerializeField]
    private MainGameConfig _config;

    private const string ConfigPath = "MainGameConfig";

    public MainGameConfig GetConfig() {
        if (_config == null) {
            _config = Resources.Load<MainGameConfig>(ConfigPath);
            if (_config == null) {
                Debug.LogError($"MainGameConfig not found at Resources/{ConfigPath}");
            }
        }

        return _config;
    }
}
