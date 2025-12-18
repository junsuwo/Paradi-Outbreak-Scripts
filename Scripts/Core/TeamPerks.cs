using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamPerksType
{
    ReviveAll,          //전체 팀원 부활
    TeamStatBuff,       //전체 팀스텟 버프
    WallEnforce         //벽 강화 및 회복
}
public class TeamPerks
{
    public void Apply(TeamPerksType type)
    {
        switch (type)
        {
            case TeamPerksType.ReviveAll:
                //PlayerManager.Instance.ReviveAllPlayers();
                break;

            case TeamPerksType.TeamStatBuff:
                //PlayerManager.Instance.BuffAllStats(0.1f);
                break;
            case TeamPerksType.WallEnforce:
                //WallManager.Instance.WallEnforce(0.1f);
                break;
        }
    }
}
