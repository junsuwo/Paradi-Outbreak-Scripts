using UnityEngine;
using UnityEngine.UI;

public class ProfilePortraitUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats player;   // 비워두면 자동으로 PlayerStats.Local로 세팅
    public Image portraitImg;    // 반드시 Profile/Mask/Portrait(Image)를 드래그 연결

    void Awake()
    {
        // 런타임에 이미 로컬이 준비되어 있으면 즉시 바인딩
        if (!player && PlayerStats.Local) SetPlayer(PlayerStats.Local);

        // 이후에 스폰돼도 이벤트로 잡음
        PlayerStats.OnLocalReady += SetPlayer;
    }

    void OnDestroy()
    {
        PlayerStats.OnLocalReady -= SetPlayer;
        if (player != null) player.OnStatsChanged -= Refresh;
    }

    public void SetPlayer(PlayerStats ps)
    {
        if (player == ps) { Refresh(); return; }

        if (player != null) player.OnStatsChanged -= Refresh; // 이전 구독 해제
        player = ps;
        if (player != null) player.OnStatsChanged += Refresh;
        Refresh();
#if UNITY_EDITOR
        if (player) Debug.Log($"[ProfilePortraitUI] Bound to {player.name}");
#endif
    }

    public void Refresh()
    {
        if (!portraitImg) return;

        Sprite sp = null;
        if (player && player.currentTitan)
            sp = player.currentTitan.portrait ? player.currentTitan.portrait : player.currentTitan.icon;

        portraitImg.enabled = sp != null;
        portraitImg.sprite = sp;
        portraitImg.preserveAspect = true;
    }
}
