using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;   // 씬 이동에 필요
using TMPro;

// 🔥 Photon 관련 using 추가
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LoginManager : MonoBehaviour
{
    [Header("UI References (Login Panel)")]
    public TMP_InputField inputEmail;      // LoginPanel의 이메일 입력
    public TMP_InputField inputPassword;   // LoginPanel의 비밀번호 입력
    public TMP_Text statusText;            // Text_LoginStatus

    [Header("Server URL")]
    public string loginURL = "http://192.168.0.101/paradi/login.php";

    [Header("Scene Names")]
    public string lobbySceneName = "Lobby";        // 닉네임 있는 유저 → 로비
    public string nicknameSceneName = "User_Id";   // 닉네임 없는 유저 → 닉네임 설정 씬

    [System.Serializable]
    private class LoginResponse
    {
        public bool success;
        public string message;
        public int userId;
        public string username;

        public string nickname;

        // 전적/랭크 정보
        public int clear_count;
        public int mvp_count;
        public int rank_level;
        public string rank_title;
    }

    public void OnClickLogin()
    {
        StartCoroutine(CoLogin());
    }

    private IEnumerator CoLogin()
    {
        string email = inputEmail.text;
        string pw = inputPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw))
        {
            SetStatus("아이디와 비밀번호를 입력하세요.", Color.red);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("username", email);   // PHP 쪽에서는 username으로 받음
        form.AddField("password", pw);

        using (UnityWebRequest www = UnityWebRequest.Post(loginURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                SetStatus("서버 연결 실패: " + www.error, Color.red);
                yield break;
            }

            string json = www.downloadHandler.text;
            // Debug.Log("Login response: " + json);

            LoginResponse res = null;
            try
            {
                res = JsonUtility.FromJson<LoginResponse>(json);
            }
            catch
            {
                SetStatus("응답 파싱 실패", Color.red);
                yield break;
            }

            if (res == null)
            {
                SetStatus("응답이 비어있습니다.", Color.red);
            }
            else if (!res.success)
            {
                // 로그인 실패
                SetStatus(res.message, Color.red);
            }
            else
            {
                // 로그인 성공
                SetStatus(res.message, Color.green);

                // 공통 정보 저장
                PlayerPrefs.SetInt("UserId", res.userId);
                PlayerPrefs.SetString("Username", res.username);
                PlayerPrefs.SetString("RankTitle", res.rank_title);

                // 런타임 세션에도 저장
                UserSession.UserId = res.userId;
                UserSession.Username = res.username;
                UserSession.Nickname = res.nickname;

                // 전적/랭크 정보도 세션에 저장
                UserSession.ClearCount = res.clear_count;
                UserSession.MvpCount = res.mvp_count;
                UserSession.RankLevel = res.rank_level;
                UserSession.RankTitle = res.rank_title;

                Debug.Log($"[Login] 세션 저장 완료 → " +
                          $"ID:{UserSession.UserId}, Nick:{UserSession.Nickname}, Title:{UserSession.RankTitle}");

                // ⭐ 로그인 후 내 정보 Photon CustomProperties로 업로드
                UploadSessionToPhoton();

                // 닉네임 여부에 따라 씬 이동
                HandleNicknameFlow(res);
            }

        }
    }

    // 🔥 새로 추가된 함수: UserSession 정보를 Photon에 올림
    private void UploadSessionToPhoton()
    {
        if (!PhotonNetwork.connected || PhotonNetwork.player == null)
        {
            Debug.LogWarning("[Login] Photon에 아직 연결되지 않아 CustomProperties를 설정할 수 없습니다.");
            return;
        }

        var ht = new Hashtable
        {
            { "Nickname",  UserSession.Nickname },
            { "RankTitle", UserSession.RankTitle },
            { "ClearCount", UserSession.ClearCount }
        };

        PhotonNetwork.player.SetCustomProperties(ht);

        Debug.Log($"[Login] Photon Custom Properties 업로드 완료 → " +
                  $"Nick={UserSession.Nickname}, Rank={UserSession.RankTitle}, Clear={UserSession.ClearCount}");
    }

    private void HandleNicknameFlow(LoginResponse res)
    {
        string nick = res.nickname;

        if (!string.IsNullOrEmpty(nick))
        {
            // 이미 닉네임이 있는 계정 → 바로 Lobby로
            // PUN1: PhotonNetwork.playerName
            PhotonNetwork.playerName = nick;
            PlayerPrefs.SetString("Nickname", nick);

            if (!string.IsNullOrEmpty(lobbySceneName))
            {
                SceneManager.LoadScene(lobbySceneName);
            }
        }
        else
        {
            // 아직 닉네임 설정 안 된 계정 → User_Id 씬으로
            if (!string.IsNullOrEmpty(nicknameSceneName))
            {
                SceneManager.LoadScene(nicknameSceneName);
            }
        }
    }

    private void SetStatus(string msg, Color color)
    {
        if (statusText)
        {
            statusText.text = msg;
            statusText.color = color;
        }
    }
}
