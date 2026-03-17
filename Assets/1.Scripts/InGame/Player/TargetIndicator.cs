using UnityEngine;

public class TargetIndicator : MonoSingleton<TargetIndicator>
{
    public Transform target;
    public void SetTaret(Transform tr)
    {
        target = tr;
        gameObject.SetActive(true);
    }
    void Update()
    {
        if (target == null || !target.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            target = null;
            return;
        }

        float dis = Vector2.Distance(target.position, Player.Instance.transform.position);
        if (dis > Player.Instance.playerStatMgr.AttackRange)
        {

            gameObject.SetActive(false);
            target = null;
            return;
        }


        transform.position = target.position;
    }
}
