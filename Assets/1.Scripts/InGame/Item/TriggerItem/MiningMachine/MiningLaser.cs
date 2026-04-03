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
        transform.position = Player.Instance.transform.position;

        base.Update();

        // 타겟 없으면 레이저 끄기
        if (targetOre == null)
            lineRenderer.enabled = false;
    }

    protected override void Attack(OreStone ore)
    {
        ore.TakeDamage(attackPower);

        // 레이저 시각화
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, ore.transform.position);
    }
}
