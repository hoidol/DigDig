using UnityEngine;
using Cysharp.Threading.Tasks;

public class ApprochingAttackBossBehaviour : BossBehaviour
{
    public string patternName;
    public async override UniTask StartBehaviour()
    {
        while (Vector2.Distance(transform.position, Player.Instance.transform.position) > 6f)
        {
            Debug.Log("TigerWraithBoss Approaching ToPlayer");
            Vector2Int[] dirs = Enemy.FindPath(transform.position, Player.Instance.transform.position);
            await boss.MoveTo(dirs[0], 1f);
        }

        var sweepPattern = boss.curBossPhase.GetEnemySpecialAttackPattern(patternName);
        if (sweepPattern != null)
            await sweepPattern.Execute(boss, () => { });
    }

}