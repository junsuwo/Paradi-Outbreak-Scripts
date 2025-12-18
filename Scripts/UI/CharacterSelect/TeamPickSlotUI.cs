using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamPickSlotUI : MonoBehaviour
{
    [Header("Refs (Inspector)")]
    public Image icon;           
    public TMP_Text nameText;     

   
    public void Set(string playerName, Sprite sp)
    {
        // 1) 전달값 로깅 (콘솔에서 확인)
        Debug.Log($"[TeamPickSlotUI:Set] name='{playerName}' sprite={(sp ? sp.name : "null")}", this);

        // 2) 이름 세팅
        if (nameText)
        {
            nameText.text = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
            nameText.enabled = true; // 혹시라도 꺼져있으면 켜줌
        }
        else
        {
            Debug.LogWarning("[TeamPickSlotUI] nameText가 인스펙터에 연결되어 있지 않아요.", this);
        }

        // 3) 아이콘 세팅
        if (icon)
        {
            icon.sprite = sp;
            icon.enabled = sp != null;
            // 아이콘이 클릭을 막지 않도록(선택) Raycast 끔
            icon.raycastTarget = false;
        }
        else
        {
            Debug.LogWarning("[TeamPickSlotUI] icon이 인스펙터에 연결되어 있지 않아요.", this);
        }
    }

    // 준비 상태에 따른 배경 색 (옵션)
    public void SetReady(bool ready)
    {
        var bg = GetComponent<Image>();
        if (bg)
            bg.color = ready
                ? new Color(0.3f, 0.7f, 1f, 0.25f)  // Ready(파란 하이라이트)
                : new Color(1f, 1f, 1f, 0.13f);     // 기본 흐린 배경
    }

   

    // 에디터에서 프리팹 열었을 때 자동으로 자식 찾아 연결(누락 방지)
    void OnValidate()
    {
        if (!nameText) nameText = GetComponentInChildren<TMP_Text>(true);
        if (!icon)
        {
            // 자식 중 "Icon" 이름을 우선 탐색
            var t = transform.Find("Icon");
            if (t) icon = t.GetComponent<Image>();
            if (!icon) icon = GetComponentInChildren<Image>(true);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("TEST/Show Dummy")]
    void __TestShowDummy()
    {
        // 에디터 우클릭 메뉴로 바로 확인용
        Set("Player", icon ? icon.sprite : null);
    }
#endif
}
