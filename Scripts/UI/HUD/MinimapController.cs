using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MinimapTeam { Self, Ally, Enemy, Neutral }

public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }

    [Header("Ping Prefab")]
    public GameObject pingPrefab;   // 미니맵 핑 표시용 프리팹

    [Header("Refs")]
    public Camera minimapCam;
    public RectTransform maskRect;
    public RawImage mapTexture;
    public Image radarCone;
    public RectTransform alliesLayer;
    public RectTransform pingLayer;
    public RectTransform playerCursor;

    [Header("Icons/Sprite (optional)")]
    public Sprite allyIcon;
    public Sprite enemyDot;
    public Material additiveMat;

    [Header("FOV")]
    [Range(0f, 1f)] public float fovFill = 0.25f;
    public bool showRotatingSweep = true;
    public float sweepSpeed = 50f;

    [Header("Pooling/Prefab")]
    public GameObject smallIconPrefab;

    [Header("Policy")]
    public bool showEnemies = false; // 적 표시 X

    [Header("Ping")]
    public RectTransform pingTemplate;   // PingLayer 아래 PingTemplate
    public float defaultPingDuration = 5f;

    [Header("Rotation")]
    public float mapRotationOffset = 0f; // 미니맵 전체 회전 오프셋(도 단위, 시계방향 +)

    // Trackable ↔ Marker 를 1:1로 관리
    readonly Dictionary<MinimapTrackable, RectTransform> _markers = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Minimap] Duplicate MinimapController, destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[Minimap] MinimapController Awake()");

        if (!allyIcon && SpriteFactory.allyIcon) allyIcon = SpriteFactory.allyIcon;
        if (radarCone && radarCone.sprite == null && SpriteFactory.circleWhite)
            radarCone.sprite = SpriteFactory.circleWhite;
        if (playerCursor && playerCursor.GetComponent<Image>()?.sprite == null && SpriteFactory.triWhite)
            playerCursor.GetComponent<Image>().sprite = SpriteFactory.triWhite;
    }

    void Update()
    {
        if (!minimapCam || !maskRect)
            return;

        var follow = minimapCam.GetComponent<MinimapCameraFollow>();
        Transform target = follow ? follow.target : null;
        float yaw = 0f;

        if (target)
            yaw = target.eulerAngles.y;

        // ➜ 회전 오프셋 적용
        float uiYaw = yaw + mapRotationOffset;

        // 레이더 회전
        if (radarCone)
            radarCone.rectTransform.localEulerAngles = new Vector3(0, 0, -uiYaw);

        // 플레이어 커서(화살표)는 항상 중앙
        if (playerCursor)
        {
            playerCursor.anchoredPosition = Vector2.zero;
            playerCursor.localEulerAngles = new Vector3(0, 0, -uiYaw);
        }

        //  디버그: 프레임마다 찍으면 너무 많으니까 30프레임에 한 번씩만
        if (Time.frameCount % 100 == 0)
        {
            //Debug.Log($"[Mini] Update() frame={Time.frameCount}, markers={_markers.Count}");
        }

        // ⬇⬇ 이제 매개변수 없는 버전 사용
        UpdateMarkers();
    }

    /// <summary>
    /// Dictionary에 들어있는 모든 트래커의 마커 위치 갱신
    /// </summary>
    void UpdateMarkers()
    {
        if (_markers.Count == 0) return;
        if (!minimapCam || !maskRect) return;

        // 플레이어(자기 자신) Transform 가져오기
        var follow = minimapCam.GetComponent<MinimapCameraFollow>();
        Transform self = follow ? follow.target : null;
        if (!self)
        {
            Debug.LogWarning("[Mini] MinimapCameraFollow.target (self) is null.");
            return;
        }

        // 미니맵 카메라 파라미터
        float ortho = minimapCam.orthographicSize; // 세로 반지름 (world 단위)
        float aspect = minimapCam.aspect;          // 가로/세로 비

        Rect r = maskRect.rect;
        float halfW = r.width * 0.5f;
        float halfH = r.height * 0.5f;

        // ➜ 자기 회전 + 오프셋
        float selfYaw = self.eulerAngles.y + mapRotationOffset;

        // null 된 트래커 정리용
        var toRemove = new List<MinimapTrackable>();

        foreach (var kvp in _markers)
        {
            MinimapTrackable t = kvp.Key;
            RectTransform marker = kvp.Value;

            if (t == null || marker == null)
            {
                toRemove.Add(t);
                continue;
            }

            // 1) 플레이어 기준 상대 위치 (XZ 평면)
            Vector3 delta3 = t.transform.position - self.position;
            Vector2 flat = new Vector2(delta3.x, delta3.z);

            // 2) 미니맵 회전 보정
            if (follow != null && follow.rotateWithPlayer)
            {
                float rad = -selfYaw * Mathf.Deg2Rad;
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                flat = new Vector2(
                    flat.x * cos - flat.y * sin,
                    flat.x * sin + flat.y * cos
                );
            }

            // 3) 카메라 범위 기준 [-1, 1] 정규화
            Vector2 norm = new Vector2(
                flat.x / (ortho * aspect),
                flat.y / ortho
            );

            if (Mathf.Abs(norm.x) > 1f || Mathf.Abs(norm.y) > 1f)
            {
                if (marker.gameObject.activeSelf)
                    marker.gameObject.SetActive(false);
                continue;
            }

            if (!marker.gameObject.activeSelf)
                marker.gameObject.SetActive(true);

            Vector2 anchored = new Vector2(
                norm.x * halfW,
                norm.y * halfH
            );

            marker.anchoredPosition = anchored;
        }

        // 죽은 트래커/마커 정리
        if (toRemove.Count > 0)
        {
            foreach (var t in toRemove)
            {
                if (t != null && _markers.TryGetValue(t, out var marker) && marker != null)
                    Destroy(marker.gameObject);

                _markers.Remove(t);
            }
        }
    }

    RectTransform CreateMarker(MinimapTrackable t, RectTransform layer)
    {
        if (smallIconPrefab == null || layer == null)
        {
            Debug.LogError("[Minimap] smallIconPrefab or layer is null!");
            return null;
        }

        var go = Instantiate(smallIconPrefab, layer);
        go.name = $"MiniMarker_{t.name}";
        Debug.Log($"[Minimap] Marker created for {t.name}");

        var img = go.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = t.icon != null ? t.icon
                      : (allyIcon != null ? allyIcon : SpriteFactory.dotWhite);
            img.color = new Color(0.31f, 0.77f, 0.97f, 1f);
            if (additiveMat) img.material = additiveMat;
        }

        var rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogError("[Minimap] smallIconPrefab MUST have RectTransform + Image on root!");
            return null;
        }

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.anchoredPosition = Vector2.zero;

        return rt;
    }

    // === 외부 API ===
    public void Register(MinimapTrackable t)
    {
        if (!t) return;

        // 자기 자신(Self)은 마커 만들지 않고 커서(화살표)로만 표시
        if (t.team == MinimapTeam.Self)
        {
            Debug.Log($"[Minimap] Skip self marker : {t.name}");
            return;
        }

        // 적은 옵션에 따라 표시/미표시
        if (t.team == MinimapTeam.Enemy && !showEnemies)
        {
            Debug.Log($"[Minimap] Register skip enemy : {t.name}");
            return;
        }

        if (_markers.ContainsKey(t))
        {
            Debug.Log($"[Minimap] Already registered : {t.name}");
            return;
        }

        var rt = CreateMarker(t, alliesLayer);
        _markers.Add(t, rt);

        Debug.Log($"[Minimap] Register success : {t.name}, team={t.team}");
    }

    public void Unregister(MinimapTrackable t)
    {
        if (!t) return;

        if (_markers.TryGetValue(t, out var rt))
        {
            if (rt != null)
                Destroy(rt.gameObject);
            _markers.Remove(t);
            Debug.Log($"[Minimap] Unregister : {t.name}");
        }
    }

    // === Ping ===
    public void CreatePing(Vector3 worldPos, float life)
    {
        if (minimapCam == null || maskRect == null || pingLayer == null || pingPrefab == null)
        {
            Debug.LogWarning("[MiniPing] 세팅이 안 된 레퍼런스가 있습니다.");
            return;
        }

        // 1) 월드 → 뷰포트 (0~1)
        Vector3 vp = minimapCam.WorldToViewportPoint(worldPos);

        // 카메라 뒤에 있어도 어차피 “처음 찍은 위치” 기준으로만 처리
        Rect r = maskRect.rect;
        Vector2 pos = new Vector2(
            (vp.x - 0.5f) * r.width,
            (vp.y - 0.5f) * r.height
        );

        // 2) 원형 미니맵 테두리에 클램프
        float radius = Mathf.Min(r.width, r.height) * 0.5f;
        float edge = radius - 10f;
        float dist = pos.magnitude;
        if (dist > edge)
            pos = pos.normalized * edge;

        // 3) 핑 프리팹 생성 (한 번만 위치 설정)
        GameObject go = Instantiate(pingPrefab, pingLayer);
        var rtPing = go.GetComponent<RectTransform>();
        if (rtPing == null) rtPing = go.AddComponent<RectTransform>();

        rtPing.anchorMin = rtPing.anchorMax = rtPing.pivot = new Vector2(0.5f, 0.5f);
        rtPing.anchoredPosition = pos;
        rtPing.localScale = Vector3.one;

        go.SetActive(true);

        var pingComp = go.GetComponent<MinimapPing>();
        if (pingComp != null)
        {
            // life가 0 이하로 들어오면 최소값을 하나 정해준다 (안전장치)
            float lt = life > 0f ? life : defaultPingDuration;
            pingComp.Init(lt);
        }
        else
        {
            // 만약 prefab에 아직 스크립트 안 붙어있다면 경고
            Debug.LogWarning("[MiniPing] pingPrefab에 MinimapPing 컴포넌트가 없습니다.");
        }
    }

    // IEnumerator CoPingLife(GameObject go, float life)
    // {
    //     yield return new WaitForSeconds(life);
    //     if (go) Destroy(go);
    // }
}
