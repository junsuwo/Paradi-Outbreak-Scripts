using System;

[Serializable]
public class PlayerResult
{
    public string playerName;
    public string rankTitle; 

    public int kills;
    public int deaths;
    public int wallHpLeft;
    public int clearCount;
    public bool isMvp;
}