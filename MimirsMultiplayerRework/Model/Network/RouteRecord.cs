using System.Collections.Generic;

namespace MimirMultiplayerRework.Model.Network;

public sealed class RouteRecord
{
    public readonly ZNetPeer NetPeer;
    public readonly long UserId;
    public readonly HashSet<long> NearbyUserIds = [];
    public Vector2i Sector = Vector2i.zero;

    public RouteRecord(ZNetPeer netPeer) {
        NetPeer = netPeer;
        UserId = netPeer.m_uid;
    }
}
