using Photon;
using ExitGames.Client.Photon;

public static class KillReporter
{
    // killer: PhotonPlayer (가해자)
    public static void AddKill(PhotonPlayer killer)
    {
        if (!PhotonNetwork.isMasterClient || killer == null) return;

        var ht = killer.CustomProperties ?? new Hashtable();
        int cur = ht.ContainsKey("Kills") ? (int)ht["Kills"] : 0;
        ht["Kills"] = cur + 1;
        killer.SetCustomProperties(ht);
    }

    // 오프라인용: 로컬 변수
    public static int offlineKills;
    public static void AddKillOffline() => offlineKills++;
}
