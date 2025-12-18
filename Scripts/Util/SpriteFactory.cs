using UnityEngine;

public class SpriteFactory : MonoBehaviour
{
    public static Sprite dotWhite, triWhite, circleWhite, allyIcon;

    void Awake()
    {
        if (!dotWhite) dotWhite = MakeCircle(32, 32, 1f);   // 가득 찬 원
        if (!circleWhite) circleWhite = MakeCircle(128, 128, 1f);  // 큰 원
        if (!triWhite) triWhite = MakeTriangle(64, 64);      // 위쪽 삼각형
        if (!allyIcon) allyIcon = dotWhite;                 // 임시로 흰 점 재사용
    }

    static Sprite MakeCircle(int w, int h, float fill = 1f)
    {
        var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color c = Color.white;
        Color t = new Color(1, 1, 1, 0);
        float rx = w * 0.5f, ry = h * 0.5f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float nx = (x - rx + 0.5f) / rx, ny = (y - ry + 0.5f) / ry;
                float r2 = nx * nx + ny * ny;
                tex.SetPixel(x, y, r2 <= fill * fill ? c : t);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    static Sprite MakeTriangle(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color c = Color.white;
        Color t = new Color(1, 1, 1, 0);
        // 위로 향하는 등변삼각형: y = |2x-1|  같은 방식으로 래스터라이즈
        for (int y = 0; y < h; y++)
        {
            float py = (float)y / (h - 1);
            float halfWidth = (1f - py) * 0.5f; // 위로 갈수록 폭 좁아짐
            for (int x = 0; x < w; x++)
            {
                float px = (float)x / (w - 1);
                bool inside = Mathf.Abs(px - 0.5f) <= halfWidth;
                tex.SetPixel(x, y, inside ? c : t);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 100f); // pivot (0.5,0)
    }
    public static Sprite MakeRadarTriangle(int size = 128, Color? color = null)
    {
        Color c = color ?? new Color(1f, 1f, 1f, 0.25f); // 흰색 반투명
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            // 밑변에서 꼭짓점으로 갈수록 폭 줄이기
            float t = (float)y / (size - 1);
            int halfWidth = Mathf.RoundToInt((1f - t) * size * 0.5f);
            for (int x = 0; x < size; x++)
            {
                if (x >= halfWidth && x < size - halfWidth)
                    tex.SetPixel(x, y, c);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }

        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f), 100f);
    }
}
