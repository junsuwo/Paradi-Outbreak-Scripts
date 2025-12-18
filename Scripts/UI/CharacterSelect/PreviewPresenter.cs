using UnityEngine;

public class PreviewPresenter : MonoBehaviour
{
    public Transform pivot;        // 회전 중심
    public Camera previewCam;      // 캐릭터를 찍는 전용 카메라 (RenderTexture용)
    private GameObject current;    // 현재 보여지는 프리팹

    // 호환용: 예전처럼 offset 미전달 시 0으로 동작
    public void Show(GameObject prefab) => Show(prefab, Vector2.zero);

    // 데이터에서 받은 화면 오프셋 사용
    public void Show(GameObject prefab, Vector2 screenOffset)
    {
        // 기존 프리뷰 제거
        if (current) Destroy(current);
        if (!prefab) return;

        // 새 캐릭터 생성
        current = Instantiate(prefab, pivot);
        current.transform.localPosition = Vector3.zero;
        current.transform.localRotation = Quaternion.identity;

        // 전용 레이어로 변경 (Preview 전용 카메라만 보이게)
        SetLayerRecursively(current.transform, LayerMask.NameToLayer("Preview"));

        //  프레이밍: 모델 크기에 맞춰 카메라 거리/시선 자동조정
        if (previewCam)
        {
            var rends = current.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                // 모델 전체 크기를 감싸는 Bounds 계산
                Bounds b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);

                // 중심점과 거리 계산
                float radius = b.extents.magnitude;

                float fovRad = previewCam.fieldOfView * Mathf.Deg2Rad;
                float distByHeight = radius / Mathf.Sin(fovRad * 0.5f);
                float distByWidth = radius / Mathf.Sin(Mathf.Atan(Mathf.Tan(fovRad * 0.5f) * previewCam.aspect));
                float dist = Mathf.Max(distByHeight, distByWidth) * 0.7f; // 패딩(0.4~0.7 가감 가능)

                // 화면 오프셋 적용 (카메라의 right/up 기준, 모델 크기에 비례)
                Vector3 look = b.center
                             + previewCam.transform.right * (screenOffset.x * radius)
                             + previewCam.transform.up * (screenOffset.y * radius);

                // 카메라를 모델 정면으로 배치
                Vector3 dir = -previewCam.transform.forward;
                previewCam.transform.position = look + dir * dist;
                previewCam.transform.LookAt(look, Vector3.up);
            }
        }
    }

    private void SetLayerRecursively(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t) SetLayerRecursively(child, layer);
    }

    void Update()
    {
        // 회전 애니메이션
        if (pivot) pivot.Rotate(Vector3.up, 15f * Time.deltaTime);
    }
}
