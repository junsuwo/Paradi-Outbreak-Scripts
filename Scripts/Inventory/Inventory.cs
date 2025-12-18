using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ItemStack
{
    public ItemData data;
    public int count;
    public bool IsEmpty => data == null || count <= 0;
    public ItemStack(ItemData d, int c) { data = d; count = c; }
}

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    [Header("UI 연결")]
    public GameObject inventoryPanel;
    public InventoryUI inventoryUI;

    [Header("인벤토리 설정")]
    [Min(1)] public int capacity = 40;
    public List<ItemStack> items = new List<ItemStack>();

    [Header("선택/상태")]
    [SerializeField, Tooltip("현재 선택된 슬롯 인덱스(디버그용)")]
    private int selectedIndex = -1;    // <-- 필드(Inspector에 표시됨)
    public int SelectedIndex => selectedIndex; // 읽기 전용 프로퍼티

    public Action OnChanged;           // 이건 직렬화되지 않으니 Header 없이 그대로

    [Header("옵션: 인벤토리 열릴 때 숨길 HUD 캔버스")]
    [Tooltip("HUD(슬라이더 등) 최상위 오브젝트")]
    public GameObject hudCanvas;

    [Header("옵션: 커서/시간 제어")]
    public bool controlCursor = true;
    public bool pauseTimeWhileOpen = false;

    public bool IsOpen => inventoryPanel && inventoryPanel.activeSelf;

    // (선택) 플레이어 참조 캐시
    private PlayerStats _player;

    public ItemStack FindFirstByType(ItemType t)
    {
        return items.FirstOrDefault(s => s != null && s.data != null && s.data.itemType == t);
    }


    public bool UseByType(ItemType t)
    {
        var st = FindFirstByType(t);
        if (st == null) return false;
        int index = items.IndexOf(st);
        if (index >= 0)
        {
            UseAt(index);
            return true;
        }
        return false;
    }

    void Awake()
    {
        Instance = this;
        EnsureCapacity();
        _player = FindAnyObjectByType<PlayerStats>();
    }

    void Start()
    {
        if (inventoryUI) inventoryUI.Bind(this);
        SetOpen(false);
        RaiseChanged();
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.I)) SetOpen(!IsOpen);
        

        // 인벤토리 열려있으면 퀵바 입력 무시
        if (IsOpen) return;

        //if (Input.GetKeyDown(KeyCode.Alpha1)) QuickUse(0);
        //if (Input.GetKeyDown(KeyCode.Alpha2)) QuickUse(1);
        //if (Input.GetKeyDown(KeyCode.Alpha3)) QuickUse(2);
        //if (Input.GetKeyDown(KeyCode.Alpha4)) QuickUse(3);
    }

    public bool TryMove(int from, int to)
    {
        if (from < 0 || from >= items.Count) return false;
        if (to < 0 || to >= items.Count) return false;
        if (from == to) return false;

        var temp = items[from];
        items[from] = items[to];
        items[to] = temp;

        Select(-1);  //  선택 초기화 (프로퍼티 setter 대신 Select 메서드!)
        RaiseChanged();
        return true;
    }

    public void ClickSlot(int index)
    {
        if (index < 0 || index >= items.Count) return;

        if (SelectedIndex == index) { Select(-1); return; }

        if (SelectedIndex == -1)
        {
            Select(index);
            return;
        }

        if (!TryMove(SelectedIndex, index))
            Select(index); // 이동 실패 시 새 선택
    }


    public void SetOpen(bool open)
    {
        if (inventoryPanel) inventoryPanel.SetActive(open);
        if (hudCanvas) hudCanvas.SetActive(!open);

        if (controlCursor)
        {
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible   = open;
        }
        if (pauseTimeWhileOpen)
            Time.timeScale = open ? 0f : 1f;

        // 열고 닫을 때 상세정보/선택 갱신
        if (open) inventoryUI?.RefreshAll();
    }

    public void QuickUse(int index) => UseAt(index);

    public void Select(int index)
    {
        if (index < 0 || index >= items.Count || items[index].IsEmpty)
            selectedIndex = -1;  
        else
            selectedIndex = index; 

        if (inventoryUI) inventoryUI.RefreshAll();

        inventoryUI?.RefreshDetail(selectedIndex);
    }

    public bool Add(ItemData data, int count = 1)
    {
        if (!data || count <= 0) return false;

        // 스택 합치기
        if (data.stackable)
        {
            for (int i = 0; i < items.Count && count > 0; i++)
            {
                var st = items[i];
                if (st.data == data && st.count < data.maxStack)
                {
                    int can = Mathf.Min(count, data.maxStack - st.count);
                    st.count += can;
                    count    -= can;
                }
            }
        }

        // 빈 칸 채우기
        for (int i = 0; i < items.Count && count > 0; i++)
        {
            if (items[i].IsEmpty)
            {
                int put = data.stackable ? Mathf.Min(count, data.maxStack) : 1;
                items[i].data  = data;
                items[i].count = put;
                count         -= put;
            }
        }

        RaiseChanged();
        return count <= 0;
    }

    public void RemoveAt(int index, int count = 1)
    {
        if (!InRange(index) || count <= 0) return;
        var st = items[index];
        if (st.IsEmpty) return;

        st.count -= count;
        if (st.count <= 0) ClearAt(index);

        RaiseChanged();
    }

    public void ClearAt(int index)
    {
        if (!InRange(index)) return;
        items[index].data  = null;
        items[index].count = 0;
        if (selectedIndex == index) Select(-1); 
    }

    public void UseAt(int index)
    {
        // 0) 인덱스 / 슬롯 체크
        if (!InRange(index))
        {
            Debug.LogWarning($"[Inv] UseAt({index}) : 인덱스 범위 밖");
            return;
        }

        var st = items[index];
        if (st == null || st.IsEmpty || st.data == null)
        {
            Debug.Log($"[Inv] UseAt({index}) : 비어있는 슬롯");
            return;
        }

        Debug.Log($"[Inv] UseAt({index}) : {st.data.name}, type={st.data.itemType}");

        // 1) "진짜 내" PlayerStats 찾기
        PlayerStats player = null;

        // 캐시된 _player가 있으면 우선 사용
        if (_player != null)
        {
            var pvCached = _player.GetComponentInParent<PhotonView>();
            if (PhotonNetwork.offlineMode || (pvCached && pvCached.isMine))
                player = _player;
        }

        // 없으면 새로 찾기
        if (player == null)
        {
            // 1순위: PlayerStats.Local 사용 중이면
            if (PlayerStats.Local != null)
            {
                player = PlayerStats.Local;
            }
            else
            {
                // 모든 PlayerStats 중에서 isMine 인 것 찾기
                var all = FindObjectsOfType<PlayerStats>(false);
                foreach (var cand in all)
                {
                    var pv = cand.GetComponentInParent<PhotonView>();
                    if (PhotonNetwork.offlineMode || (pv && pv.isMine))
                    {
                        player = cand;
                        break;
                    }
                }
            }

            _player = player; // 캐시
        }

        if (player == null)
        {
            Debug.LogWarning("[Inv] UseAt : 로컬 PlayerStats를 찾지 못했습니다.");
            return;
        }

        // 여기서부터는 무조건 "내" 플레이어
        var view = player.GetComponentInParent<PhotonView>();
        Debug.Log($"[Inv] UseAt({index}) : 실제 owner={view?.owner}");

        // 2) 기존 아이템 효과 적용
        player.ApplyUse(st.data);

        // 3) 신호탄이면 미니맵 핑 찍기 (네트워크 브로드캐스트)
        if (st.data.itemType == ItemType.Flare)
        {
            Vector3 pingPos = player.transform.position;
            Debug.Log($"[Inv] Flare 사용! SendPing 호출, pos={pingPos}");
            MinimapPingNetwork.SendPing(pingPos, 5f);
        }

        // 4) 소모형이면 개수 감소 처리
        if (st.data.consumableUse)
            RemoveAt(index, 1);
        else
            RaiseChanged();
    }



    public void RaiseChanged()
    {
        OnChanged?.Invoke();
        inventoryUI?.RefreshAll();
    }

    bool InRange(int i) => i >= 0 && i < items.Count;

    void EnsureCapacity()
    {
        if (capacity < 1) capacity = 1;
        if (items == null) items = new List<ItemStack>(capacity);

        if (items.Count < capacity)
            while (items.Count < capacity) items.Add(new ItemStack(null, 0));
        else if (items.Count > capacity)
            items.RemoveRange(capacity, items.Count - capacity);
    }



#if UNITY_EDITOR
    void OnValidate()
    {
        // 에디터에서 값 바뀔 때 리스트 길이 맞추기
        EnsureCapacity();
    }
#endif
}
