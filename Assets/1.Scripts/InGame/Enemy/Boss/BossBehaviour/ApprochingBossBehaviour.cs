using UnityEngine;
using Cysharp.Threading.Tasks;

public class ApprochingBossBehaviour : BossBehaviour
{
    public float distance = 6;
    public ApprochingBossBehaviour()
    {
        distance = 6;//6가까움 10충분히 범 
        behaviourName = "Approching";
    }
    public async override UniTask StartBehaviour()
    {
        while (Vector2.Distance(transform.position, Character.Instance.transform.position) > distance)
        {
            Debug.Log("TigerWraithBoss Approaching ToPlayer");
            Vector2Int[] dirs = Enemy.FindPath(transform.position, Character.Instance.transform.position);
            await boss.MoveTo(dirs[0], 1f);
        }
    }

}