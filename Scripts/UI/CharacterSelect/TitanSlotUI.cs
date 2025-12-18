using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitanSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;               // 버튼에 직접 있는 이미지
    public TMP_Text nameText;

    [HideInInspector] public TitanData data;

    private Button btn;
    private System.Action<TitanData> onClick;

    void Awake()
    {
        btn = GetComponent<Button>();

        // 만약 icon을 안 넣었다면, 자기 자신(Button)에 있는 Image 자동 참조
        if (icon == null)
            icon = GetComponent<Image>();
    }

    public void Bind(TitanData d, System.Action<TitanData> click, bool locked, bool selected)
    {
        data = d;
        onClick = click;

        if (icon != null)
            icon.sprite = d.icon;

        if (nameText != null)
            nameText.text = d.displayName;

        // 잠금 or 선택 색상 효과 (원하면 유지)
        Color c = Color.white;
        if (locked) c = Color.gray;
        else if (selected) c = Color.green;

        if (icon != null)
            icon.color = c;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke(d));
    }
}
