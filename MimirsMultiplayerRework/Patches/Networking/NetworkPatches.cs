using System;
using MimirMultiplayerRework.Model.Network;

namespace MimirMultiplayerRework.Patches.Networking;

public class NetworkPatches : IPatch
{

    public void InitPatch()
    {
        On.ZDOMan.HandleDestroyedZDO += ZDOManOnHandleDestroyedZDO;
        On.ZNet.UpdateNetTime += ZNetOnUpdateNetTime;
        On.ZNet.UpdatePlayerList += ZNetOnUpdatePlayerList;
        On.ZRoutedRpc.AddPeer += ZRoutedRpcOnAddPeer;
        On.ZRoutedRpc.RemovePeer += ZRoutedRpcOnRemovePeer;
        On.ZRoutedRpc.RouteRPC += ZRoutedRpcOnRouteRPC;
    }

    void ZRoutedRpcOnRouteRPC(On.ZRoutedRpc.orig_RouteRPC orig, ZRoutedRpc self, ZRoutedRpc.RoutedRPCData rpcData)
    {
        if (self.m_server)
        {
            RouteManager.Instance.RouteRPC(self, rpcData);
            return;
        }
        orig(self, rpcData);
    }
    
    void ZRoutedRpcOnRemovePeer(On.ZRoutedRpc.orig_RemovePeer orig, ZRoutedRpc self, ZNetPeer peer)
    {
        RouteManager.Instance.OnRemovePeer(peer);
        orig(self, peer);
    }

    void ZRoutedRpcOnAddPeer(On.ZRoutedRpc.orig_AddPeer orig, ZRoutedRpc self, ZNetPeer peer)
    {
        RouteManager.Instance.OnAddPeer(peer);
        orig(self, peer);
    }

    void ZNetOnUpdatePlayerList(On.ZNet.orig_UpdatePlayerList orig, ZNet self)
    {
        self.UpdatePlayerList();
        RouteManager.Instance.RefreshRouteRecords();
    }

    void ZNetOnUpdateNetTime(On.ZNet.orig_UpdateNetTime orig, ZNet self, float dt)
    {
        orig(self, dt);
        EnRouteManager.Instance.NetTimeTicks = (long) self.m_netTime * TimeSpan.TicksPerSecond;    }

    void ZDOManOnHandleDestroyedZDO(On.ZDOMan.orig_HandleDestroyedZDO orig, ZDOMan self, ZDOID uid)
    {
        
        if (uid == ZDOID.None) {
            return;
        }

        if (uid.UserID == self.m_sessionID && uid.ID >= self.m_nextUid) {
            self.m_nextUid = uid.ID + 1U;
        }

        if (!self.m_objectsByID.TryGetValue(uid, out ZDO zdo) || zdo == null) {
            return;
        }

        self.m_onZDODestroyed(zdo);

        self.RemoveFromSector(zdo, zdo.GetSector());
        self.m_objectsByID.Remove(zdo.m_uid);

        if (Game.instance.PortalPrefabHash.Contains(zdo.m_prefab)) {
            self.m_portalObjects.Remove(zdo);
        }

        ZDOPool.Release(zdo);

        foreach (ZDOMan.ZDOPeer zdoPeer in self.m_peers) {
            if (zdoPeer.m_zdos.Remove(uid)) {
                // TODO: need to route this RPC for any matching NetPeers.
            }
        }

        if (ZNet.m_isServer) {
            self.m_deadZDOs[uid] = EnRouteManager.Instance.NetTimeTicks;
        }

    }
}



