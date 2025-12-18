using System.Collections;
using UnityEngine;

public class BombExplosion : MonoBehaviour
{
    //폭발까지의 시간
    public float explosionDelay = 2f;
    //폭발 반경
    public float explosionRadius = 5f;
    //폭발 데미지
    public float explosionDamage = 400f;
    //폭발 이펙트 프리팹
    public GameObject explosionEffectPrefab;

    private PhotonView pv;
    private bool isTriggered = false;  //Enmey가 닿았는지
    private bool exploded = false;

    static int _nextAttackId = 200000;   // 폭탄용 시작 번호
    // ⭐ 폭탄 설치자의 PhotonView 저장
    public PhotonView ownerView;

    public void InitOwner(PhotonView owner)
    {
        ownerView = owner;
        Debug.Log($"[BOMB] InitOwner: ownerViewId={ownerView?.viewID}");
    }
    private void Start()
    {
        pv = GetComponent<PhotonView>();

        //Collider가 Enemy를 막지않게 Trigger 설정
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        //Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if(rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    //Collider 감지
    private void OnTriggerEnter(Collider other)
    {
        if(isTriggered ||exploded) return;

            //Enemy 태그 감지
            if(other.CompareTag("Enemy"))
            {
                isTriggered = true;
                Debug.Log("Enemy 감지! 2초 후 폭발 대기 시작");
                StartCoroutine(ExplosionCountdown());
            }
        
    }

    IEnumerator ExplosionCountdown()
    {
        yield return new WaitForSeconds(explosionDelay);

        if(!exploded)
        {
            exploded = true;
            pv.RPC("RPC_Explode", PhotonTargets.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_Explode()
    {
        //폭발 이펙트
        if(explosionEffectPrefab != null)
        {
            var effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        SFXManager.Instance.PlaySFX("Bomb");
        int attackerViewId = ownerView != null ? ownerView.viewID : 0;
        //Enemy 타격
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            // 1) 먼저 Hurtbox 기준으로 처리 (킬 카운트용)
            var hb = hit.GetComponentInParent<Hurtbox>();
            if (hb != null)
            {
                int attackId = _nextAttackId++;   // ⭐ 몹마다 유니크 attackId
                Debug.Log($"[BOMB] Hurtbox hit {hit.name}, dmg={explosionDamage}, ownerViewId={attackerViewId}, attackId={attackId}");

                hb.ApplyAttackId(attackId, attackerViewId);
                continue;
            }

            // 2) Hurtbox 없으면 예전 로직으로도 한 번 더 시도
            var ec = hit.GetComponentInParent<EnemyController>();
            if (ec != null)
            {
                ec.ApplyDamage_Authoritative(explosionDamage);
                Debug.Log($"[BOMB] EnemyController에 {explosionDamage} 피해 적용 (fallback)");
                continue;
            }

            var enemy = hit.GetComponentInParent<Enemy_UseItem>();
            if (enemy != null)
            {
                enemy.TakeDamage(explosionDamage);
                Debug.Log($"[BOMB] Enemy_UseItem에도 {explosionDamage} 피해 적용 (fallback)");
            }
        }
        if(pv.isMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
