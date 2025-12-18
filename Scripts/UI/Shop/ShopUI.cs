using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button healBtn;
    public Button flareBtn;
    public Button trapBtn;
    public Button bombBtn;
    public Button closeBtn;

    [Header("Texts")]
    public TMP_Text coinText;   // 상단 "COIN n"
    public TMP_Text titleText;  // 중앙 위: 아이템 이름
    public TMP_Text descText;   // 중앙 아래: 아이템 설명

    [Header("ItemData (Inspector에서 연결)")]
    public ItemData healItem;   // 회복약
    public ItemData flareItem;  // 신호탄
    public ItemData trapItem;   // 함정
    public ItemData bombItem;   // 폭탄

    [Header("Inventory Ref")]
    [SerializeField] private Inventory inventory; // 비워두면 자동 찾음

    // 웨이브당 1회 구매 제한
    private bool purchasedThisWave = false;
    //private PhotonView pv;
    //private UIManager uiManager;
    [Header("Managers")]
    public ItemManager itemManager;

    TeamResourceManager TRM => GameManager.Instance.teamResourceManager;

    void Awake()
    {
       // pv = GetComponent<PhotonView>();
    }
    void Start()
    {
        if (!inventory) inventory = FindObjectOfType<Inventory>(true);


        if (!itemManager) itemManager = FindObjectOfType<ItemManager>(true);

        WireButton(healBtn, healItem);
        WireButton(flareBtn, flareItem);
        WireButton(trapBtn, trapItem);
        WireButton(bombBtn, bombItem);

        if (closeBtn) closeBtn.onClick.AddListener(GameManager.Instance.uiManager.CloseShopUI);

        ShowTooltip("", "");
        RefreshHeader();
    }

    void WireButton(Button btn, ItemData data)
    {
        if (!btn) return;

        // 클릭
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => TryBuy(data));

        // 호버(툴팁)
        var hover = btn.gameObject.GetComponent<ShopItemHover>();
        if (!hover) hover = btn.gameObject.AddComponent<ShopItemHover>();
        string n = data ? (string.IsNullOrEmpty(data.displayName) ? data.name : data.displayName) : "";
        string d = data ? data.description : "";
        hover.Bind(this, n, d);
    }

    // 로컬 플레이어 닉네임
    string LocalNick() => PhotonNetwork.player?.NickName ?? "local";

    public void RefreshHeader()
    {
        int coins = 0;
        if (TRM) coins = TRM.GetPersonalResource(LocalNick());   // 개인 재화 표시

        if (coinText) coinText.text = $"COIN  {coins}";

        bool canBuy = coins > 0 && !purchasedThisWave;
        SetInteractable(healBtn, canBuy && healItem != null);
        SetInteractable(flareBtn, canBuy && flareItem != null);
        SetInteractable(trapBtn, canBuy && trapItem != null);
        SetInteractable(bombBtn, canBuy && bombItem != null);
    }

    void SetInteractable(Button b, bool on)
    {
        if (b) b.interactable = on;
    }

    public void ShowTooltip(string nameText, string description)
    {
        if (titleText) titleText.text = nameText;
        if (descText) descText.text = description;
    }

    void TryBuy(ItemData data)
    {
        Debug.Log($"[SHOP:UI] click -> {data?.itemId}, manager={(itemManager ? itemManager.name : "null")}");

        if (!data) { Debug.LogWarning("[SHOP] ItemData가 비었습니다."); return; }
        if (!inventory) { Debug.LogWarning("[SHOP] Inventory 참조 없음"); return; } // 헤더 갱신/호버용
        if (purchasedThisWave) { Debug.Log("[SHOP] 이번 웨이브는 이미 구매"); return; }

        string nick = LocalNick();

        if (!itemManager)
        {
            itemManager = FindObjectOfType<ItemManager>(true);
            if (!itemManager)
            {
                Debug.LogError("[SHOP] ItemManager를 찾을 수 없습니다. (ShopPnl에 ItemManager 부착 필요)");
                return;
            }
        }

        // if (!TRM.TryUsePersonalResource(nick, 1))
        // {
        //     Debug.Log("[SHOP] 개인 재화 부족");
        //     RefreshHeader();
        //     return;
        // }
        // 네트워크 로직은 ItemManager가 전담
        itemManager.TryBuy(data);

        // purchasedThisWave = true;
        //RefreshHeader();
        //if (!data) { Debug.LogWarning("[SHOP] ItemData가 비었습니다."); return; }
        //if (!inventory) { Debug.LogWarning("[SHOP] Inventory 참조 없음"); return; }
        // if (purchasedThisWave) { Debug.Log("[SHOP] 이번 웨이브는 이미 구매"); return; }

        // string nick = LocalNick();
        //pv.RPC("RPC_TryBuy", PhotonTargets.MasterClient, nick, data.itemId);

        // // 개인 재화 1 소모
        // if (TRM == null || !TRM.TryUsePersonalResource(nick, 1))
        // {
        //     Debug.Log("[SHOP] 개인 코인 부족");
        //     RefreshHeader();
        //     return;
        // }

        // // 인벤토리 첫 빈 칸에 추가
        // if (!inventory.Add(data))
        // {
        //     // 실패 시 환불하고 싶으면 아래 한 줄을 해제:
        //     // TRM.AddPersonalResource(nick, 1);
        //     Debug.Log("[SHOP] 인벤토리 가득");
        //     RefreshHeader();
        //     return;
        // }

        // purchasedThisWave = true;
        // RefreshHeader();

        // var disp = string.IsNullOrEmpty(data.displayName) ? data.name : data.displayName;
        // Debug.Log($"[SHOP] 구매 완료: {disp}");
    }
    //[PunRPC]
    //void RPC_TryBuy(string buyer, string itemId, PhotonMessageInfo info)
    //{
    //    if (!PhotonNetwork.isMasterClient) return;

    //    ItemData item = ItemDatabase.Instance.GetItemByStringID(itemId);
    //    if (item == null) return;

    //    if (!TRM.TryUsePersonalResource(buyer, 1))
    //    {
    //        Debug.Log($"[SHOP] {buyer} 코인 부족");
    //        pv.RPC("RPC_BuyResult", info.sender, false, itemId);
    //        return;
    //    }

    //    pv.RPC("RPC_BuyResult", info.sender, true, itemId);
    //    Debug.Log($"[SHOP] {buyer} {item.displayName} 구매 승인");
    //}

    //[PunRPC]
    //void RPC_BuyResult(bool success,string itemId)
    //{
    //    if (!success)
    //    {
    //        RefreshHeader();
    //        return;
    //    }

    //    ItemData item = ItemDatabase.Instance.GetItemByStringID(itemId);
    //    if (!item) return;

    //    if (!inventory.Add(item))
    //    {
    //        Debug.Log("[SHOP] 인벤토리 가득 - 환불 안함");
    //        RefreshHeader();
    //        return;
    //    }

    //    purchasedThisWave = true;
    //    RefreshHeader();

    //    Debug.Log($"[SHOP] 구매 완료 : {item.displayName}");
    //}

    public void MarkPurchasedThisWave()
    {
        purchasedThisWave = true;
        RefreshHeader();
    }

    // 웨이브 종료 시(외부에서 호출)
    public void OnWaveEnd()
    {
        purchasedThisWave = false;  // 1회 제한 초기화만
        RefreshHeader();            // 코인 +1 지급은 TRM의 보상 로직에서 처리
    }

    public void OpenShop()
    {
        gameObject.SetActive(true);
        RefreshHeader();
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }
}
