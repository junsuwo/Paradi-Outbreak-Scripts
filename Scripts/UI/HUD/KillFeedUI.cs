using System.Linq;
using UnityEngine;
using TMPro;

// PUN1: Photon.MonoBehaviour 를 상속 (override 사용 X)
public class KillFeedUI : Photon.MonoBehaviour
{
    [Header("UI")]
    public Transform content;      // TopRight_KillCount 안의 List
    public GameObject rowPrefab;   // Row 프리팹 (Name, Count 2개의 TMP_Text 포함)

    void OnEnable() => Rebuild();

    // ── PUN1 콜백 (override 아님!) ─────────────────────────────────────────
    public void OnJoinedRoom() => Rebuild();
    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer) => Rebuild();
    public void OnPhotonPlayerDisconnected(PhotonPlayer other) => Rebuild();
    public void OnPhotonPlayerPropertiesChanged(object[] data) => Rebuild();

    void Rebuild()
    {
        // 기존 행들 삭제
        foreach (Transform c in content)
            Destroy(c.gameObject);

        // 오프라인 모드면 로컬 한 줄만
        if (PhotonNetwork.offlineMode)
        {
            var go = Instantiate(rowPrefab, content);
            var t = go.GetComponentsInChildren<TMP_Text>();
            t[0].text = "You";
            t[1].text = KillReporter.offlineKills.ToString();
            return;
        }

        // 온라인: 방에 있는 모든 플레이어 순회
        var list = PhotonNetwork.playerList.OrderBy(p => p.ID);
        foreach (var p in list)
        {
            int k = 0;
            var props = p.CustomProperties;
            if (props != null)
            {
                // 🔥 지금은 PlayerStatsTracker가 "Kills" 키를 사용함
                if (props.ContainsKey("Kills"))
                    k = (int)props["Kills"];
                // 혹시 예전 데이터에 "K" 만 있을 경우를 대비해 fallback
                else if (props.ContainsKey("K"))
                    k = (int)props["K"];
            }

            var go = Instantiate(rowPrefab, content);
            var t = go.GetComponentsInChildren<TMP_Text>();

            // 닉네임(없으면 "Player ID")
            string nick = !string.IsNullOrEmpty(p.NickName)
                ? p.NickName
                : $"Player {p.ID}";

            t[0].text = nick;      // 플레이어 이름
            t[1].text = k.ToString(); // 그 플레이어의 킬 수
        }
    }
}
