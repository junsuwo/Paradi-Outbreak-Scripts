using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomItem : BaseItem
{
    [Header("Bomb Prefab")]
    public GameObject bombPrefab;
    public GameObject bombGhostPrefab;

    [Header("설치 관련 설정")]
    //플레어어 앞쪽 거리
    public float placementDistance = 5f;
    //지면 판정
    public LayerMask groundLayer;

    private GameObject currentGhost;
    private PhotonView pv;
    private Transform playerTransform;
    private ItemManager itemManager;

    private void Start()
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

    private void Update()
    {
        if (!pv.isMine) return;

        //아이템 사용 (고스트 On/Off)
        if (Input.GetKeyDown(useKey))
        {
            var inv = Inventory.Instance;
            if (inv == null) {
                Debug.LogWarning("인벤토리 연결 안됨");
                return;
            }

            if (inv.FindFirstByType(ItemType.Bomb) != null)
            {
                inv.UseByType(ItemType.Bomb);
                ToggleGhost();
            }
        }
        //고스트 활성 중이면 위치 갱신
            if (isGhostActive && currentGhost != null)
            {
                UpdateGhostPosition();
                // F -> 설치
                if (Input.GetKeyDown(KeyCode.F))
                {
                    PlaceBomb();
                }
            }
    }

    //고스트 활성 / 비활성
    void ToggleGhost()
    {
        isGhostActive = !isGhostActive;

        if (isGhostActive)
        {
            currentGhost = Instantiate(bombGhostPrefab);

            //다른 아이템 고스트 자동 비활성화
            itemManager.NotifyGhostActive(this);
        }
        else
        {
            if (currentGhost != null) Destroy(currentGhost);
        }
    }

    //고스트 위치를 플레이어 앞쪽으로 갱신
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

    //폭탄 설치
    void PlaceBomb()
    {
        if (bombPrefab == null) return;

        Vector3 pos = currentGhost.transform.position;
        Quaternion rot = currentGhost.transform.rotation;

        GameObject bomb = PhotonNetwork.Instantiate(bombPrefab.name, pos, rot, 0);

        // ⭐ 설치자 연결
        var bombScript = bomb.GetComponent<BombExplosion>();
        var myView = GetComponent<PhotonView>();
        if (bombScript != null && myView != null)
        {
            bombScript.InitOwner(myView);
        }

        Destroy(currentGhost);
        isGhostActive = false;
    }

    public override void SetGhostVisible(bool isActive)
    {
        base.SetGhostVisible(isActive);

        if (!isActive && currentGhost != null)
            Destroy(currentGhost);
    }

    public override void Use()
    {
        ToggleGhost();
    }
}