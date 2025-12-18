using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class InventoryDBSync : MonoBehaviour
{
    public Inventory inventory;
    private string baseUrl = "http://192.168.0.101/paradi"; // 네 서버 IP

    void Start()
    {
        // 바로 LoadInventory() 호출하지 말고, 한 프레임 기다렸다가 실행
        StartCoroutine(DelayedAutoLoad());
    }

    IEnumerator DelayedAutoLoad()
    {
        // 1프레임 정도 기다려서 Inventory / ItemDatabase / UI가 준비되도록 함
        yield return null;

        if (!inventory)
            inventory = FindObjectOfType<Inventory>(true);

        // ItemDatabase가 준비될 때까지 기다리기 (Awake 끝날 때까지)
        while (ItemDatabase.Instance == null)
            yield return null;

        // 유저 정보가 있는 경우에만 로드
        if (UserSession.UserId > 0)
        {
            Debug.Log("[InvDB] Main Scene → Auto Load (Delayed)");
            StartCoroutine(LoadInventory());
        }
    }


    // ==========================
    // SAVE
    // ==========================
    public void SaveNow()
    {
        // 기존: StartCoroutine(SaveInventory());
        StartCoroutine(SaveInventoryCoroutine());
    }

    //  UIManager에서 yield return 할 수 있는 공개 코루틴
    public IEnumerator SaveInventoryCoroutine()
    {
        yield return SaveInventory();
    }

    //  실제 저장 로직은 private 코루틴으로 분리
    IEnumerator SaveInventory()
    {
        if (UserSession.UserId <= 0)
        {
            Debug.LogWarning("[InvDB] UserSession.UserId 없음");
            yield break;
        }

        List<InventoryItemDTO> list = new();

        for (int i = 0; i < inventory.items.Count; i++)
        {
            var st = inventory.items[i];
            if (st == null || st.data == null || st.count <= 0)
                continue;

            list.Add(new InventoryItemDTO
            {
                slot_index = i,
                item_code = st.data.itemId,
                item_count = st.count
            });
        }

        InventorySavePayload payload = new()
        {
            userId = UserSession.UserId,
            items = list.ToArray()
        };

        string json = JsonUtility.ToJson(payload);

        WWWForm form = new WWWForm();
        form.AddField("payload", json);

        string url = baseUrl + "/save_inventory.php";
        UnityWebRequest www = UnityWebRequest.Post(url, form);

        Debug.Log("[InvDB] POST " + url + " payload=" + json);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[InvDB] Save Failed : " + www.error);
        }
        else
        {
            Debug.Log("[InvDB] Save Success : " + www.downloadHandler.text);
        }
    }


    // ==========================
    // LOAD
    // ==========================
    public void LoadNow()
    {
        StartCoroutine(LoadInventory());
    }

    IEnumerator LoadInventory()
    {
        string url = $"{baseUrl}/get_inventory.php?userId={UserSession.UserId}";
        Debug.Log("[InvDB] Load URL = " + url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[InvDB] Load Error : " + www.error);
            yield break;
        }

        string json = www.downloadHandler.text;
        Debug.Log("[InvDB] STEP1: Raw Response = " + json);

        InventoryLoadResponse resp = null;
        try
        {
            resp = JsonUtility.FromJson<InventoryLoadResponse>(json);
            Debug.Log("[InvDB] STEP2: FromJson OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[InvDB] JSON Parse Error : " + e.Message);
            yield break;
        }

        if (resp == null)
        {
            Debug.LogError("[InvDB] STEP3: resp == null");
            yield break;
        }

        if (!resp.success)
        {
            Debug.LogError("[InvDB] STEP4: resp.success == false : " + json);
            yield break;
        }

        if (resp.items == null)
        {
            Debug.LogError("[InvDB] STEP5: resp.items == null");
            yield break;
        }

        Debug.Log("[InvDB] STEP5: items.Length = " + resp.items.Length);

        // Inventory 찾기
        if (inventory == null)
            inventory = FindObjectOfType<Inventory>(true);

        if (inventory == null)
        {
            Debug.LogError("[InvDB] STEP6: Inventory 컴포넌트를 찾지 못함");
            yield break;
        }

        //  여기서 ItemDatabase 체크
        if (ItemDatabase.Instance == null)
        {
            Debug.LogError("[InvDB] STEP7: ItemDatabase.Instance == null (씬에 ItemDatabase 프리팹 있는지 확인)");
            yield break;
        }

        // 초기화
        Debug.Log("[InvDB] STEP8: 인벤토리 초기화 시작, slotCount=" + inventory.items.Count);
        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i] == null) continue;
            inventory.items[i].data = null;
            inventory.items[i].count = 0;
        }

        // 적용
        Debug.Log("[InvDB] STEP9: foreach 시작");
        foreach (var dto in resp.items)
        {
            Debug.Log($"[InvDB] DTO → code={dto.item_code}, slot={dto.slot_index}, count={dto.item_count}");

            var data = ItemDatabase.Instance.GetItemByStringID(dto.item_code);
            if (data == null)
            {
                Debug.LogWarning("[InvDB] ItemDatabase에서 못 찾는 item_code: " + dto.item_code);
                continue;
            }

            if (dto.slot_index < 0 || dto.slot_index >= inventory.items.Count)
            {
                Debug.LogWarning("[InvDB] slot_index 범위 밖: " + dto.slot_index);
                continue;
            }

            var slot = inventory.items[dto.slot_index];
            slot.data = data;
            slot.count = dto.item_count;

            Debug.Log($"[InvDB] Apply slot {dto.slot_index} → {data.itemId} x{dto.item_count}");
        }

        // UI 갱신
        Debug.Log("[InvDB] STEP10: inventory.RaiseChanged() 호출");
        inventory.RaiseChanged();   

        Debug.Log("[InvDB] Inventory Loaded 완료!");
    }
}