using UnityEngine;

public class ConfigManager : MonoBehaviour {
    private static MainGameConfig _config;
    private const string ConfigPath = "MainGameConfig";

    public static MainGameConfig Config {
        get {
            if (_config == null) {
                _config = Resources.Load<MainGameConfig>(ConfigPath);
                if (_config == null) {
                    Debug.LogError($"MainGameConfig not found at Resources/{ConfigPath}");
                }
            }

            return _config;
        }
    }

    private void Awake() {
        _ = Config;
    }
}