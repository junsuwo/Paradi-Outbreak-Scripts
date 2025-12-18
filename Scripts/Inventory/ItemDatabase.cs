using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [Header("등록할 아이템 리스트")]
    public List<ItemData> allItems = new List<ItemData>();

    private readonly Dictionary<string, ItemData> lookup = new Dictionary<string, ItemData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildDictionary();
    }

    void BuildDictionary()
    {
        lookup.Clear();
        foreach (var item in allItems)
        {
            if (item == null) continue;
            if (lookup.ContainsKey(item.itemId))
            {
                Debug.LogWarning($"[ItemDB] 중복된 ID 발견 : {item.itemId}");
                continue;
            }
            lookup[item.itemId] = item;
        }
        Debug.Log($"[ItemDB] 아이템 {lookup.Count}개 등록 완료");
    }

    public ItemData GetItemByStringID(string id)
    {
        if (lookup.TryGetValue(id, out var item))
            return item;

        Debug.LogWarning($"[ItemDB] ID '{id}'에 해당하는 아이템 없음");
        return null;
    }

    public void Refresh()
    {
        BuildDictionary();
    }
}
