using UnityEngine;
using UnityEngine.UI;

public class MinimapPing : MonoBehaviour
{
    float life;
    float t;
    Image img;
    RectTransform rt;
    Vector3 startScale, endScale;

    public void Init(float lifeTime)
    {
        life = lifeTime;
        img = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
        startScale = Vector3.one * 0.6f;
        endScale = Vector3.one * 1.6f;
        rt.localScale = startScale;
    }

    void Update()
    {
        t += Time.deltaTime;
        float p = Mathf.Clamp01(t / life);
        if (rt != null) rt.localScale = Vector3.Lerp(startScale, endScale, p);
        if (img != null) img.color = new Color(img.color.r, img.color.g, img.color.b, 1f - p);
        if (p >= 1f) Destroy(gameObject);
    }
}
