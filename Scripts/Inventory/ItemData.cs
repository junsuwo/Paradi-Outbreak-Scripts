using UnityEngine;

[CreateAssetMenu(menuName = "Game/ItemData", fileName = "Item_")]
public class ItemData : ScriptableObject
{
    public enum ItemKind { Consumable, Essence, Other }

    [Header("ID & Meta")]
    public string itemId;               
    public string displayName;           
    [TextArea] public string description;

    [Header("Visual")]
    public Sprite icon;

    [Header("Kind & Stack")]
    public ItemKind kind = ItemKind.Consumable;
    public bool stackable = true;
    public int maxStack = 99;

    [Header("효과 (스탯 보정치)")]
    public float addAttack;
    public float addDefense;
    public float addHP;
    public float addAttackSpeed;
    public float addMoveSpeed;
    public float addRegen;

    [Header("사용형 아이템")]
    public bool consumableUse;           // true면 Use 시 1개 소모
    public float healHPOnUse;            // 예: 포션이면 50 회복

    [Header("아이템 타입")]
    public ItemType itemType;   // Heal, Flare, Trap, Bomb

}
