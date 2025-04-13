using BepInEx;
using BepInEx.Logging;
using MimirMultiplayerRework;
using MimirMultiplayerRework.Patches;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    PluginPatchManager _manager;
    
    void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        InitPatchManager();
    }

    /**
     * Initialises the Patch manager and it's patches for the plugin. 
     */
    void InitPatchManager()
    {
        Logger.LogInfo($"Initializing Patch Manager!");
        _manager = PluginPatchManager.Instance;
        AddPatches();
        _manager.InitPatches();
    }
    
    /**
     * <p>Logic for adding the appropriate <see cref="IPatch"> patches</see> to the Patch Manager.
     * </p>
     * <i>Developer Note: Allows for more complex logic in the future, such as certain patches not being executed
     * if certain conditions (such as config file settings) are not met. </i>
     * 
     */
    void AddPatches()
    {
        Logger.LogInfo($"Adding Patches!");
    
        Logger.LogInfo($"Patches added!");
    }
}