using UnityEngine;

public class ConfigProvider : MonoBehaviour, IConfigProvider {
    [SerializeField]
    private MainGameConfig _config;

    private const string ConfigPath = "MainGameConfig";

    public MainGameConfig GetConfig() {
        if (_config == null) {
            _config = Resources.Load<MainGameConfig>(ConfigPath);
        }
        return _config;
    }
}
