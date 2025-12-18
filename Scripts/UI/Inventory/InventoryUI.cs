using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InventoryUI : MonoBehaviour
{

    [Header("좌측 슬롯 영역")]
    public Transform gridParent;            // SlotGrid
    public InventorySlotUI slotTemplate;    // 슬롯 템플릿

    [Header("아이템 상세 영역")]
    public TMP_Text itemName;
    public TMP_Text itemDesc;
    public Button useButton;

    [Header("오른쪽 영역")]
    public Image profileImage;
    public Transform statList;              // VerticalLayoutGroup
    public Transform essenceParent;         // EssenceSlots
    public EssenceSlotUI essenceTemplate;

    public PlayerStats playerOverride;

    // 오른쪽 하단 “추가효과(정수 총합 %)” 출력용
    public TMP_Text extraEffectText;

    Inventory inv;
    PlayerStats player;

    readonly List<InventorySlotUI> slots = new();
    readonly List<TMP_Text> statLines = new();
    readonly List<EssenceSlotUI> essenceSlots = new();

    // ================== 바인딩 ==================
    public void Bind(Inventory inventory)
    {
        inv = inventory;

        // PlayerStats 찾기
        // player = (PlayerManager.Instance && PlayerManager.Instance.localPlayer)
        //     ? PlayerManager.Instance.localPlayer.GetComponent<PlayerStats>()
        //     : FindAnyObjectByType<PlayerStats>();
        player = FindObjectsOfType<PlayerStats>()
            .FirstOrDefault(p =>
            {
                var v = p.GetComponent<PhotonView>();
                return v != null && v.isMine;
            });
        
        if (player == null && inv != null)
            player = inv.GetComponentInParent<PlayerStats>();
        if (playerOverride) player = playerOverride;

        BuildGrid();
        BuildEssence();

        // 우측 스탯 라인 수집
        statLines.Clear();
        if (statList)
        {
            foreach (Transform t in statList)
            {
                var tmp = t.GetComponent<TMP_Text>();
                if (tmp) statLines.Add(tmp);
            }
        }

        // 이벤트 등록
        if (player)
        {
            player.OnStatsChanged -= RefreshStats;
            player.OnStatsChanged -= RefreshEssence;
            player.OnStatsChanged -= RefreshProfile;
            player.OnStatsChanged -= RefreshExtraEffects;

            player.OnStatsChanged += RefreshStats;
            player.OnStatsChanged += RefreshEssence;
            player.OnStatsChanged += RefreshProfile;
            player.OnStatsChanged += RefreshExtraEffects;
        }

        RefreshAll();
        Debug.Log($"[InvUI Bind] PlayerStats={player}, Titan={player?.currentTitan}");

    }


    public void AttachPlayer(PlayerStats p)
    {
        // 기존 구독 해제
        if (player)
        {
            player.OnStatsChanged -= RefreshStats;
            player.OnStatsChanged -= RefreshEssence;
            player.OnStatsChanged -= RefreshProfile;
            player.OnStatsChanged -= RefreshExtraEffects;
        }

        player = p;

        if (player)
        {
            player.OnStatsChanged += RefreshStats;
            player.OnStatsChanged += RefreshEssence;
            player.OnStatsChanged += RefreshProfile;
            player.OnStatsChanged += RefreshExtraEffects;
        }

        BuildEssence();   // player 기준으로 에센스 슬롯 다시 바인딩
        RefreshAll();
        Debug.Log($"[InvUI AttachPlayer] Player={p}, Titan={p.currentTitan}");

    }


    void OnEnable()
    {
        // 켜질 때 한 번 강제 갱신해서 클릭 없이도 보이게
        ForceUIRefresh();
    }
    void OnDisable()
    {
        if (player)
        {
            player.OnStatsChanged -= RefreshStats;
            player.OnStatsChanged -= RefreshEssence;
            player.OnStatsChanged -= RefreshProfile;
            player.OnStatsChanged -= RefreshExtraEffects;
        }
    }

    // ================== UI 빌드 ==================
    void BuildGrid()
    {
        slots.Clear();
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(gridParent.GetChild(i).gameObject);

        for (int i = 0; i < inv.capacity; i++)
        {
            var s = Instantiate(slotTemplate, gridParent);
            s.name = $"Slot_{i}";
            s.Bind(inv, i);
            slots.Add(s);
        }
    }

    void BuildEssence()
    {
        essenceSlots.Clear();
        for (int i = essenceParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(essenceParent.GetChild(i).gameObject);

        for (int i = 0; i < 4; i++)
        {
            var e = Instantiate(essenceTemplate, essenceParent);
            e.name = $"Essence_{i}";
            e.Bind(player, i, this); // 툴팁 콜백 위해 InventoryUI 자신을 넘김
            essenceSlots.Add(e);
        }
    }

    // ================== 갱신 ==================
    public void RefreshAll()
    {
        // 인벤토리 슬롯
        for (int i = 0; i < slots.Count; i++)
        {
            var st = (i < inv.items.Count) ? inv.items[i] : null;
            bool selected = (i == inv.SelectedIndex);
            slots[i].Set(st, selected);
        }

        RefreshDetail(inv.SelectedIndex);
        RefreshStats();
        RefreshEssence();
        RefreshProfile();
        RefreshExtraEffects();   // 오른쪽 하단 “추가효과(정수 총합 %)” 갱신
    }

    void RefreshProfile()
    {

        if (!profileImage) return;

        TitanData titan = player ? player.currentTitan : null;
        if (titan != null)
        {
            var sp = titan.portrait ? titan.portrait : titan.icon;
            profileImage.sprite = sp;
            profileImage.color = sp ? Color.white : Color.clear;
        }
        else
        {
            var sp = Resources.Load<Sprite>("UI/DefaultProfile");
            profileImage.sprite = sp;
            profileImage.color = sp ? Color.white : Color.clear;
        }
    }

    public void RefreshDetail(int index)
    {
        if (index < 0 || index >= inv.items.Count)
        {
            if (itemName) itemName.text = "";
            if (itemDesc) itemDesc.text = "";
            return;
        }

        var st = inv.items[index];
        if (st.data != null)
        {
            if (itemName) itemName.text = st.data.displayName;
            if (itemDesc) itemDesc.text = string.IsNullOrEmpty(st.data.description) ? "" : st.data.description;
        }
        else
        {
            if (itemName) itemName.text = "";
            if (itemDesc) itemDesc.text = "";
        }
    }

    // 오른쪽 하단: 장착된 정수들의 “총합 %” 표시
    void RefreshExtraEffects()
    {
        if (!extraEffectText) return;

        if (!player || player.essence == null)
        {
            extraEffectText.text = "";
            return;
        }

        float atk = 0, def = 0, hp = 0, aspd = 0, mspd = 0, regen = 0;

        foreach (var e in player.essence)
        {
            if (!e) continue;
            atk += e.addAttack;
            def += e.addDefense;
            hp += e.addHP;
            aspd += e.addAttackSpeed;
            mspd += e.addMoveSpeed;
            regen += e.addRegen;
        }

        bool any =
            Mathf.Abs(atk) > 0.0001f ||
            Mathf.Abs(def) > 0.0001f ||
            Mathf.Abs(hp) > 0.0001f ||
            Mathf.Abs(aspd) > 0.0001f ||
            Mathf.Abs(mspd) > 0.0001f ||
            Mathf.Abs(regen) > 0.0001f;

        if (!any) { extraEffectText.text = ""; return; }

        System.Text.StringBuilder sb = new();
        sb.AppendLine("<color=#FFD700>추가효과</color>");

        void Line(string n, float v)
        {
            if (Mathf.Abs(v) > 0.0001f)
                sb.AppendLine($"{n} +{v * 100f:0.#}%");
        }

        Line("공격력", atk);
        Line("방어력", def);
        Line("체력", hp);
        Line("공격속도", aspd);
        Line("이동속도", mspd);
        Line("회복력", regen);

        extraEffectText.text = sb.ToString();
    }

    void RefreshStats()
    {
        if (!player) return;

        string[] names = {
            "공격력", "방어력", "체력",
            "공격속도", "이동속도", "체력 재생력"
        };

        float[] vals = {
            player.Attack,
            player.Defense,
            player.MaxHP,
            player.AttackSpeed,
            player.MoveSpeed,
            player.HPRegen
        };

        int c = Mathf.Min(statLines.Count, names.Length);
        for (int i = 0; i < c; i++)
            statLines[i].text = $"{names[i]}: {vals[i]:0.##}";
    }

    // ---------- 툴팁용 유틸 ----------
    // 정수 아이템 하나의 효과만 보여주기
    public void ShowEssenceTooltip(ItemData d)
    {
        if (!extraEffectText) return;
        if (!d) { extraEffectText.text = ""; return; }
        extraEffectText.text = MakeBonusText(d);
    }

    // 호버에서 빠져나오면 다시 “총합 %효과”로 복구
    public void ShowEssenceTotal()
    {
        RefreshExtraEffects();
    }

    // 단일 아이템 효과 텍스트(퍼센트 표기)
    string MakeBonusText(ItemData d)
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine("<color=#FFD700>추가효과</color>");

        void Line(string n, float v)
        {
            if (Mathf.Abs(v) > 0.0001f)
                sb.AppendLine($"{n} +{v * 100f:0.#}%");
        }

        Line("공격력", d.addAttack);
        Line("방어력", d.addDefense);
        Line("체력", d.addHP);
        Line("공격속도", d.addAttackSpeed);
        Line("이동속도", d.addMoveSpeed);
        Line("회복력", d.addRegen);

        return sb.ToString();
    }
    // ---------- /툴팁 유틸 ----------

    void RefreshEssence()
    {
        if (!player) return;
        for (int i = 0; i < essenceSlots.Count; i++)
        {
            var data = (player.essence != null && i < player.essence.Length)
                       ? player.essence[i] : null;
            essenceSlots[i].Set(data);
        }
    }

    // ============== Use 버튼 바인딩 ==============
    void Start()
    {
        if (useButton)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OnClickUse);
        }
    }
    public void ForceUIRefresh()
    {
        // 정수/스탯/프로필 갱신
        RefreshEssence();
        RefreshStats();
        RefreshProfile();
        RefreshExtraEffects();

        // 레이아웃/캔버스 강제 갱신 (아이콘 즉시 반영)
        if (essenceParent)
            LayoutRebuilder.ForceRebuildLayoutImmediate(essenceParent as RectTransform);
        if (statList)
            LayoutRebuilder.ForceRebuildLayoutImmediate(statList as RectTransform);

        Canvas.ForceUpdateCanvases();
    }

    void OnClickUse()
    {
        if (inv == null) return;
        int idx = inv.SelectedIndex;
        if (idx < 0) return;
        inv.UseAt(idx);
    }
}
