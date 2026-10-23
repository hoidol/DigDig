using UnityEngine;

public class ReinforceSlotContainer : MonoBehaviour
{
    public ReinforceSlotType reinforceSlotType;
}

public enum ReinforceSlotType
{
    Target, //
    Buff,
    Value
}

public enum ReinforceTargetType //총 7개
{
    KingSlime, //1개
    Slime, //5개
    None, //꽝
}

public enum ReinforceBuffType
{
    AttackPower, //*5% *10%,*15% *20%
    AttackSpeed, //*5% *10%,*15%
    CritChance, //+2% +3%,+4% +5%
    CritPower, //+20%,+25% +30%
    MaxHp,
    None, //꽝
}
