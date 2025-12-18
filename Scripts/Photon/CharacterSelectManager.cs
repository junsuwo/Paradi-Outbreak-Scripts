using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine.SceneManagement;   // 오프라인일 때 로컬 씬 로드용

// Photon (PUN1)
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CharacterSelectManager : Photon.MonoBehaviour
{
    // ===== 데이터 =====
    [Header("Data")]
    public TitanData[] titans;                          // 캐릭터  목록
    private Dictionary<TitanId, TitanData> dataMap;     // id 매핑

    // ===== UI 참조 =====
    [Header("UI Refs")]
    public Transform heroStripParent;                   // 하단 선택 버튼 부모
    public TitanSlotUI slotPrefab;                      // 하단 버튼 프리팹
    public RawImage previewPortrait;                    // 중앙 프리뷰 이미지(옵션)
    public TMP_Text previewName;                        // 중앙 프리뷰 이름
    public Button selectButton;
    public TMP_Text selectButtonText;
    public TMP_Text countdownText;

    // ===== 팀 표시(상단/우측) =====
    [Header("Team UI (optional)")]
    public Image[] teamIcons;                           // 우측 작은 아이콘들(옵션)
    public PreviewPresenter previewPresenter;           // 중앙 3D 프리뷰(옵션)
    public Transform teamBarParent;                     // Canvas/TopBar/TeamBar
    public TeamPickSlotUI teamSlotPrefab;               // 팀바 슬롯 프리팹
    private readonly List<TeamPickSlotUI> teamSlots = new List<TeamPickSlotUI>();

    // ===== "픽했을 때만" 다른 이미지를 쓰기 위한 오버라이드 리스트 =====
    [System.Serializable]
    public struct PickedIconEntry
    {
        public TitanId id;           // 캐릭터 ID
        public Sprite pickedSprite;  // "선택 완료" 상태에서 보여줄 전용 이미지
    }

    [Header("Picked Icon Overrides (optional)")]
    public List<PickedIconEntry> pickedIconOverrides = new List<PickedIconEntry>(); // 인스펙터에서 세팅
    private Dictionary<TitanId, Sprite> pickedIconMap = new Dictionary<TitanId, Sprite>(); // 런타임 맵

    // ===== Photon 커스텀 키 =====
    const string K_SELECTED = "SelectedTitan";
    const string K_READY = "Ready";
    const string K_LOCKED = "LockedTitans";
    const string K_TIMER_END = "SelectEndUTCTicks";

    // ===== 로컬 상태 =====
    private readonly List<TitanSlotUI> slots = new List<TitanSlotUI>(); // 하단 버튼 슬롯들
    private HashSet<string> lockedSet = new HashSet<string>();           // 룸 잠금 세트
    private TitanData hovered;                                           // 마지막 선택 후보(하이라이트)
    private TitanData mine;                                              // 내가 최종 선택한 캐릭터

    // ---------- Unity 라이프사이클 ----------
    void Awake()
    {
        // SO → 맵 구성
        dataMap = new Dictionary<TitanId, TitanData>();
        foreach (var t in titans) dataMap[t.id] = t;

        // 오버라이드용 아이콘 맵 구성 (id → 선택완료 전용 스프라이트)
        pickedIconMap.Clear();
        foreach (var entry in pickedIconOverrides)
        {
            if (entry.pickedSprite != null && !pickedIconMap.ContainsKey(entry.id))
                pickedIconMap.Add(entry.id, entry.pickedSprite);
        }
        PhotonNetwork.automaticallySyncScene = true;
    }

    void Start()
    {
        if (!PhotonNetwork.connected && !PhotonNetwork.offlineMode)
        {
            PhotonNetwork.offlineMode = true;
            if (!PhotonNetwork.inRoom)
            {
                if (string.IsNullOrEmpty(PhotonNetwork.playerName))
                    PhotonNetwork.playerName = $"Player {Random.Range(1000, 9999)}";
                PhotonNetwork.CreateRoom("DEV");
            }
        }


        // 하단 선택 스트립 생성
        foreach (var t in titans)
        {
            var s = Instantiate(slotPrefab, heroStripParent);
            s.Bind(t, OnClickSlot, IsLocked(t.id), false);
            slots.Add(s);
        }

        // 버튼 이벤트
        selectButton.onClick.AddListener(OnClickSelect);

        // 초기 UI
        SetPreview(null);
        UpdateCountdownFromRoom();
        RefreshAllUI();
        BuildTeamBar();
        RefreshAllUI();

        // 마스터가 처음 들어온 경우 타이머 세팅(이미 있으면 유지)
        if (PhotonNetwork.isMasterClient)
        {
            var roomProps = PhotonNetwork.room != null ? PhotonNetwork.room.CustomProperties : null;
            if (roomProps == null || !roomProps.ContainsKey(K_TIMER_END))
            {
                long end = System.DateTime.UtcNow.AddSeconds(60).Ticks; // 15초 카운트다운

                // ⬇로컬에도 즉시 반영 (콜백 기다리지 않음)
                _endTicks = end;

                Hashtable ht = new Hashtable { { K_TIMER_END, end } };
                PhotonNetwork.room.SetCustomProperties(ht);
            }
        }
        else
        {
            // 마스터가 올려둔 값이 있을 수 있으니 입장 즉시 한 번 읽기
            UpdateCountdownFromRoom();
        }
        if (PhotonNetwork.room == null) // 오프라인/룸 없음
        {
            _endTicks = System.DateTime.UtcNow.AddSeconds(15).Ticks;
        }
        ResetSelectionUI();   // UI 초기화
        ClearMySelection();

    }

    // ---------- 하단 카드 클릭 ----------
    void OnClickSlot(TitanData t)
    {
        hovered = t;
        SetPreview(t);
        selectButtonText.text = (mine != null && mine.id == t.id) ? "CANCEL" : "SELECT";
        if (selectButton != null)
            selectButton.interactable = true;
    }

    // 중앙 프리뷰 설정 (3D 프리뷰 우선)
    void SetPreview(TitanData t)
    {
        bool has = (t != null);
        previewPortrait.enabled = has;
        previewName.enabled = has;

        if (has)
        {
            if (previewPresenter) previewPresenter.Show(t.previewPrefab, t.previewOffset); // 3D 프리뷰
            previewName.text = t.displayName;
        }
        else
        {
            if (previewPresenter) previewPresenter.Show(null);
            previewName.text = "";
        }
    }

    // ---------- 선택 버튼 ----------
    void OnClickSelect()
    {
        if (hovered == null) return;

        // 같은 걸 다시 누르면 선택 취소(토글)
        if (mine != null && mine.id == hovered.id)
        {
            ClearMySelection();
            return;
        }

        // 이미 다른 플레이어가 잠근 캐릭터면 무시
        if (IsLocked(hovered.id))
        {
            Debug.Log($"[Select] already locked: {hovered.id}");
            return;
        }

        // 내 선택 저장 (내 커스텀 프로퍼티에 Selected/Ready 반영)
        SetMySelected(hovered.id);

        // --- PlayerStats & InventoryUI 동기화 (로컬 전용) ---
        var ps = FindObjectOfType<PlayerStats>(true);
        if (ps) ps.currentTitan = hovered; // 선택한 TitanData 넘기기
        ps.Recalculate();                // 선택된 타이탄 기준으로 스탯 재계산
        ps.OnStatsChanged?.Invoke();     // UI에 즉시 반영 (프로필/스탯창 모두)


        var invUI = FindObjectOfType<InventoryUI>(true);
        if (invUI) invUI.ForceUIRefresh(); // 오른쪽 네모칸 즉시 갱신
                                           // ---------------------------------------------
                                           // 방장이라면 즉시 룸 잠금 반영, 아니면 방장에게 요청 RPC
        if (PhotonNetwork.isMasterClient) LockInRoom(hovered.id);
        else photonView.RPC("RPC_RequestLock", PhotonTargets.MasterClient, (int)hovered.id);

        selectButtonText.text = "CANCEL";
        RefreshAllUI();
    }

    // 내 선택 반영(커스텀 프로퍼티: Selected/Ready)
    void SetMySelected(TitanId id)
    {
        mine = dataMap[id];

        Hashtable p = new Hashtable
    {
        { K_SELECTED, id.ToString() },
        { K_READY, true },
        { "titanId", (int)mine.id }   //  enum → int 변환
    };
        PhotonNetwork.player.SetCustomProperties(p);
    }


    // 내 선택 취소
    void ClearMySelection()
    {
        // 취소 전에 이전 선택을 잡아둔다 (비동기 대비)
        string prevSelected = GetMySelectedName();

        mine = null;

        // 내 커스텀 프로퍼티 초기화
        Hashtable p = new Hashtable { { K_SELECTED, null }, { K_READY, false } };
        PhotonNetwork.player.SetCustomProperties(p);

        // 방장은 룸 락에서도 제거
        if (PhotonNetwork.isMasterClient && !string.IsNullOrEmpty(prevSelected))
        {
            var set = GetLockedSetFromRoom();
            set.Remove(prevSelected);
            SaveLockedSetToRoom(set);
        }

        selectButtonText.text = "SELECT";
        RefreshAllUI();

        if (selectButton != null)
            selectButton.interactable = false;
    }

    // 내 현재 선택 이름(문자열) 가져오기
    string GetMySelectedName()
    {
        if (mine != null) return mine.id.ToString();
        var props = PhotonNetwork.player.CustomProperties;
        if (props != null && props.ContainsKey(K_SELECTED))
            return props[K_SELECTED] as string;
        return null;
    }

    // ---------- 방 잠금 처리 ----------
    [PunRPC]
    void RPC_RequestLock(int id)
    {
        LockInRoom((TitanId)id);
    }

    // 선택된 캐릭터를 룸 잠금 세트에 추가하고 저장
    void LockInRoom(TitanId id)
    {
        var set = GetLockedSetFromRoom();
        set.Add(id.ToString());
        SaveLockedSetToRoom(set);
    }

    // 룸에서 잠금 세트 읽기
    HashSet<string> GetLockedSetFromRoom()
    {
        var result = new HashSet<string>();
        if (PhotonNetwork.room != null && PhotonNetwork.room.CustomProperties != null)
        {
            var roomProps = PhotonNetwork.room.CustomProperties;
            if (roomProps.ContainsKey(K_LOCKED))
            {
                string s = roomProps[K_LOCKED] as string;
                if (!string.IsNullOrEmpty(s))
                {
                    var tokens = s.Split(',');
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        var token = tokens[i].Trim();
                        if (!string.IsNullOrEmpty(token)) result.Add(token);
                    }
                }
            }
        }
        return result;
    }

    // 잠금 세트 문자열로 저장
    void SaveLockedSetToRoom(HashSet<string> set)
    {
        // 오프라인이면 룸에 못 씀 → 로컬 변수만 갱신하고 끝
        if (PhotonNetwork.room == null)
        {
            lockedSet = set;
            return;
        }

        string joined = string.Join(",", new List<string>(set).ToArray());
        Hashtable ht = new Hashtable { { K_LOCKED, joined } };
        PhotonNetwork.room.SetCustomProperties(ht);
    }


    // 해당 id가 룸 잠금 상태인지
    bool IsLocked(TitanId id) { return lockedSet.Contains(id.ToString()); }

    // ---------- "픽했을 때" 표시 이미지 선택 헬퍼 ----------
    // pickedIconOverrides에 등록된 스프라이트가 있으면 그걸 사용,
    // 없으면 기존 dataMap[id].icon으로 대체
    Sprite GetPickedSprite(TitanId id)
    {
        // 1) 인스펙터에서 지정한 오버라이드가 있으면 그거 사용
        if (pickedIconMap.TryGetValue(id, out var overrideSp) && overrideSp != null)
            return overrideSp;

        // 2) 없다면 TitanData에서 portrait 우선, 없으면 icon
        if (dataMap != null && dataMap.TryGetValue(id, out var t) && t != null)
            return (t.portrait != null) ? t.portrait : t.icon;

        return null;
    }

    // ---------- 전체 UI 갱신 ----------
    void RefreshAllUI()
    {
        // 룸 잠금 최신화
        lockedSet = GetLockedSetFromRoom();

        // 하단 선택 스트립 상태 갱신
        foreach (var s in slots)
        {
            bool locked = IsLocked(s.data.id);
            bool selectedByMe = (mine != null && mine.id == s.data.id);
            s.Bind(s.data, OnClickSlot, locked && !selectedByMe, selectedByMe);
        }

        // 우측 팀 아이콘 갱신(옵션)
        if (teamIcons != null && teamIcons.Length > 0)
        {
            int i = 0;
            var players = PhotonNetwork.playerList;
            for (int k = 0; k < players.Length; k++)
            {
                var p = players[k];
                Sprite sp = null;

                if (p.CustomProperties != null && p.CustomProperties.ContainsKey(K_SELECTED))
                {
                    string val = p.CustomProperties[K_SELECTED] as string;
                    TitanId id;
                    if (!string.IsNullOrEmpty(val) && System.Enum.TryParse(val, out id))
                        sp = GetPickedSprite(id); // 여기서 "픽 전용" 스프라이트 사용
                }

                // 로컬 즉시 반영(동기화 지연 대비)
                if (p == PhotonNetwork.player && sp == null && mine != null)
                    sp = GetPickedSprite(mine.id);

                if (i < teamIcons.Length)
                {
                    teamIcons[i].sprite = sp;
                    teamIcons[i].enabled = (sp != null);
                }
                i++;
            }
            for (; i < teamIcons.Length; i++) teamIcons[i].enabled = false;
        }

        // 상단 팀바 갱신
        RefreshTeamBar();
    }

    // ---------- Photon 콜백 ----------
    void OnPhotonCustomRoomPropertiesChanged(Hashtable changed)
    {
        if (changed == null) return;
        if (changed.ContainsKey(K_LOCKED)) RefreshAllUI();
        if (changed.ContainsKey(K_TIMER_END)) UpdateCountdownFromRoom();
    }

    void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        RefreshAllUI();
        if (PhotonNetwork.isMasterClient) TryAutoStartWhenAllReady();
    }

    void OnPhotonPlayerPropertiesChanged(object playerObj, Hashtable changed)
    {
        RefreshAllUI();
        if (PhotonNetwork.isMasterClient) TryAutoStartWhenAllReady();
    }

    // ---------- 카운트다운 ----------
    void Update() { TickCountdown(); }

    void UpdateCountdownFromRoom()
    {
        if (PhotonNetwork.room != null && PhotonNetwork.room.CustomProperties != null)
        {
            var props = PhotonNetwork.room.CustomProperties;
            if (props.ContainsKey(K_TIMER_END))
            {
                object v = props[K_TIMER_END];
                if (v is long) _endTicks = (long)v;
                else if (v is double) _endTicks = (long)(double)v; // 일부 플랫폼 double 케이스
            }
        }
    }

    long _endTicks;
    void TickCountdown()
    {
        if (_endTicks == 0) return;
        var now = System.DateTime.UtcNow;
        var end = new System.DateTime(_endTicks, System.DateTimeKind.Utc);
        var remain = end - now;
        int sec = Mathf.Max(0, Mathf.CeilToInt((float)remain.TotalSeconds));
        if (countdownText != null) countdownText.text = sec.ToString();

        if (sec <= 0)
        {
            //  방이 없거나(오프라인) 마스터일 때 강제 시작
            if (PhotonNetwork.room == null || PhotonNetwork.isMasterClient)
                ForceFillAndStart();

            _endTicks = 0;
        }
    }

    // ---------- 팀바 ----------
    void BuildTeamBar()
    {
        if (!teamBarParent || !teamSlotPrefab) return;

        // 기존 슬롯 제거
        for (int i = teamBarParent.childCount - 1; i >= 0; i--)
            Destroy(teamBarParent.GetChild(i).gameObject);
        teamSlots.Clear();

        // 현재 인원 수만큼 슬롯 생성
        var players = PhotonNetwork.playerList;
        for (int i = 0; i < players.Length; i++)
        {
            var slot = Instantiate(teamSlotPrefab, teamBarParent);
            teamSlots.Add(slot);
        }
    }

    void RefreshTeamBar()
    {
        if (teamSlots.Count == 0) return;

        var players = PhotonNetwork.playerList;
        int count = Mathf.Min(players.Length, teamSlots.Count);

        for (int i = 0; i < count; i++)
        {
            var p = players[i];

            // 닉네임
            //string nick = !string.IsNullOrEmpty(p.NickName) ? p.NickName : p.name;
            string nick = p.NickName;

            // 선택된 캐릭터의 표시 이미지(픽 전용 적용)
            Sprite sp = null;
            if (p.CustomProperties != null && p.CustomProperties.ContainsKey(K_SELECTED))
            {
                var val = p.CustomProperties[K_SELECTED] as string;
                TitanId id;
                if (!string.IsNullOrEmpty(val) && System.Enum.TryParse(val, out id))
                    sp = GetPickedSprite(id);
            }

            // 준비 상태
            bool ready = false;
            if (p.CustomProperties != null && p.CustomProperties.ContainsKey(K_READY))
            {
                var r = p.CustomProperties[K_READY];
                if (r is bool) ready = (bool)r;
            }

            teamSlots[i].Set(nick, sp);
            teamSlots[i].SetReady(ready);
        }
    }


    // CharacterSelectManager 내부에 추가
    public void ResetSelectionUI()
    {
        // 1) 로컬 상태
        hovered = null;
        mine = null;

        // 2) 하단 슬롯 전부 "선택 안 됨 / 잠금 없음" 상태로 돌리기
        foreach (var s in slots)
        {
            if (s == null) continue;
            // 잠금은 일단 false, selected도 false
            s.Bind(s.data, OnClickSlot, false, false);
        }

        // 3) 중앙 프리뷰 비우기
        SetPreview(null);   // portrait / 이름 / 3D 프리뷰 전부 꺼짐:contentReference[oaicite:0]{index=0}

        // 4) 버튼 상태 초기화
        if (selectButtonText) selectButtonText.text = "SELECT";
        if (selectButton) selectButton.interactable = false;
    }



    // 인원 변동 시 팀바 재빌드
    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        BuildTeamBar();
        RefreshAllUI();
    }
    void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        BuildTeamBar();
        RefreshAllUI();
    }

    // 방에 성공적으로 들어왔을 때(클라이언트/마스터 공통)
    void OnJoinedRoom()
    {
        UpdateCountdownFromRoom();

        SyncUserSessionToPhoton();
        // 로그인 정보를 PhotonProperties로 올리기
        Hashtable ht = new Hashtable();
        ht["Nickname"] = UserSession.Nickname;
        ht["RankTitle"] = UserSession.RankTitle;
        ht["ClearCount"] = UserSession.ClearCount;

        ht["Kills"] = 0;
        ht["Deaths"] = 0;

        PhotonNetwork.player.SetCustomProperties(ht);
    }

    // 방을 내가 만들었을 때(마스터)
    void OnCreatedRoom()
    {
        UpdateCountdownFromRoom();
    }

    // ---------- 전원 준비시 자동 시작 ----------
    void TryAutoStartWhenAllReady()
    {
        var players = PhotonNetwork.playerList;
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
            if (p.CustomProperties == null) return;
            if (!p.CustomProperties.ContainsKey(K_READY) || !(bool)p.CustomProperties[K_READY]) return;
            if (!p.CustomProperties.ContainsKey(K_SELECTED) || p.CustomProperties[K_SELECTED] == null) return;
        }

        /* PhotonNetwork.LoadLevel("Game");*/ // 자동 씬 동기화가 켜져 있어야 함
                                              // (예: 초기화 스크립트에서 PhotonNetwork.automaticallySyncScene = true;)
        if (PhotonNetwork.isMasterClient)
            PhotonNetwork.LoadLevel("Main");
    }

    // ---------- 타이머 만료: 미선택자 자동 배정 후 시작 ----------
    // 타이머 종료 시: 미선택자는 남은 캐릭터 중 임의 배정 (씬 이동 없음)
    void ForceFillAndStart()
    {
        var taken = GetLockedSetFromRoom();
        var players = PhotonNetwork.playerList;

        // 오프라인(또는 룸 없음)일 때: 로컬 플레이어만 배정하고 끝
        if (!InOnlineRoom())
        {
            if (mine == null) // 내가 아직 미선택이면
            {
                TitanData assign = null;
                foreach (var t in titans)
                {
                    if (!taken.Contains(t.id.ToString())) { assign = t; break; }
                }
                if (assign != null)
                {
                    // 내 선택 바로 세팅 (로컬)
                    Hashtable props = new Hashtable { { K_SELECTED, assign.id.ToString() }, { K_READY, true } };
                    PhotonNetwork.player.SetCustomProperties(props);
                    taken.Add(assign.id.ToString());
                    SaveLockedSetToRoom(taken); // 로컬 lockedSet 갱신
                    RefreshAllUI();
                }
            }
            return; // 씬 이동도 없고 여기서 종료
        }

        // ===== 여기부터 온라인(룸 있음)일 때만 =====
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];

            bool hasSel = (p.CustomProperties != null &&
                           p.CustomProperties.ContainsKey(K_SELECTED) &&
                           p.CustomProperties[K_SELECTED] != null);

            if (!hasSel)
            {
                TitanData assign = null;
                foreach (var t in titans)
                {
                    if (!taken.Contains(t.id.ToString())) { assign = t; break; }
                }

                if (assign != null)
                {
                    // 각 클라가 자기 것을 직접 세팅하도록 해당 플레이어에게 RPC
                    photonView.RPC("RPC_ForceAssign", p, assign.id.ToString());
                    taken.Add(assign.id.ToString());
                }
            }
        }

        SaveLockedSetToRoom(taken);
        // 씬 이동은 요청하신 대로 하지 않음
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel("Main");
        }
    }


    bool InOnlineRoom()
    {
        return PhotonNetwork.room != null && PhotonNetwork.connected;
    }


    [PunRPC]
    void RPC_ForceAssign(string titanName)
    {
        // 로컬 플레이어가 자기 선택을 직접 세팅
        TitanId id;
        if (!System.Enum.TryParse(titanName, out id)) return;

        var props = new Hashtable
    {
        { K_SELECTED, id.ToString() },
        { K_READY, true }
    };
        PhotonNetwork.player.SetCustomProperties(props);
    }

    // CharacterSelectManager 내부 어딘가 (클래스 안 맨 아래 쪽에 추가)
    void SyncUserSessionToPhoton()
    {
        var ht = new Hashtable();

        ht["Nickname"] = UserSession.Nickname;
        ht["RankTitle"] = UserSession.RankTitle;
        ht["ClearCount"] = UserSession.ClearCount;

        // 킬/데스는 게임 시작 시 0으로 초기화
        ht["Kills"] = 0;
        ht["Deaths"] = 0;

        PhotonNetwork.player.SetCustomProperties(ht);
    }
}
