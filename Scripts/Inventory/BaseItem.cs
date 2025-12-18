using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseItem : MonoBehaviour
{
    [Header("아이템 기본 정보")]
    //아이템 이름 
    public string itemName;
    //아이템 UI 아이콘
    public Sprite icon;
    //아이템 사용 키
    public KeyCode useKey;

    [HideInInspector]
    //고스트 활성 상태 추적
    public bool isGhostActive = false;

    //아이템 사용 동작 (각 아이템 스크립트에서 구현)
    public abstract void Use();

    public virtual void SetGhostVisible(bool isActive)
    {
        isGhostActive = isActive;
    }

}