using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealItem : BaseItem
{
    public PlayerHealth playerHealth;

    private PhotonView pv;

    [Header("회복량 설정")]
    public int healAmount = 30;

    [Header("회복 파티클")]
    public GameObject healEffectPrefab;

    private void Start()
    {
        itemName = "회복약";
        useKey = KeyCode.Alpha1;
        pv = GetComponentInParent<PhotonView>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        //내 캐릭터일 때만 
        if (pv != null && pv.isMine)
        {
            if (Input.GetKeyDown(useKey))
            {
                var inv = Inventory.Instance;
                if (inv == null) return;

                // 아이템 보유 여부 확인
                if (inv.FindFirstByType(ItemType.Heal) != null)
                {
                    inv.UseByType(ItemType.Heal);    // 최종 실행
                    Use();
                }
                
            }
        }
    }

    public override void Use()
    {
        if (!pv.isMine)
            return;

        if (playerHealth == null)
        {
            Debug.LogWarning("[HealItem] PlayerHealth 컴포넌트를 찾지 못했습니다!");
            return;
        }

        if (playerHealth.GetCurrentHealth() >= playerHealth.maxHP)
        {
            Debug.Log("[HealItem] 현재 체력이 최대입니다. 회복약 사용 불가.");
            return;
        }
        else
        {
        //체력 회복
        playerHealth.Heal(healAmount);
        SFXManager.Instance.PlaySFX("CartSkill_Heal");
        Debug.Log("[HealItem] 체력 회복 + " + healAmount);
        }
        //다른 클라이언트에게 이펙트 표시
        pv.RPC("ShowHealEffect_RPC", PhotonTargets.Others, pv.viewID);

        //내 화면에도 표시
        ShowHealEffect(transform);
    }

    [PunRPC]
    void ShowHealEffect_RPC(int viewID)
    {
        PhotonView targetPV = PhotonView.Find(viewID);

        if (targetPV != null)
        {
            ShowHealEffect(targetPV.transform);
        }
    }

    private void ShowHealEffect(Transform target)
    {
        if (healEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                healEffectPrefab,
                target.position + Vector3.up * 1.5f,
                Quaternion.identity,
                target
                );

            var particle = effect.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                var main = particle.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
            }

            Destroy(effect, 2f);
        }
    }
}