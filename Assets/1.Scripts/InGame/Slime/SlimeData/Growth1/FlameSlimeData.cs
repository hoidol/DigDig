using UnityEngine;


[CreateAssetMenu(fileName ="Flame",menuName ="Growth1/Flame")]
public class FlameSlimeData : EnhanceAbilitySlimeData
{
    public float burnDuration = 4;

    // FlameDPS는 EnhanceAbilitySlimeData.LoadData()가 SlimeStatType 이름과 같은 컬럼을 고유 스탯으로 자동 로드한다.
}
