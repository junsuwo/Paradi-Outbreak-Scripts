using UnityEngine;

public enum TitanId { Attack, Armored, Colossus, Female, Beast, Jaw, Cart, WarHammer, Founding }

[CreateAssetMenu(menuName = "Paradi/Titan Data")]
public class TitanData : ScriptableObject
{
    public TitanId id;
    public string displayName;
    public Sprite icon;          // 하단 스트립/상단 팀슬롯
    public Sprite portrait;      // 중앙 프리뷰(크게)
    public Color themeColor = Color.white; // 선택 하이라이트 등
    public GameObject previewPrefab;   // 중앙에서 보여줄 3D 프리팹(Idle 애니/포즈)

    public Vector2 previewOffset = Vector2.zero;
}