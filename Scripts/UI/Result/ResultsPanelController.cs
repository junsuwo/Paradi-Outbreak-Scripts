using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResultsPanelController : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform content;    // ScrollView/Viewport/Content
    public GameObject rowPrefab;     // ResultRow.prefab
    public TMP_Text titleText;       // "Victory" / "Wave X Clear" 등
    

    readonly List<GameObject> spawned = new();

    public void Show(string title, List<PlayerResult> results)
    {
        titleText.text = title;

        // 정렬: MVP 우선 → Kill 내림차순 → Death 오름차순
        var ordered = results
            .OrderByDescending(r => r.isMvp)
            .ThenByDescending(r => r.kills)
            .ThenBy(r => r.deaths)
            .ToList();

        ClearRows();

        foreach (var r in ordered)
        {
            var go = Instantiate(rowPrefab, content);
            go.GetComponent<ResultRowUI>().Set(r);
            spawned.Add(go);
        }

        gameObject.SetActive(true);
      
    }



    void ClearRows()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
    }

    System.Collections.IEnumerator Fade(float from, float to, float dur, System.Action onEnd = null)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
           
            yield return null;
        }
       
        onEnd?.Invoke();
    }
}