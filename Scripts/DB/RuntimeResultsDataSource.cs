using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RuntimeResultsDataSource : MonoBehaviour, IResultsDataSource
{
    // 1) Wave 결과 리스트 생성
    public async Task<List<PlayerResult>> GetWaveResultsAsync(int waveIndex, int teamWallHpLeft)
    {
        await Task.Yield();

        var list = new List<PlayerResult>();

        foreach (var p in PhotonNetwork.playerList)
        {
            string nickname = GetStringProp(p, "Nickname", p.NickName);

            // 여기서 더 이상 UserSession.RankTitle 을 쓰지 않음
            string rankTitle = GetStringProp(p, "RankTitle", "훈련병");

            int kills = GetIntProp(p, "Kills", 0);
            int deaths = GetIntProp(p, "Deaths", 0);

            // 여기서도 UserSession.ClearCount 대신 0 기본값
            int clearCount = GetIntProp(p, "ClearCount", 0);

            list.Add(new PlayerResult
            {
                playerName = nickname,
                rankTitle = rankTitle,
                kills = kills,
                deaths = deaths,
                wallHpLeft = teamWallHpLeft,
                clearCount = clearCount,
                isMvp = false
            });
        }

        // 정렬 규칙: Kill 내림차순 → Death 오름차순
        list.Sort((a, b) =>
        {
            int cmp = b.kills.CompareTo(a.kills);
            if (cmp != 0) return cmp;

            return a.deaths.CompareTo(b.deaths);
        });

        // 정렬 후 0번 인덱스를 MVP로
        if (list.Count > 0)
            list[0].isMvp = true;

        return list;
    }

    // 2) 인터페이스 요구로 형식상 구현 (지금은 UserSession 값만 반환)
    public async Task<int> GetTotalClearCountAsync(string nickname)
    {
        await Task.Yield();

        if (!string.IsNullOrEmpty(UserSession.Nickname) &&
            nickname == UserSession.Nickname)
        {
            return UserSession.ClearCount;
        }

        return 0;
    }

    string GetStringProp(PhotonPlayer p, string key, string def)
    {
        if (p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(key) &&
            p.CustomProperties[key] is string s &&
            !string.IsNullOrEmpty(s))
        {
            return s;
        }
        return def;
    }

    int GetIntProp(PhotonPlayer p, string key, int def)
    {
        if (p.CustomProperties != null &&
            p.CustomProperties.ContainsKey(key) &&
            p.CustomProperties[key] is int v)
        {
            return v;
        }
        return def;
    }
}
