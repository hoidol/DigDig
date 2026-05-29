using System.Collections;
using UnityEngine;

// 부비 트랩 - 35m 이동마다 트랩 자동 설치
public class BoobyTrapAbility : Ability
{
    const float TRIGGER_DISTANCE = 35f;
    const int MAX_TRAPS = 2;

    public Trap trapPrefab;

    int activeTrapCount;
    float traveledDistance;
    Vector2 lastPos;

    public override string GetDescription(bool detail = false)
    {
        return $"{TRIGGER_DISTANCE}m 이동마다 트랩 자동 설치";
    }

    public override void OnEquip(Player player)
    {
        traveledDistance = 0;
        lastPos = Player.Instance.transform.position;
        StartCoroutine(TrackDistance());
    }

    public override void OnUnequip(Player player)
    {
        StopAllCoroutines();
    }

    IEnumerator TrackDistance()
    {
        while (true)
        {
            yield return null;
            Vector2 curPos = Player.Instance.transform.position;
            traveledDistance += Vector2.Distance(curPos, lastPos);
            lastPos = curPos;

            if (traveledDistance >= TRIGGER_DISTANCE)
            {
                traveledDistance = 0;
                PlaceTrap();
            }
        }
    }

    void PlaceTrap()
    {
        if (trapPrefab == null) return;
        if (activeTrapCount >= MAX_TRAPS) return;

        Vector2 pos = Player.Instance.transform.position;
        var trap = Instantiate(trapPrefab, pos, Quaternion.identity);
        trap.Spawn(pos, 1);
        activeTrapCount++;
        trap.onReleased += () => activeTrapCount--;
    }
}
