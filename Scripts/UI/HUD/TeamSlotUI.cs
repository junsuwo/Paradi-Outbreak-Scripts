using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamSlotUI : MonoBehaviour
{
    public Image portrait;
    public Image hpBar;
   

    public void SetPortrait(Sprite sp)
    {
        if (!portrait) return;
        portrait.enabled = (sp != null);
        portrait.sprite = sp;
        portrait.preserveAspect = true;
    }

    public void SetHP(float hp, float max)
    {
        if (!hpBar) return;
        float fill = (max > 0) ? Mathf.Clamp01(hp / max) : 1f;
        hpBar.fillAmount = fill;
    }

}
