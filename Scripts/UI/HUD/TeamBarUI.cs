using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;

public class TeamBarUI : Photon.MonoBehaviour
{
    [Header("UI")]
    public Transform grid;
    public GameObject slotPrefab;

    [Header("Data")]
    public TitanData[] titans;
    private Dictionary<TitanId, TitanData> titanById;   //  TitanId로 수정

    void Awake()
    {
        titanById = new Dictionary<TitanId, TitanData>();
        foreach (var t in titans)
        {
            if (t != null)
            {
                if (!titanById.ContainsKey(t.id))
                    titanById.Add(t.id, t);
                else
                    Debug.LogWarning($"[TeamBar] Duplicate Titan ID: {t.id} ({t.name})");
            }
        }
    }

    void OnEnable() => Rebuild();
    public void OnJoinedRoom() => Rebuild();
    public void OnPhotonPlayerConnected(PhotonPlayer newP) => Rebuild();
    public void OnPhotonPlayerDisconnected(PhotonPlayer p) => Rebuild();
    public void OnPhotonPlayerPropertiesChanged(object[] payload) => Rebuild();

    void Rebuild()
    {
        foreach (Transform c in grid)
            Destroy(c.gameObject);

        if (PhotonNetwork.offlineMode)
        {
            BuildOffline();
            return;
        }

        var list = PhotonNetwork.playerList.OrderBy(p => p.ID);
        foreach (var p in list)
            BuildSlot(p);
    }

    void BuildSlot(PhotonPlayer p)
    {
        var go = Instantiate(slotPrefab, grid);
        var slot = go.GetComponent<TeamSlotUI>();

        // ==============================
        // 초상화 표시 (titanId 기반)
        // ==============================
        Sprite sp = null;
        int titanInt = TryGetInt(p, "titanId", -1);
        TitanId titanEnum = (TitanId)titanInt;  //  int → enum 변환

        if (titanById.TryGetValue(titanEnum, out var data) && data != null)
        {
            sp = data.portrait ? data.portrait : data.icon;
        }
        else
        {
            string sel = TryGetString(p, "K_SELECTED", null);
            if (!string.IsNullOrEmpty(sel))
            {
                if (System.Enum.TryParse(sel, out TitanId parsed))
                {
                    if (titanById.TryGetValue(parsed, out var fallback))
                        sp = fallback.portrait ? fallback.portrait : fallback.icon;
                }
            }
        }

        slot.SetPortrait(sp);

        // ==============================
        // HP 표시
        // ==============================
        float hp = TryGetFloat(p, "HP", 1f);
        float hpMax = TryGetFloat(p, "HPMax", 1f);
        slot.SetHP(hp, hpMax);

        Debug.Log($"[TeamBar] {p.NickName} titanId={(int)titanEnum} found={(sp != null)}");
    }

    void BuildOffline()
    {
        var go = Instantiate(slotPrefab, grid);
        var slot = go.GetComponent<TeamSlotUI>();

        var ps = PlayerStats.Local;
        if (ps && ps.currentTitan)
        {
            var sp = ps.currentTitan.portrait ? ps.currentTitan.portrait : ps.currentTitan.icon;
            slot.SetPortrait(sp);
        }
        slot.SetHP(1f, 1f);
    }

    int TryGetInt(PhotonPlayer p, string key, int def)
    {
        if (p.CustomProperties != null && p.CustomProperties.ContainsKey(key))
        {
            var v = p.CustomProperties[key];
            if (v is int i) return i;
            if (int.TryParse(v.ToString(), out int parsed)) return parsed;
        }
        return def;
    }

    string TryGetString(PhotonPlayer p, string key, string def)
    {
        if (p.CustomProperties != null && p.CustomProperties.ContainsKey(key))
        {
            object v = p.CustomProperties[key];
            return v?.ToString() ?? def;
        }
        return def;
    }

    float TryGetFloat(PhotonPlayer p, string key, float def)
    {
        if (p.CustomProperties != null && p.CustomProperties.ContainsKey(key))
        {
            var v = p.CustomProperties[key];
            if (v is float f) return f;
            if (v is double d) return (float)d;
            if (float.TryParse(v.ToString(), out float parsed)) return parsed;
        }
        return def;
    }
}
