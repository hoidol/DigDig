using UnityEngine;
using Cysharp.Threading.Tasks;

public class ExcutePatternBossBehaviour : BossBehaviour
{
    public ExcutePatternBossBehaviour()
    {
        behaviourName = "ExcutePattern";
    }
    public string patternName;
    public async override UniTask StartBehaviour()
    {
        var pattern = boss.curBossPhase.GetEnemyAttackPattern(patternName);
        if (pattern != null)
            await pattern.Execute(boss,null);
    }

}