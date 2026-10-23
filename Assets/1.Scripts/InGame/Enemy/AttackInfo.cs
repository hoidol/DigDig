using System;
using UnityEngine;

// 공격 속성(AttackType) × 방어 타입(DefenceType) 피해 배율 표
// Resources/DefenceInfo 에셋 하나를 게임 전역에서 사용 (인스펙터에서 표 형태로 편집)
[CreateAssetMenu(fileName = "AttackInfo", menuName = "AttackInfo")]
public class AttackInfo : ScriptableObject
{   
    public static string Title(AttackType attackType)
    {
        switch (attackType)
        {
            case AttackType.Physic:
             return "물리형";
            case AttackType.Magic:
             return "마법형";
            case AttackType.Vibrational:
             return "진동형";
        }
        return "물리형";
    }

    // public static string Desc(AttackType attackType)
    // {
        
    //     switch (attackType)
    //     {
    //           case AttackType.Physic:
    //          return "물리";
    //         case AttackType.Magic:
    //          return "마법";
    //         case AttackType.Vibrational:
    //          return "진동";
    //     }
    //     return "일반형";
    // }
    public static Sprite GetSprite(AttackType aType)
    {
        return Resources.Load<Sprite>($"Icons/AttackType/{aType}");
    }
}
