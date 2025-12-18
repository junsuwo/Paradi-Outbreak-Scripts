using UnityEngine;
using Photon;   

[RequireComponent(typeof(MinimapCameraFollow))]
public class MinimapCameraBinder : UnityEngine.MonoBehaviour
{
    MinimapCameraFollow follow;

    void Awake()
    {
        follow = GetComponent<MinimapCameraFollow>();
    }

    /// 외부에서 직접 지정하고 싶으면 호출
    public void SetTarget(Transform t)
    {
        follow.target = t;
        // Debug.Log("[Minimap] target bound (SetTarget): " + t.name);
    }

    void Update()
    {
        if (follow.target != null) return;

        // 1) Player 태그 + isMine 우선 탐색 (가장 안전)
        var tagged = GameObject.FindGameObjectsWithTag("Player");
        foreach (var go in tagged)
        {
            var pv = go.GetComponent<PhotonView>();
            if (pv != null && pv.isMine)
            {
                follow.target = go.transform;
                // Debug.Log("[Minimap] target bound (tag): " + go.name);
                return;
            }
        }

        // 2) 백업: 씬 전체에서 isMine && MinimapTrackable 있는 오브젝트
        var pvs = GameObject.FindObjectsOfType<PhotonView>();
        foreach (var pv in pvs)
        {
            if (pv != null && pv.isMine && pv.GetComponent<MinimapTrackable>() != null)
            {
                follow.target = pv.transform;
                // Debug.Log("[Minimap] target bound (fallback): " + pv.name);
                return;
            }
        }
    }
}
