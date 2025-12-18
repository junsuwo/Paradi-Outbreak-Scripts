using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class RegisterManager : MonoBehaviour
{
    [Header("UI References (Register Panel)")]
    public TMP_InputField inputEmail;          // 회원가입 패널 이메일
    public TMP_InputField inputPassword;       // 비밀번호
    public TMP_InputField inputPasswordConfirm;// 비밀번호 확인
    public TMP_Text statusText;                // Text_RegisterStatus

    [Header("Server URL")]
    public string registerURL = "http://192.168.0.101/paradi/register.php";


    [Header("Optional")]
    public AuthUIController authUI;            // 회원가입 성공 후 로그인 패널로 돌아갈 때 사용

    [System.Serializable]
    private class RegisterResponse
    {
        public bool success;
        public string message;
    }

    public void OnClickRegister()
    {
        StartCoroutine(CoRegister());
    }

    private IEnumerator CoRegister()
    {
        string email = inputEmail.text;
        string pw = inputPassword.text;
        string pw2 = inputPasswordConfirm.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(pw2))
        {
            SetStatus("모든 항목을 입력하세요.", Color.red);
            yield break;
        }

        if (pw != pw2)
        {
            SetStatus("비밀번호가 서로 다릅니다.", Color.red);
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("username", email);   // PHP의 username 필드에 이메일 사용
        form.AddField("password", pw);

        using (UnityWebRequest www = UnityWebRequest.Post(registerURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                SetStatus("서버 연결 실패: " + www.error, Color.red);
                yield break;
            }

            string json = www.downloadHandler.text;
            // Debug.Log("Register response: " + json);

            RegisterResponse res = null;
            try
            {
                res = JsonUtility.FromJson<RegisterResponse>(json);
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
                SetStatus(res.message, Color.red);
            }
            else
            {
                SetStatus(res.message, Color.green);

                // 회원가입 성공 후 로그인 화면으로 자동 이동
                if (authUI != null)
                {
                    yield return new WaitForSeconds(1.0f); // 잠깐 메시지 보여주기
                    authUI.ShowLogin();
                }
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
