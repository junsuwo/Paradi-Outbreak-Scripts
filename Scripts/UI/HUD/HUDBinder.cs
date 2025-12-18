using UnityEngine;

public class HUDBinder : MonoBehaviour
{
    public HUDController hud;

    void Start()
    {
        // 최초 값 세팅(예시)
        hud.SetWaveAndTime(1);
        hud.SetWallHp(1000, 1000);
        hud.SetPlayerHp(150, 150);
        hud.SetStamina(100, 100);
        hud.ShowBossHP(false);

        // 킬 카운트 자리 미리 생성
        for (int i = 1; i <= 8; i++) hud.SetKill(i, 0);
    }

    void Update()
    {
        // 예시 타이머 (실제는 WaveManager 시간 사용)
        // hud.SetWaveAndTime(currentWave, waveSecondsLeft);
        // hud.SetWallHp(WallManager.hp, WallManager.max);
        // hud.SetPlayerHp(Player.hp, Player.max);
        // hud.SetStamina(Player.sta, Player.staMax);
    }

    // 외부 시스템에서 호출할 간단 이벤트용 래퍼
    public void OnEnemyKilled(int playerNo) { /* 누적/호출 후 */ }
    public void OnBossSpawn(float maxHp) { hud.ShowBossHP(true); hud.SetBossHp(maxHp, maxHp); }
    public void OnBossDead() { hud.ShowBossHP(false); }
}
