using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EssenceType
{
    AttackEssence,      //진격의거인 정수
    ArmoredEssence,     //갑옷의거인 정수
    WarHammerEssence,   //전퇴의거인 정수
    ColossalEssence,    //초대형거인 정수
    BeastEssence,       //짐승거인 정수
    JawEssence,    //턱거인 정수
    CartEssence,        //차력거인 정수
    FemaleEssence,      //여성형거인 정수
    TheFoundingEssence  //시조의거인 정수
}
public class PersonalPerks
{
    public void Apply(PlayerController player,EssenceType type)
    {
        // switch (type)
        // {
        //     case EssenceType.AttackEssence:
        //         player.Stats.Attack *= 1.2f;
        //         break;

        //     case EssenceType.ArmoredEssence:
        //         player.Stats.Armor *= 1.2f;
        //         break;

        //     case EssenceType.WarHammerEssence:
        //         player.Stats.MaxHp *= 1.2f;
        //         break;

        //     case EssenceType.ColossalEssence:
        //         player.Stats.HpRecovery *= 1.2f;
        //         break;

        //     case EssenceType.BeastEssence:
        //         player.Stats.Attack *= 1.05f;
        //         player.Stats.Armor *= 1.05f;
        //         player.Stats.MaxHp *= 1.05f;
        //         player.Stats.HpRecovery *= 1.05f;
        //         break;

        //     case EssenceType.JawEssence:
        //         player.Stats.AtkSpeed *= 1.2f;
        //         break;

        //     case EssenceType.CartEssence:
        //         player.Stats.MoveSpeed *= 1.2f;
        //         break;

        //     case EssenceType.FemaleEssence:
        //         player.Stats.AtkSpeed *= 1.1f;
        //         player.Stats.MoveSpeed *= 1.1f;
        //         break;

        //     case EssenceType.TheFoundingEssence:
        //         player.Stats.Attack *= 1.1f;
        //         player.Stats.Armor *= 1.1f;
        //         player.Stats.MaxHp *= 1.1f;
        //         player.Stats.HpRecovery *= 1.1f;
        //         player.Stats.AtkSpeed *= 1.1f;
        //         player.Stats.MoveSpeed *= 1.1f;
        //         break;
        // }
    }
}
