using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class QuickSlotsUI : MonoBehaviour
{
    [System.Serializable]
    public class QSlot
    {
        public ItemType type;      // Heal / Flare / Trap / Bomb
        public Image icon;         // 아이콘 이미지
        public TMP_Text countText; // 개수 텍스트(= 네가 연결한 "Count" TMP)
    }

    [Header("Refs")]
    public Inventory inventory;    // 비어있으면 런타임에 자동 바인딩 (아래 TryAutoBind 참고)
    public QSlot[] slots = new QSlot[4];

    // 1,2,3,4 키
    public KeyCode[] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    [Header("Options")]
    public bool hideCountWhenOne = true; // 1개일 때 숫자 숨기기

    float rebindTimer;

    void OnEnable()
    {
        TryAutoBind();
        Hook(true);
        Refresh();
    }

    void OnDisable() => Hook(false);

    void Start()
    {
        TryAutoBind();
    }
    void Update()
    {
        // 인벤토리 늦게 생기면 주기적으로 다시 바인딩
        if (!inventory)
        {
            rebindTimer += Time.unscaledDeltaTime;
            if (rebindTimer > 0.5f)
            {
                rebindTimer = 0f;
                TryAutoBind();
            }
            return;
        }

        // 인벤토리 열려 있으면 사용 입력 막기(원하면 주석처리)
        if (inventory.IsOpen) return;

        for (int i = 0; i < slots.Length && i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                var slot = slots[i];
                Debug.Log($"[QuickSlot] Key {keys[i]} 눌림 -> 타입 {slot.type} 사용 시도");
                Use(slot.type);
            }
        }
    }

    void Hook(bool add)
    {
        if (!inventory) return;
        if (add) inventory.OnChanged += Refresh;
        else inventory.OnChanged -= Refresh;
    }

    void TryAutoBind()
    {
        // var ps=PlayerStats.Local;
        // if (ps != null)
        // {
        //     var inv=ps.GetComponent<Inventory>();
        //     if (inv != null)
        //     {
        //         inventory=inv;
        //         Debug.Log("[QuickSlotsUI] 로컬 플레이어 인벤토리 바인딩 성공");
        //     }
        // }
        if (inventory) return;

        // PlayerStats.Local 방식 사용 중이라면:
        var ps = PlayerStats.Local;
        if (ps!=null)
        {
            // Fallback: 씬에서 로컬/첫번째 플레이어 찾기
            var all = FindObjectsOfType<PlayerStats>(false);
            foreach (var cand in all)
            {
                var pv = cand.GetComponent<PhotonView>();
                if (PhotonNetwork.offlineMode || (pv && pv.isMine))
                {
                    ps = cand;
                    break;
                }
            }
            if (!ps && all.Length > 0) ps = all[0];
        }

        if (ps)
        {
            var inv = ps.GetComponent<Inventory>() ?? FindObjectOfType<Inventory>(false);
            if (inv)
            {
                inventory = inv;
                Hook(true);
                Refresh();

                // 빌드에서도 보이게 UNITY_EDITOR 제거
                Debug.Log($"[QuickSlotsUI] Inventory 바인딩 완료 → {inventory.gameObject.name}");
            }
            else
            {
                Debug.LogWarning("[QuickSlotsUI] PlayerStats는 찾았는데 Inventory 컴포넌트를 못 찾음");
            }
        }
        else
        {
            Debug.LogWarning("[QuickSlotsUI] PlayerStats.Local 및 로컬 플레이어 탐색 실패");
        }
    }

    public void Refresh()
    {
        if (!inventory) return;

        foreach (var s in slots)
        {
            // 1) 해당 타입 아이템 "첫 스택" (사용 시 대상)
            var first = inventory.items.FirstOrDefault(st =>
                st != null && st.data && st.data.itemType == s.type);

            // 2) 해당 타입 "총합 개수" (여러 스택 합계)
            int total = inventory.items.Where(st =>
                st != null && st.data && st.data.itemType == s.type)
                .Sum(st => Mathf.Max(0, st.count));

            // 아이콘 설정
            if (first != null && first.data != null && first.data.icon != null)
            {
                s.icon.enabled = true;
                s.icon.sprite = first.data.icon;
                s.icon.preserveAspect = true;
            }
            else
            {
                s.icon.enabled = false;
                s.icon.sprite = null;
            }

            // 개수 텍스트 설정
            if (s.countText)
            {
                if (total <= 0)
                {
                    s.countText.text = "";
                }
                else if (hideCountWhenOne && total == 1)
                {
                    s.countText.text = "";   // 1개일 때 숨김(원하면 "1"로)
                }
                else
                {
                    s.countText.text = total.ToString();
                }
            }
        }
    }

    void Use(ItemType type)
    {
        if (!inventory)
        {
            Debug.LogWarning($"[QuickSlotsUI] Use({type}) 호출됐지만 inventory == null");
            return;
        }

        Debug.Log($"[QuickSlotsUI] Use({type}) → Inventory.UseByType 호출");
        bool ok = inventory.UseByType(type);

        if (ok)
        {
            Debug.Log($"[QuickSlotsUI] {type} 사용 성공, Refresh 호출");
            Refresh(); // 사용 후 바로 갱신
        }
        else
        {
            Debug.LogWarning($"[QuickSlotsUI] {type} 사용 실패 (해당 타입 아이템 없음?)");
        }
    }
}
