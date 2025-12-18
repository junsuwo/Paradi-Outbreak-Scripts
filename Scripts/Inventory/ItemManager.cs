using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Heal, Flare, Trap, Bomb }

public class ItemManager : MonoBehaviour,IGameSystem
{
    public static ItemManager Instance { get; private set; }
    [Header("보유 중인 아이템 리스트")]
    public List<BaseItem> items = new List<BaseItem>();

    public PlayerHealth playerHealth;

    public int healAmount = 30;
    [Header("Shop Catalog (서버/클라 공용)")]
    public List<ItemData> catalog = new List<ItemData>();
    private readonly Dictionary<string, ItemData> byId = new Dictionary<string, ItemData>();
    private Dictionary<ItemType, int> itemPrices = new()
    {
        {ItemType.Heal,1},
        {ItemType.Flare,2},
        {ItemType.Trap,3},
        {ItemType.Bomb,4}
    };

    private PhotonView pv;
    public Inventory inventory;              // 비워두면 자동 탐색
    private readonly HashSet<int> purchasedThisWaveActors = new(); // 서버 보호: 웨이브당 1회
    // 아이템 관리용
    public void Init() { }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        pv = pv ?? GetComponent<PhotonView>();
        if (!inventory) inventory = FindObjectOfType<Inventory>(true);

        byId.Clear();
        foreach (var i in catalog)
            if (i && !string.IsNullOrEmpty(i.itemId))
                byId[i.itemId] = i;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
            }
        }
    }
    
    public void ReleaseSystem() 
    {
        Debug.Log("아이템 시스템 초기화");
    }

    public void UpdateSystem()
    {
        Debug.Log("아이템 시스템 해제");
    }
    public bool TryBuyItem(ItemType type)
    {
        var player = PhotonNetwork.player;
        var tm = GameManager.Instance.teamResourceManager;

        int cost = itemPrices[type];
        bool success = tm.TryUsePersonalResource(player.NickName, cost);

        if (success)
        {
            //player.AddItem(type);
            Debug.Log($"{player}이 {type} 구매");
        }
        else
        {
            Debug.Log("개인 재화 부족");
        }
        return success;
    }
    public void GiveReward()
    {
        Debug.Log("개인 재화 지급");
    }

    //고스트가 켜질 때 호출
    public void NotifyGhostActive(BaseItem activeItem)
    {
        foreach (var item in items)
        {
            if (item == null) continue;

            //자신 제외하고 전부 고스트 비활성화
            if (item != activeItem)
            {
                item.SetGhostVisible(false);
            }
        }
    }

    public void TryBuy(ItemData data)
    {
        Debug.Log($"[SHOP] TryBuy: {data.itemId} | online={(PhotonNetwork.connected && PhotonNetwork.inRoom && !PhotonNetwork.offlineMode)} | pv={(GetComponent<PhotonView>()?.viewID)}");

        if (data == null) { Debug.LogWarning("[SHOP] data == null"); return; }

        bool online = PhotonNetwork.connected && PhotonNetwork.inRoom && !PhotonNetwork.offlineMode;

        if (online)
        {
            if (!pv) { Debug.LogError("[SHOP] PhotonView missing on ItemManager."); return; }
            int actor = (PhotonNetwork.player != null) ? PhotonNetwork.player.ID : -1;
            // 마스터에게 itemId만 보냄 (치트 방지: 마스터가 DB 재조회)
            pv.RPC("RPC_BuyRequest", PhotonTargets.MasterClient, actor, data.itemId);
        }
      
    }
    [PunRPC]
    void RPC_BuyRequest(int actorNumber, string itemId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient) return;
        pv = pv ?? GetComponent<PhotonView>();
        if (!pv) { Debug.LogError("[SHOP](master) pv null"); return; }

        // ★ 여기! DB 대신 byId 사용
        if (!byId.TryGetValue(itemId, out var item))
        {
            Debug.LogError("[SHOP](master) catalog에 itemId 없음: " + itemId);
            pv.RPC("RPC_BuyResult", info.sender, false, itemId);
            return;
        }

        var tm = GameManager.Instance ? GameManager.Instance.teamResourceManager : null;
        if (!tm) { pv.RPC("RPC_BuyResult", info.sender, false, itemId); return; }

        PhotonPlayer p = PhotonPlayer.Find(actorNumber);
        string key = (p != null && !string.IsNullOrEmpty(p.NickName)) ? p.NickName : actorNumber.ToString();

        if (!tm.TryUsePersonalResource(key, 1))
        {
            Debug.Log($"[SHOP](master) 코인 부족: {key}");
            pv.RPC("RPC_BuyResult", info.sender, false, itemId);
            return;
        }

        pv.RPC("RPC_BuyResult", info.sender, true, itemId);
        GameManager.Instance.teamResourceManager.GetComponent<PhotonView>()
    .RPC("RPC_SyncPersonalResource", info.sender, key, tm.GetPersonalResource(key));
        Debug.Log($"[SHOP](master) 승인 OK: actor={actorNumber}, item={item.displayName}");
    }

    [PunRPC]
    void RPC_BuyResult(bool success, string itemId)
    {
        Debug.Log($"[SHOP] BuyResult 수신: success={success}, itemId={itemId}");
        if (!success) return;

        var inv = inventory ? inventory : FindObjectOfType<Inventory>(true);
        if (!inv) { Debug.LogError("[SHOP] inventory null"); return; }

        // ★ catalog 우선
        ItemData item = null;
        byId.TryGetValue(itemId, out item);

        // 폴백: 혹시 DB가 있으면 사용
        if (!item && ItemDatabase.Instance)
            item = ItemDatabase.Instance.GetItemByStringID(itemId);

        if (!item) { Debug.LogError("[SHOP] item null on client: " + itemId); return; }

        bool added = inv.Add(item);
        Debug.Log($"[SHOP] Inventory.Add -> {added}");
        if (!added) return;

        var shop = FindObjectOfType<ShopUI>(true);
        if (shop) shop.SendMessage("MarkPurchasedThisWave", SendMessageOptions.DontRequireReceiver);
        if (shop) shop.RefreshHeader();

        var invUI = FindObjectOfType<InventoryUI>(true);


        Debug.Log($"[SHOP] 구매 완료(반영): {item.displayName}");
    }



    // === 웨이브 종료 시(외부 호출) 서버 보호 플래그 리셋 ===
    public void OnWaveEnd()
    {
        purchasedThisWaveActors.Clear();
    }
}
