using TMPro;
using UnityEngine;

public class ResultRowUI : MonoBehaviour
{
    [Header("Name & Title")]
    public TMP_Text nicknameText;
    public TMP_Text rankTitleText;

    [Header("Stats")]
    public TMP_Text killText;
    public TMP_Text deathText;
    public TMP_Text wallHpText;
    public TMP_Text clearCountText;
    public TMP_Text mvpText;

    public void Set(PlayerResult r)
    {
        // 닉네임
        if (nicknameText != null)
            nicknameText.text = r.playerName;

        // 칭호 (괄호 스타일)
        if (rankTitleText != null)
        {
            if (string.IsNullOrEmpty(r.rankTitle))
                rankTitleText.text = "";
            else
                rankTitleText.text = $"({r.rankTitle})";   
        }

        killText.text = r.kills.ToString();
        deathText.text = r.deaths.ToString();
        wallHpText.text = r.wallHpLeft.ToString();
        clearCountText.text = r.clearCount.ToString();
        mvpText.text = r.isMvp ? "MVP" : "";
    }
}
