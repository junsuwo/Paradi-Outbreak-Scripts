using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HUDController : MonoBehaviour
{
    [Header("Top/Wave")]
    [SerializeField] TMP_Text waveTitle;   // "웨이브진행도   남은시간"
    [SerializeField] TMP_Text waveInfo;    // "1wave   60:00"
    [SerializeField] Image wallHp;
    [SerializeField] TMP_Text wallHpLabel;

    [Header("Kill Count")]
    [SerializeField] Transform killListRoot; // VerticalLayout
    [SerializeField] TMP_Text killEntryPrefab;

    [Header("Boss")]
    [SerializeField] GameObject bossBox;
    [SerializeField] Image bossHp;
    [SerializeField] TMP_Text bossHpLabel;

    [Header("Player")]
    [SerializeField] Image playerHp;
    [SerializeField] TMP_Text playerHpLabel;
    [SerializeField] Image stamina;
    [SerializeField] TMP_Text staminaLabel;

    [Header("QuickSlots")]
    [SerializeField] List<Image> slotIcons = new();
    [SerializeField] List<TMP_Text> slotCounts = new();

    [Header("Chat")]
    [SerializeField] TMP_Text chatLog; // ScrollView의 Content 안 1개 TMP (auto-size)
    [SerializeField] ScrollRect chatScroll;

    private PlayerHealth localHealth;

    readonly Dictionary<int, TMP_Text> _killRows = new();

    void Awake() { bossBox.SetActive(false); }

    public void BindLocalPlayer(PlayerHealth hp)
    {
        localHealth = hp;
    }
    // ===== Wave / Time / Wall =====
    public void SetWaveAndTime(int waveIndex)
    {
        waveInfo.text = $"{waveIndex}wave";
    }
    
    void Update()
    {
        if (localHealth != null)
            SetPlayerHp(localHealth.currentHP, localHealth.maxHP);
    }
    public void SetWallHp(float cur, float max)
    {
        if (wallHp)
        {
            float ratio = Mathf.Clamp01(cur / max);
            wallHp.fillAmount = ratio;
        }

        if (wallHpLabel)
            wallHpLabel.text = $"{Mathf.CeilToInt(cur)}/{Mathf.CeilToInt(max)}";
    }

    // ===== Player =====
    public void SetPlayerHp(float cur, float max)
    {
        if (playerHp)
        {
            float ratio = Mathf.Clamp01(cur / max);
            playerHp.fillAmount = ratio;
        }

        if (playerHpLabel)
            playerHpLabel.text = $"{Mathf.CeilToInt(cur)}/{Mathf.CeilToInt(max)}";
    }
    public void SetStamina(float cur, float max)
    {
        if (stamina)
        {
            float ratio = Mathf.Clamp01(cur / max);
            stamina.fillAmount = ratio;
        }

        if (staminaLabel)
            staminaLabel.text = $"{Mathf.CeilToInt(cur)}/{Mathf.CeilToInt(max)}";
    }

    // ===== Boss =====
    public void ShowBossHP(bool show)
    {
        bossBox.SetActive(show);
    }
    public void SetBossHp(float cur, float max)
    {
        if (bossHp)
        {
            float ratio = Mathf.Clamp01(cur / max);
            bossHp.fillAmount = ratio;
        }

        if (bossHpLabel)
            bossHpLabel.text = $"{Mathf.CeilToInt(cur)}/{Mathf.CeilToInt(max)}";
    }

    // ===== Kill Count =====
    public void SetKill(int playerIndex1Based, int kills)
    {
        if (!_killRows.TryGetValue(playerIndex1Based, out var row))
        {
            row = Instantiate(killEntryPrefab, killListRoot);
            _killRows[playerIndex1Based] = row;
        }
        row.text = $"{playerIndex1Based}번 : {kills}킬";
    }

    // ===== Quick Slots =====
    public void SetQuickSlot(int slot, Sprite icon, int count = 0)
    {
        if (slot < 0 || slot >= slotIcons.Count) return;
        slotIcons[slot].enabled = (icon != null);
        slotIcons[slot].sprite = icon;
        if (slot < slotCounts.Count) slotCounts[slot].text = count > 1 ? count.ToString() : "";
    }

    // ===== Chat =====
    public void AddChat(string nickname, string message)
    {
        chatLog.text += $"\n{nickname} : {message}";
        Canvas.ForceUpdateCanvases();
        chatScroll.normalizedPosition = new Vector2(0, 0); // 아래로
    }

    string FormatTime(float t)
    {
        t = Mathf.Max(0, t);
        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        return $"{m:00}:{s:00}";
    }
}
