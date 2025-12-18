using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MockResultsDataSource : MonoBehaviour, IResultsDataSource
{
    [Header("Mock Options")]
    public string[] sampleNames = { "Player", "Teammate1", "Teammate2", "Teammate3" };
    public Vector2Int killRange = new Vector2Int(10, 35);
    public Vector2Int deathRange = new Vector2Int(0, 5);

    public async Task<List<PlayerResult>> GetWaveResultsAsync(int waveIndex, int teamWallHpLeft)
    {
        // 간단한 목업 생성
        var list = new List<PlayerResult>();
        foreach (var n in sampleNames)
        {
            // 임시 칭호 (실제에선 DB/Photon에서 받아온 값 사용)
            string title = "훈련병";
            if (!string.IsNullOrEmpty(UserSession.Nickname) && n == UserSession.Nickname)
            {
                // 나 자신인 경우, 로그인 세션의 칭호 사용
                title = UserSession.RankTitle;
            }
            
            list.Add(new PlayerResult
            {
                playerName = n,
                kills = Random.Range(killRange.x, killRange.y + 1),
                deaths = Random.Range(deathRange.x, deathRange.y + 1),
                wallHpLeft = teamWallHpLeft,
                clearCount = await GetTotalClearCountAsync(n), // 오프라인에선 랜덤
                isMvp = false
            });
        }
        // MVP 지정: 가장 Kill 높은 사람
        int maxK = -1, idx = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].kills > maxK) { maxK = list[i].kills; idx = i; }
        }
        if (idx >= 0) list[idx].isMvp = true;

        return list;
    }

    public async Task<int> GetTotalClearCountAsync(string nickname)
    {
        await Task.Yield(); // 비동기 형태 유지
        return Random.Range(3, 40); //  나중에 DB 값으로 대체
    }
}
