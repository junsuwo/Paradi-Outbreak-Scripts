using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target / Settings")]
    public Transform target;
    public float height = 100f;
    public bool rotateWithPlayer = true;
    public float orthoSize = 80f;

    [Header("Rotation Offset")]
    [Tooltip("Y축 회전 오프셋 (시계 방향 +)")]
    public float yRotationOffset = 0f;   // ⬅️ 미니맵 카메라 Y축 회전 오프셋 추가

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = orthoSize;
        transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    void LateUpdate()
    {
        if (!target) return;

        // ===== 위치 따라가기 =====
        Vector3 p = target.position;
        transform.position = new Vector3(p.x, p.y + height, p.z);

        // ===== 회전 따라가기 =====
        if (rotateWithPlayer)
        {
            float yaw = target.eulerAngles.y + yRotationOffset;   // 오프셋 적용
            transform.rotation = Quaternion.Euler(90f, yaw, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, yRotationOffset, 0f);
        }
    }

    // ===== 줌 조절 =====
    public void SetZoom(float size)
    {
        orthoSize = Mathf.Clamp(size, 20f, 200f);
        if (cam)
            cam.orthographicSize = orthoSize;
    }
}
