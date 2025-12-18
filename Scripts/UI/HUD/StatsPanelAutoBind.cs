using UnityEngine;

public class StatsPanelAutoBind : MonoBehaviour
{
    public StatsPanel panel;
    public float retryInterval = 0.5f;
    float t;

    void Reset() { panel = GetComponent<StatsPanel>(); }

    void Update()
    {
        if (!panel || panel.player) return;
        t += Time.deltaTime;
        if (t < retryInterval) return;
        t = 0f;

        // 가장 간단한 방식: 씬에서 첫 PlayerStats 찾기
        var ps = FindObjectOfType<PlayerStats>();
        if (ps) panel.SetPlayer(ps);
    }
}
