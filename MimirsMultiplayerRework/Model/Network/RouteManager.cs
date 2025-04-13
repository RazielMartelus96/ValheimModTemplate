using System.Collections.Generic;
using MimirMultiplayerRework.Model.Extensions;

namespace MimirMultiplayerRework.Model.Network;

public class RouteManager
{
    static RouteManager _instance;
    public static RouteManager Instance => _instance ??= new RouteManager();
    private readonly int RoutedRPCHashCode = "RoutedRPC".GetStableHashCode();
    Dictionary<long, ZNetPeer> NetPeers = [];
    Dictionary<long, RouteRecord> NetPeerRouting = [];
    readonly ZPackage _package = new();
    
    public void OnAddPeer(ZNetPeer peer)
    {
        NetPeers.Add(peer.m_uid, peer);
        NetPeerRouting.Add(peer.m_uid, new RouteRecord(peer));
    }

    public void OnRemovePeer(ZNetPeer peer)
    {
        NetPeers.Remove(peer.m_uid);
        NetPeerRouting.Remove(peer.m_uid);
    }

    public void RefreshRouteRecords()
    {
        foreach (RouteRecord record in NetPeerRouting.Values) {
            record.Sector = ZoneSystem.instance.GetZone(record.NetPeer.m_refPos);
        }

        foreach (RouteRecord record in NetPeerRouting.Values) {
            RefreshRouteRecord(record);
        }
    }

    void RefreshRouteRecord(RouteRecord record)
    {
        record.NearbyUserIds.Clear();

        foreach (RouteRecord otherRecord in NetPeerRouting.Values) {
            if (otherRecord.UserId == record.UserId) {
                continue;
            }

            if (record.Sector.IsSectorInRange(otherRecord.Sector, 2)) {
                record.NearbyUserIds.Add(otherRecord.UserId);
            }
        }
    }

    public void RouteRPC(ZRoutedRpc routedRpc, ZRoutedRpc.RoutedRPCData rpcData)
    {
        if (rpcData.m_targetPeerID == ZRoutedRpc.Everybody) {
            RouteRPCToEverybody(routedRpc, rpcData);
        } else if (TryGetPeer(rpcData.m_targetPeerID, out ZNetPeer netPeer)) {
            SerializeRoutedRPCInvoke(rpcData, RoutedRPCHashCode, _package);
            SendPackage(netPeer.m_rpc, _package);
        }
    }

    void RouteRPCToEverybody(ZRoutedRpc routedRpc, ZRoutedRpc.RoutedRPCData rpcData)
    {
        if (EnRouteManager.Instance.NearbyRPCMethodHashCodes.Contains(rpcData.m_methodHash)
            && NetPeerRouting.TryGetValue(rpcData.m_senderPeerID, out RouteRecord record)) {
            RouteToNearby(rpcData, record);
        } else {
            RouteToPeers(routedRpc, rpcData, rpcData.m_senderPeerID);
        }
    }

    void RouteToNearby(ZRoutedRpc.RoutedRPCData rpcData, RouteRecord record)
    {
        if (record.NearbyUserIds.Count <= 0) {
            return;
        }

        SerializeRoutedRPCInvoke(rpcData, RoutedRPCHashCode, _package);

        foreach (long peerId in record.NearbyUserIds) {
            if (TryGetPeer(peerId, out ZNetPeer netPeer)) {
                SendPackage(netPeer.m_rpc, _package);
            }
        }
    }

    void RouteToPeers(ZRoutedRpc routedRpc, ZRoutedRpc.RoutedRPCData rpcData, long senderPeerId)
    {
        SerializeRoutedRPCInvoke(rpcData, RoutedRPCHashCode, _package);

        foreach (ZNetPeer netPeer in routedRpc.m_peers)
        {
            if (netPeer.m_uid != senderPeerId && netPeer.IsReady())
            {
                SendPackage(netPeer.m_rpc, _package);
            }
        }
    }
    
    void SerializeRoutedRPCInvoke(ZRoutedRpc.RoutedRPCData rpcData, int methodhash, ZPackage package) {
        package.Clear();

        package.Write(0);
        package.Write(0);

        int size = package.Size();
        rpcData.WriteToPackage(package);

        long position = package.m_stream.Position;
        package.m_stream.Position = 0;

        package.Write(methodhash);
        package.Write(package.Size() - size);

        package.m_stream.Position = position;
    }
    void SendPackage(ZRpc rpc, ZPackage package) {
        if (rpc.m_socket.IsConnected()) {
            rpc.m_sentPackages++;
            rpc.m_sentData += package.Size();
            rpc.m_socket.Send(package);
        }
    }

    bool TryGetPeer(long targetPeerId, out ZNetPeer netPeer) {
        if (NetPeers.TryGetValue(targetPeerId, out netPeer) && netPeer != null && netPeer.IsReady()) {
            return true;
        }

        netPeer = default;
        return false;
    }
}
