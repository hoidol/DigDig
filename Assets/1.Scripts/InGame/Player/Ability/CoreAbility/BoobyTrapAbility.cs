using System.Collections;
using UnityEngine;

// 부비 트랩 - 특정 이동량마다 트랩 자동 설치
public class BoobyTrapAbility : Ability
{
    static readonly float[] triggerDistances = { 50f, 45f, 40f, 35f, 30f };

    public Trap trapPrefab;

    const int MAX_TRAPS = 2;
    int activeTrapCount;
    float traveledDistance;
    Vector2 lastPos;

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

            if (traveledDistance >= triggerDistances[count - 1])
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
        trap.Spawn(pos, count);
        activeTrapCount++;
        trap.onReleased += () => activeTrapCount--;
    }

    public override string GetDescription(int c = -1, bool detail = false)
    {
        if (c <= 0) c = count;
        return $"{triggerDistances[c - 1]}m 이동마다 트랩 자동 설치";
    }
}
