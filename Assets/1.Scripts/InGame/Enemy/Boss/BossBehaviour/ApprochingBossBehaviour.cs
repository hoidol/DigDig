using UnityEngine;
using Cysharp.Threading.Tasks;

public class ApprochingBossBehaviour : BossBehaviour
{
    public ApprochingBossBehaviour()
    {
        behaviourName = "Approching";
    }
    public string patternName;
    public async override UniTask StartBehaviour()
    {
        while (Vector2.Distance(transform.position, Player.Instance.transform.position) > 6f)
        {
            Debug.Log("TigerWraithBoss Approaching ToPlayer");
            Vector2Int[] dirs = Enemy.FindPath(transform.position, Player.Instance.transform.position);
            await boss.MoveTo(dirs[0], 1f);
        }

        var pattern = boss.curBossPhase.GetEnemySpecialAttackPattern(patternName);
        if (pattern != null)
            await pattern.Execute(boss, () => { });
    }

}