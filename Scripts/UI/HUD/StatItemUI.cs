using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text value;
    [Tooltip("AD/AP/ARM/MR/AS/MS/CRIT 등 키워드")]
    public string statKey;
}
