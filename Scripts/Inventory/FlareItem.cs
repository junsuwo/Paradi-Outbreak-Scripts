using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//===========================================================
// [FlareItem]
// - 숫자 2 키를 눌러 사용
// - 로컬 플레이어만 사용 가능 (pv.isMine)
// - PhotonNetwork.Instantiate()로 모든 클라이언트에서 파티클 표시
// - 5초간 상승 후 폭발 파티클 생성
//===========================================================
public class FlareItem : BaseItem
{
    [Header("플레어 관련 프리팹 (Resources 폴더 내)")]
    public GameObject flarePrefab;          // 위로 올라가는 플레어
    public GameObject explosionPrefab;      // 터질 때 이펙트
    //public float flareLifeTime = 5f;        // 플레어 상승 시간
    public float riseSpeed = 5f;            // 상승 속도
    public float explosionHeight = 120f;     //폭발 높이

    private PhotonView pv;

    void Start()
    {
        itemName = "플레어건";
        useKey = KeyCode.Alpha2;
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        // 🔹 내 플레이어만 입력 감지
        if (pv != null && pv.isMine)
        {
            if (Input.GetKeyDown(useKey))
            {
                var inv = Inventory.Instance;
                if (inv == null) return;

                if (inv.FindFirstByType(ItemType.Flare) != null)
                {
                    inv.UseByType(ItemType.Flare);
                    Use();
                }
            }
        }
    }

    public override void Use()
    {
        // 🔹 로컬 플레이어만 실제로 Use 실행
        if (pv != null && !pv.isMine)
            return;

        if (flarePrefab == null)
        {
            Debug.LogError("[FlareItem] Flare Prefab이 비어있습니다!");
            return;
        }

        // 🔹 플레어 생성 (PhotonNetwork로 전체 표시)
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        Debug.Log("[FlareItem] 플레어 생성: " + spawnPos);
        GameObject flareObj = PhotonNetwork.Instantiate(flarePrefab.name, spawnPos, Quaternion.identity, 0);
        SFXManager.Instance.PlaySFX("Flare");
        // 🔹 상승 및 폭발 루틴 시작 (호스트만 실행)
        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(FlareRoutine(flareObj, spawnPos));
        }
    }

    private IEnumerator FlareRoutine(GameObject flareObj,Vector3 originPos)
    {
        float targetY = originPos.y + explosionHeight;

        while(flareObj != null && flareObj.transform.position.y < targetY)
        {
            flareObj.transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            yield return null;
        }


        // 🔹 일정 시간 후 폭발로 전환
        if (flareObj != null)
        {
            Vector3 explodePos = new Vector3(originPos.x, targetY, originPos.z);
            //PhotonNetwork.Destroy(flareObj);

            //if (explosionPrefab != null)
            //{
            //    GameObject explosion = PhotonNetwork.Instantiate(explosionPrefab.name, explodePos, Quaternion.identity, 0);
            //    Destroy(explosion, 3f);
            //}
            pv.RPC("SpawnExplosionRPC", PhotonTargets.AllBuffered, explodePos);

            PhotonNetwork.Destroy(flareObj);
        }
    }

    [PunRPC]
    void SpawnExplosionRPC(Vector3 pos)
    {
        if(explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, pos, Quaternion.identity);
            Destroy(explosion, 3f);
        }
    }
}
