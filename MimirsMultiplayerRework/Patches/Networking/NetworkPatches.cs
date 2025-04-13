namespace MimirMultiplayerRework.Patches.Networking;

public class NetworkPatches : IPatch
{

    public void InitPatch()
    {
        On.ZNet.SendPeriodicData += ZNetOnSendPeriodicData;
    }

    void ZNetOnSendPeriodicData(On.ZNet.orig_SendPeriodicData orig, ZNet self, float dt)
    {
        if (!SendPeriodEllapsed(self, dt))
            return;

        if (!self.IsServer())
        {
            SendPeersSyncedClientData(self);
            return;
        }
        SendServerSyncedData(self);
    }

    bool SendPeriodEllapsed(ZNet self, float dt)
    {
        self.m_periodicSendTimer += dt;
        if ((double) self.m_periodicSendTimer < 2.0)
            return false;
        self.m_periodicSendTimer = 0.0f;
        return true;
    }
    void SendServerSyncedData(ZNet self)
    {
        self.SendNetTime();
        self.SendPlayerList();
    }
    
    void SendPeersSyncedClientData(ZNet self)
    {
        var syncedDataPackage = WriteSyncPackage(self);
        foreach (ZNetPeer peer in self.m_peers)
        {
            if (peer.IsReady())
                peer.m_rpc.Invoke("ServerSyncedPlayerData", (object) new ZPackage(syncedDataPackage.GetArray()));

        }
    }

    ZPackage WriteSyncPackage(ZNet self)
    {
        ZPackage zpackage = new ZPackage();
        var serverSyncedData = self.m_serverSyncedPlayerData;
        
        zpackage.Write(self.m_referencePosition);
        zpackage.Write(self.m_publicReferencePosition);
        zpackage.Write(serverSyncedData.Count);
        foreach (var keyValuePair in serverSyncedData)
        {
            zpackage.Write(keyValuePair.Key);
            zpackage.Write(keyValuePair.Value);
        }
        return zpackage;
    }
    
}
