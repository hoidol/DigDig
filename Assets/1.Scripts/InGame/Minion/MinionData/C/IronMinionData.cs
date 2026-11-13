using UnityEngine;


[CreateAssetMenu(fileName = "Iron", menuName = "MinionData/C/Iron")]
public class IronMinionData : EnhanceMinionData
{
    // 엘리트/보스에게 주는 데미지 배율 (방어 타입 배율, 거인 사냥 타일과 곱연산)
    public float[] eliteBossDamageMuls = {1.5f,1.75f,2,2.25f,2.5f};

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Iron.Desc"), eliteBossDamageMuls[mergeLv]);
    }
}
