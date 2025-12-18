using UnityEngine;

public class TrapItem_Photon : BaseItem
{
    [Header("Trap Prefabs")]
    public GameObject trapPrefab;        // PhotonNetwork.Instantiate 로 생성할 프리팹
    public GameObject trapGhostPrefab;   // 설치 미리보기용 고스트 프리팹

    [Header("설치 관련 설정")]
    public float placementDistance = 5f; // 플레이어 앞쪽 거리
    public LayerMask groundLayer;        // 지면 판정용

    private GameObject currentGhost;
    private PhotonView pv;
    private Transform playerTransform;
    private ItemManager itemManager;

    void Start()
    {
        if (pv == null)
        pv = GetComponent<PhotonView>();

        if (pv == null)
        {
            Debug.LogWarning("[TrapItem] PhotonView 없음, 네트워크 동작 비활성");
            enabled = false;
            return;
        }
        itemManager = ItemManager.Instance;
        playerTransform = transform;
        //itemManager = FindAnyObjectByType<ItemManager>();

        if (!itemManager.items.Contains(this))
            itemManager.items.Add(this);
    }

    void Update()
    {
        if (!pv.isMine) return;

        // 아이템 사용 (고스트 ON/OFF)
        if (Input.GetKeyDown(useKey)){
            var inv = Inventory.Instance;
            if (inv == null) return;

            if (inv.FindFirstByType(ItemType.Trap) != null)
            {
                inv.UseByType(ItemType.Trap);
                ToggleGhost();
            }
        }
        // 고스트 활성 중이면 위치 갱신
        if (isGhostActive && currentGhost != null)
        {
            UpdateGhostPosition();
            // F → 설치
            if (Input.GetKeyDown(KeyCode.F))
            {
                PlaceTrap();
            }
        }
    }

    // 고스트 활성/비활성
    void ToggleGhost()
    {
        isGhostActive = !isGhostActive;

        if (isGhostActive)
        {
            currentGhost = Instantiate(trapGhostPrefab);
            //다른 아이템 고스트 자동 비활성화
            itemManager.NotifyGhostActive(this);
        }
        else
        {
            if (currentGhost != null) Destroy(currentGhost);
        }
    }

    // 고스트 위치를 플레이어 앞쪽으로 갱신
    void UpdateGhostPosition()
    {
        Vector3 startPos = playerTransform.position + Vector3.up * 1f;
        Vector3 forward = playerTransform.forward;
        Ray ray = new Ray(startPos, forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f, groundLayer))
        {
            currentGhost.transform.position = hit.point + Vector3.up * 0.05f;
        }
        else
        {
            currentGhost.transform.position = playerTransform.position + forward * placementDistance;
        }

        currentGhost.transform.rotation = Quaternion.Euler(0, playerTransform.eulerAngles.y, 0);
    }

    // 트랩 설치 (PhotonNetwork로 모든 클라이언트에 동기화)
    void PlaceTrap()
    {
        //if (trapPrefab == null) return;

        //Vector3 pos = currentGhost.transform.position;
        //Quaternion rot = currentGhost.transform.rotation;

        //PhotonNetwork.Instantiate(trapPrefab.name, pos, rot, 0);

        //Destroy(currentGhost);
        //isGhostActive = false;

        if (currentGhost == null) return;

        Vector3 Pos = currentGhost.transform.position;
        Quaternion Rot = currentGhost.transform.rotation;

        GameObject trap = PhotonNetwork.Instantiate("Trap_WoodenSpike", Pos, Rot, 0);

        trap.tag = "Trap";

        Destroy(currentGhost);
        isGhostActive = false;
        currentGhost = null;

        Debug.Log($"{itemName} 설치 완료 (PhotonNetwork.Instantiate 호출됨");
    }

    public override void SetGhostVisible(bool isActive)
    {
        base.SetGhostVisible(isActive);

        if (!isActive && currentGhost != null)
            Destroy(currentGhost);
    }

    // BaseItem 필수 함수 오버라이드
    public override void Use()
    {
        ToggleGhost();
    }
}