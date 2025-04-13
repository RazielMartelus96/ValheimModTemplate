using BepInEx.Configuration;
using JetBrains.Annotations;

namespace MimirMultiplayerRework.Config;

public class ConfigHandler
{
    [CanBeNull] static ConfigHandler _instance;

    public ConfigEntry<string> NearbyRPCMethodNames
    {
        get; private set;
    }
    ConfigHandler() { }
    public static ConfigHandler Instance
    {
        get { return _instance ??= new ConfigHandler(); }
    }
}
