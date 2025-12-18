using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    ShopUI ui;
    string n, d;

    public void Bind(ShopUI owner, string nameText, string descText)
    {
        ui = owner; n = nameText; d = descText;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ui) ui.ShowTooltip(n, d);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ui) ui.ShowTooltip("", "");
    }
}
