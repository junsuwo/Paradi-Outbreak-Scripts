using UnityEngine;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// PUN1 기준
public class PhotonUserSessionSync : Photon.MonoBehaviour
{
    // 방에 들어갔을 때 한 번
    public void OnJoinedRoom()
    {
        ApplyUserSessionToPhoton();
    }

    // 로비에 들어갔을 때도 한 번 (필요하면)
    public void OnJoinedLobby()
    {
        ApplyUserSessionToPhoton();
    }

    // 씬 시작될 때 한 번 더 (이미 접속돼 있으면)
    private void Start()
    {
        ApplyUserSessionToPhoton();
    }

    private void ApplyUserSessionToPhoton()
    {
        if (!PhotonNetwork.connected || PhotonNetwork.player == null)
        {
            Debug.LogWarning("[PhotonUserSessionSync] 아직 Photon에 연결되지 않음");
            return;
        }

        // 닉네임이 없으면 굳이 안 올림
        if (string.IsNullOrEmpty(UserSession.Nickname))
        {
            Debug.LogWarning("[PhotonUserSessionSync] UserSession.Nickname 이 비어있음");
            return;
        }

        var ht = new Hashtable
        {
            { "Nickname",  UserSession.Nickname },
            { "RankTitle", UserSession.RankTitle },
            { "ClearCount", UserSession.ClearCount }
        };

        PhotonNetwork.player.SetCustomProperties(ht);

        Debug.Log($"[PhotonUserSessionSync] 업로드 완료 → " +
                  $"Nick={UserSession.Nickname}, Rank={UserSession.RankTitle}, Clear={UserSession.ClearCount}");
    }
}
