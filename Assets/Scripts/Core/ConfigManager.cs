using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    private static MainGameConfig config;
    private const string CONFIG_PATH = "MainGameConfig";

    public static MainGameConfig Config
    {
        get
        {
            if (config == null)
            {
                config = Resources.Load<MainGameConfig>(CONFIG_PATH);
                if (config == null)
                {
                    Debug.LogError($"MainGameConfig not found at Resources/{CONFIG_PATH}");
                }
            }
            return config;
        }
    }

    private void Awake()
    {
        config = Resources.Load<MainGameConfig>(CONFIG_PATH);
        if (config == null)
        {
            Debug.LogError($"MainGameConfig not found at Resources/{CONFIG_PATH}");
        }
    }
}
