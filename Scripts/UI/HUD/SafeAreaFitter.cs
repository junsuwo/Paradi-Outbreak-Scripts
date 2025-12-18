using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rt;
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt == null)
        {
            Debug.LogWarning("[SafeAreaFitter] RectTransform이 없습니다. UI 오브젝트에만 부착하세요.");
            return;
        }
        Apply();
    }

    void OnRectTransformDimensionsChange()
    {
        if (rt != null) Apply();
    }
        
    void Apply()
    {
        var sa = Screen.safeArea;
        var anchorMin = sa.position;
        var anchorMax = sa.position + sa.size;
        anchorMin.x /= Screen.width; anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width; anchorMax.y /= Screen.height;
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
