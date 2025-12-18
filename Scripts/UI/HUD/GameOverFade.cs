using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1.0f;     // 배경이 검게 변하는 시간

    Image bgImage;
    Color startColor;
    Color endColor;

    void Awake()
    {
        bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            // 현재 인스펙터 색(보통 흰색)을 시작 색으로 사용
            startColor = bgImage.color;
        }
    }

    void OnEnable()
    {
        // GameOverPanel이 SetActive(true) 되면 자동으로 페이드만 시작
        if (bgImage == null) return;

        StopAllCoroutines();
        StartCoroutine(Co_FadeOnly());
    }

    IEnumerator Co_FadeOnly()
    {
        startColor = bgImage.color;
        endColor = new Color(0f, 0f, 0f, startColor.a);   // 알파는 그대로 두고 RGB만 0으로

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;                  // Time.timeScale=0이어도 진행
            float p = Mathf.Clamp01(t / fadeDuration);
            bgImage.color = Color.Lerp(startColor, endColor, p);
            yield return null;
        }

        // ✅ 여기까지는 그냥 화면 어둡게만 만들고,  
        //    로비 이동/방 나가기는 전부 버튼 쪽(UIManager)에서 처리.
    }
}
