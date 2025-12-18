using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI 참조")]
    public Button click;          // 슬롯 클릭 버튼
    public Image icon;            // 아이템 아이콘
    public TMP_Text countText;    // 개수 표시
    public Image highlight;       // 선택 시 테두리/배경

    [Header("에센스 슬롯 여부")]
    [Tooltip("에센스 슬롯일 경우 체크 (EssenceTemplate에서 생성 시 자동 설정됨)")]
    public bool isEssenceSlot = false;

    private int index;
    private Inventory inv;

    // 슬롯 초기 바인딩
    public void Bind(Inventory inventory, int idx)
    {
        inv = inventory;
        index = idx;

        // 클릭 이벤트 연결
        if (click)
        {
            click.onClick.RemoveAllListeners();
            click.onClick.AddListener(() => inv.ClickSlot(index));
        }

        // 비활성 상태로 시작했더라도 활성화 (템플릿일 때도 안전)
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    // 슬롯 내용 업데이트
    public void Set(ItemStack st, bool selected)
    {
        // 아이템이 존재하면 아이콘/개수 표시
        if (st != null && st.data != null)
        {
            if (icon)
            {
                icon.enabled = true;
                icon.sprite = st.data.icon;
            }

            if (countText)
                countText.text = (st.data.stackable && st.count > 1) ? st.count.ToString() : "";
        }
        else
        {
            // 빈 칸이면 표시 숨김
            if (icon)
            {
                icon.enabled = false;
                icon.sprite = null;
            }
            if (countText)
                countText.text = "";
        }

        // 하이라이트 토글
        if (highlight)
            highlight.enabled = selected;
    }

    // 슬롯 타입 세팅 (템플릿 복제 시 자동 설정)
    public void MarkAsEssenceSlot(bool state)
    {
        isEssenceSlot = state;
    }
}
