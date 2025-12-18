using UnityEngine;
using System.Collections.Generic;

public class StatsPanel : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public StatItemUI item; // 프리팹 참조
    }

    [Header("Target Player")]
    public PlayerStats player;     // 인스펙터에 직접 연결하거나 AutoBind가 찾아줌

    [Header("UI Slots")]
    public List<Slot> slots = new();  // StatItemUI들을 드래그해서 넣기

    void OnEnable()
    {
        BindEvents(true);
        Refresh();
    }
    void OnDisable() => BindEvents(false);

    void BindEvents(bool bind)
    {
        if (!player) return;
        if (bind) player.OnStatsChanged += Refresh;
        else player.OnStatsChanged -= Refresh;
    }

    public void SetPlayer(PlayerStats p)
    {
        if (player == p) return;
        BindEvents(false);
        player = p;
        BindEvents(true);
        Refresh();
    }

    public void Refresh()
    {
        if (!player) return;
        foreach (var s in slots)
        {
            if (!s.item || !s.item.value) continue;
            s.item.value.text = GetStatText(s.item.statKey);
        }
    }

    string GetStatText(string key)
    {
        switch (key)
        {
            case "HP": return player.MaxHP.ToString();
            case "Regen": return player.HPRegen.ToString("0.0");
            case "Atk": return player.Attack.ToString("0");
            case "Def": return player.Defense.ToString("0");
            case "AS": return player.AttackSpeed.ToString("0.00");
            case "MS": return player.MoveSpeed.ToString("0.0");
            default: return "-";
        }
    }
}
