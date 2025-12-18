using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MinimapPingNetwork : MonoBehaviour
{
    public static MinimapPingNetwork Instance;
    PhotonView pv;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Ping] 중복 MinimapPingNetwork 발견, 기존 것 유지 후 새 오브젝트 제거");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        pv = GetComponent<PhotonView>();

        Debug.Log($"[Ping] MinimapPingNetwork Awake. viewID={pv.viewID}, isSceneView={pv.isSceneView}");
    }

    public static void SendPing(Vector3 worldPos, float life = 2.5f)
    {
        // 🔹 Instance가 null이면 한 번 더 찾아본다 (빌드에서 Awake 순서 꼬인 경우 대비)
        if (Instance == null)
        {
            Instance = FindObjectOfType<MinimapPingNetwork>();
            if (Instance == null)
            {
                Debug.LogWarning("[Ping] SendPing 호출됐지만 MinimapPingNetwork.Instance가 없습니다.");
                return;
            }
        }

        Debug.Log($"[Ping] SendPing 호출 from={PhotonNetwork.player.NickName}, pos={worldPos}, inRoom={PhotonNetwork.inRoom}");

        if (!PhotonNetwork.inRoom)
        {
            // 오프라인/테스트 모드
            if (MinimapController.Instance != null)
            {
                MinimapController.Instance.CreatePing(worldPos, life);
            }
            else
            {
                Debug.LogWarning("[Ping] MinimapController.Instance == null (오프라인)");
            }
        }
        else
        {
            Instance.pv.RPC("RPC_Ping", PhotonTargets.All, worldPos, life);
        }
    }

    [PunRPC]
    void RPC_Ping(Vector3 pos, float life)
    {
        Debug.Log($"[Ping] RPC_Ping 수신 on={PhotonNetwork.player.NickName}, pos={pos}");

        if (MinimapController.Instance != null)
        {
            MinimapController.Instance.CreatePing(pos, life);
        }
        else
        {
            Debug.LogWarning("[Ping] RPC_Ping 수신했지만 MinimapController.Instance가 null 입니다.");
        }
    }
}
