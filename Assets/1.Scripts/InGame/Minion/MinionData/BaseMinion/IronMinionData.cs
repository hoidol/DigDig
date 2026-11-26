using UnityEngine;


[CreateAssetMenu(fileName = "Iron", menuName = "MinionData/C/Iron")]
public class IronMinionData : MinionData
{
    // 엘리트/보스에게 주는 데미지 배율 (방어 타입 배율, 거인 사냥 타일과 곱연산)
    public float[] eliteBossDamageMuls = {1.5f,1.75f,2,2.25f,4f};
    // (2026-11-26) 일반 적 적중 시 스턴 확률(0~1)·시간(초), 머지 레벨별
    public float[] stunChances = {0.1f,0.15f,0.2f,0.25f,0.3f};
    public float[] stunTimes = {2f,2.25f,2.5f,2.75f,3f};

    public override string GetDescription(int mergeLv = 0)
    {
        return string.Format(TranslateManager.GetText("Minion.Iron.Desc"), eliteBossDamageMuls[mergeLv],
            Mathf.RoundToInt(stunChances[mergeLv] * 100f), stunTimes[mergeLv]);
    }
}
