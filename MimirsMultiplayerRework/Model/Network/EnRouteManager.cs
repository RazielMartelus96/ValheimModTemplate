using System.Collections.Generic;
using JetBrains.Annotations;
using MimirMultiplayerRework.Config;

namespace MimirMultiplayerRework.Model.Network;


public class EnRouteManager
{
    [CanBeNull] static EnRouteManager _instance;
    
    EnRouteManager() { }
    public static EnRouteManager Instance
    {
        get { return _instance ??= new EnRouteManager(); }
    }

    public readonly Dictionary<int, string> NearbyRPCMethodByHashCode = [];
    public readonly HashSet<int> NearbyRPCMethodHashCodes = [];
    public long NetTimeTicks
    {
        get; set;
    }

    public void SetupNearbyRpcMethods()
    {
        NearbyRPCMethodHashCodes.Clear();
        var names = ConfigHandler.Instance.NearbyRPCMethodNames.Value.Split([','], System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var name in names) {
            NearbyRPCMethodByHashCode[name.GetStableHashCode()] = name;
        }

        NearbyRPCMethodHashCodes.Clear();
        NearbyRPCMethodHashCodes.UnionWith(NearbyRPCMethodByHashCode.Keys);

        Plugin.Logger.LogInfo($"NearbyRPCMethods set to: {string.Join(", ", NearbyRPCMethodByHashCode.Values)}");
    }
}


