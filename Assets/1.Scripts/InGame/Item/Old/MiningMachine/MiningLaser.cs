using UnityEngine;

public class MiningLaser : MiningMachine
{
    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    protected override void Update()
    {
        // 플레이어 위치 추적
        transform.position = Character.Instance.transform.position;

        base.Update();

        // 타겟 없으면 레이저 끄기
        if (targetStone == null)
            lineRenderer.enabled = false;
    }

    protected override void Attack(Stone ore)
    {
        ore.TakeDamage(new DamageData() { damage = attackPower });

        // 레이저 시각화
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, ore.transform.position);
    }
}
