using UnityEngine;
using Photon;   // PUN1

public class MinimapTrackable : UnityEngine.MonoBehaviour
{
    public MinimapTeam team = MinimapTeam.Ally;
    public Sprite icon;

    MinimapController controller;
    PhotonView pv;

    [Header("Filter")]
    public bool usePlayerTagFilter = true;

    void Awake()
    {
        pv = GetComponent<PhotonView>(); // 없으면 null
        Debug.Log($"[Trackable] Awake() on {name}, PhotonView={(pv != null)}");
    }

    void OnEnable()
    {
        if (pv != null)
            team = pv.isMine ? MinimapTeam.Self : MinimapTeam.Ally;

        Debug.Log($"[Trackable] OnEnable() : {name}, team={team}");

        if (usePlayerTagFilter)
        {
            if (!CompareTag("Player") &&
                team != MinimapTeam.Self &&
                team != MinimapTeam.Ally)
            {
                Debug.Log($"[Trackable] {name} skipped (tag={tag}, team={team})");
                return;
            }
        }

        TryRegisterOrWait();
    }

    void OnDisable()
    {
        Debug.Log($"[Trackable] OnDisable() : {name}");

        if (controller != null)
            controller.Unregister(this);
    }

    void TryRegisterOrWait()
    {
        if (MinimapController.Instance != null)
        {
            controller = MinimapController.Instance;
            controller.Register(this);
            Debug.Log($"[Trackable] Register immediately : {name}");
        }
        else
        {
            Debug.Log($"[Trackable] MinimapController not ready, start wait coroutine : {name}");
            StartCoroutine(Co_WaitAndRegister());
        }
    }

    System.Collections.IEnumerator Co_WaitAndRegister()
    {
        while (MinimapController.Instance == null)
            yield return null;

        controller = MinimapController.Instance;
        controller.Register(this);
        Debug.Log($"[Trackable] Register after wait : {name}");
    }
}
