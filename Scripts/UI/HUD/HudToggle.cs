using UnityEngine;

public class HudToggle : MonoBehaviour
{
    public GameObject statsRoot;    // StatsPanel 오브젝트
    public KeyCode key = KeyCode.C; // 기본 C

    void Update()
    {
        if (Input.GetKeyDown(key) && statsRoot)
            statsRoot.SetActive(!statsRoot.activeSelf);
    }
}
