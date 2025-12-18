using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EssenceSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;

    int index;
    PlayerStats player;

    // 툴팁 콜백을 위해 InventoryUI 참조
    InventoryUI owner;

    // owner(InventoryUI)까지 받도록 확장
    public void Bind(PlayerStats p, int idx, InventoryUI inventoryUI = null)
    {
        player = p;
        index = idx;
        owner = inventoryUI;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        Refresh();
    }

    public void Set(ItemData data)
    {
        if (!icon) return;

        if (data && data.icon)
        {
            icon.enabled = true;
            icon.sprite = data.icon;

            // 크기 통일 보정
            var rt = icon.rectTransform;
            rt.sizeDelta = new Vector2(86, 86);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            icon.preserveAspect = true;
        }
        else
        {
            icon.enabled = false;
            icon.sprite = null;
        }
    }

    void Refresh()
    {
        ItemData data = (player && player.essence != null && index < player.essence.Length)
                        ? player.essence[index] : null;
        Set(data);
    }

    // ---------- 툴팁 ----------
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!owner) return;
        ItemData data = (player && player.essence != null && index < player.essence.Length)
                        ? player.essence[index] : null;
        if (data) owner.ShowEssenceTooltip(data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        owner?.ShowEssenceTotal();
    }
    // ---------- /툴팁 ----------
}
