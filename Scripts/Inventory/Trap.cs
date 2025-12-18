using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * =====================================
 * [가시방벽 실제 오브젝트]
 * - 체력 있음
 * - 거인 공격 시 반사 피해
 * - 체력 0 시 파괴
 * =====================================
 */

public class Trap : MonoBehaviour
{
    public float maxHP = 1000f; //테스트용
    private float currentHP;

    private PhotonView pv;

    [Header("데미지 반사 설정")]
    public float reflectDamage = 50f;

    private bool isDestroyed = false;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        currentHP = maxHP;
    }

    public void TakeDamage(float damage,GameObject attacker)
    {
        if (isDestroyed) return;
#if PHOTON_UNITY_NETWORKING
        //온라인 & 룸 안일 때는 "마스터"가 책임지고 처리 
        if(PhotonNetwork.connected && PhotonNetwork.inRoom)
        {
            if(!PhotonNetwork.isMasterClient)
            {
                //마스터가 아닌 쪽에서 호출되면 그냥 무시 
                Debug.LogWarning("[Trap] TakeDamage는 마스터에서만 호출해야함 ");
                return;
            }

            int attackerViewId = 0;
            var attackerPv = attacker ? attacker.GetComponent<PhotonView>() : null;
            if(attackerPv)
                attackerViewId = attackerPv.viewID;

            //마스터에서 모든 클라로 HP 동기화
            pv.RPC("RPC_TakeDamage", PhotonTargets.AllBuffered, damage, attackerViewId);   
            return;
        } 
#endif  
            RPC_TakeDamage(damage, 0);             
    }

    [PunRPC]
    void RPC_TakeDamage(float damage,int attackerID)
    {
        if (isDestroyed) return;

        currentHP -= damage;
        if (currentHP <= 0f)
        {
            currentHP = 0f;
            OnDestroyed();
        }
        Debug.Log($"[Trap] {name} 이 {attackerID}에게 {damage}피해를 받음 . 남은 HP: {currentHP}");

#if PHOTON_UNITY_NETWORKING
         if(PhotonNetwork.connected && PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
            return;
#endif            
        //공격자에게 반사데미지
        PhotonView attackerPV = PhotonView.Find(attackerID);
        if(attackerPV != null)
        {
            //EnemyController 기반 데미지
            var ec = attackerPV.GetComponent<EnemyController>();
            if (ec != null)
            {
                ec.TakeReflectDamage(reflectDamage);
                return;
            }
        
        }
    }

    //Trap 파괴 (Photon 동기화)
    void OnDestroyed()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        Debug.Log($"[Trap] {name} 파괴됨!");
        //모든 플레이어에게 파괴 이벤트 전달
        pv.RPC("RPC_DestroyTrap", PhotonTargets.AllBuffered);
    }

    [PunRPC]
    void RPC_DestroyTrap()
    {
        if(gameObject != null)
        {
            Destroy(gameObject);
            Debug.Log("Trap 파괴됨(모든 클라이언트 동기화)");
        }    
    }
}