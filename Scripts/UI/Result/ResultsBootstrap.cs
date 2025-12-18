using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ExitGames.Client.Photon;

public class ResultsBootstrap : MonoBehaviour
{
    [SerializeField] private MonoBehaviour dataSourceBehaviour;
    private IResultsDataSource dataSource;

    [SerializeField] private ResultsPanelController panel;

    [Header("PHP URL")]
    // 여기 수정됨
    [SerializeField]
    private string updateClearMvpUrl =
        "http://192.168.0.101/paradi/update_clear_mvp.php";

    void Awake()
    {
        dataSource = (IResultsDataSource)dataSourceBehaviour;
        Debug.Log("[ResultsBootstrap] Using data source = " + dataSource.GetType().Name);
    }

    public async void OnWaveCleared(int waveIndex, int teamWallHpLeft)
    {
        //  1) 결과 리스트 생성
        var results = await dataSource.GetWaveResultsAsync(waveIndex, teamWallHpLeft);

        //  2) Victory 결과창 표시
        panel.Show("Victory !", results);

        //  3) DB 업데이트 - MVP 체크
        bool isMvp = false;
        foreach (var r in results)
        {
            if (r.playerName == UserSession.Nickname && r.isMvp)
            {
                isMvp = true;
                break;
            }
        }
        // 🔹 4) ClearCount/MvpCount DB 반영
        if (UserSession.UserId > 0)
        {
            StartCoroutine(CoUpdateClearAndMvp(
                UserSession.UserId,
                1,             // add_clear = 1
                isMvp ? 1 : 0  // add_mvp
            ));
            // 로컬 UserSession 업데이트
            UserSession.ClearCount += 1;
            if (isMvp) UserSession.MvpCount += 1;
        }
    }

    private IEnumerator CoUpdateClearAndMvp(int userId, int addClear, int addMvp)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("add_clear", addClear);
        form.AddField("add_mvp", addMvp);

        using (UnityWebRequest www = UnityWebRequest.Post(updateClearMvpUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("[ResultDB] HTTP Error: " + www.error);
            else
                Debug.Log("[ResultDB] Response: " + www.downloadHandler.text);
        }
    }
}
