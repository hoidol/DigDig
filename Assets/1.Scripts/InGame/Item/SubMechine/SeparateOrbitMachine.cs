using System.Collections.Generic;
using UnityEngine;

// OrbitMachine: 머신 하나를 회전시켜 자식 OrbitOrb 전체가 같이 공전
// SeparateOrbitMachine: OrbitOrb마다 회전축(pivot) 오브젝트를 따로 두고 pivot을 각각 회전
//   머신(this) → pivot(회전) → OrbitOrb(localPosition = (radius, 0))
//   → 오브마다 회전 속도를 따로 조절할 수 있음 (SetSpeedMul: 0이면 그 오브만 멈춤. ex) 방패가 적을 막는 동안 정지)
// 머신 자체는 회전하지 않는다. 피해 처리(OrbitOrb)와 사용하는 쪽 코드(AddOrbit/RemoveOrbitBullet/radius/rotationSpeed)는 OrbitMachine과 같음
public class SeparateOrbitMachine : OrbitMachine
{
    readonly List<Transform> pivots = new();      // orbitOrbs와 같은 순서
    readonly List<float> speedMuls = new();       // orbitOrbs와 같은 순서, 기본 1
    readonly Stack<Transform> pivotPool = new();

    Transform GetPivot()
    {
        Transform pivot = pivotPool.Count > 0 ? pivotPool.Pop() : new GameObject("OrbitPivot").transform;
        pivot.SetParent(transform, false);
        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;
        pivot.gameObject.SetActive(true);
        return pivot;
    }

    void ReturnPivot(Transform pivot)
    {
        pivot.gameObject.SetActive(false);
        pivotPool.Push(pivot);
    }

    // ── 추가 / 제거 ────────────────────────────────────────
    public override OrbitOrb AddOrbit()
    {
        OrbitOrb obj = GetFromPool();
        Transform pivot = GetPivot();
        obj.transform.SetParent(pivot, false);
        obj.transform.localRotation = Quaternion.identity;
        obj.Spawn(this);
        obj.damage = damage;

        orbitOrbs.Add(obj);
        pivots.Add(pivot);
        speedMuls.Add(1f);
        Sorting();
        return obj;
    }

    public override void RemoveOrbitBullet(OrbitOrb obj)
    {
        int index = orbitOrbs.IndexOf(obj);
        if (index < 0)
            return;

        orbitOrbs.RemoveAt(index);
        Transform pivot = pivots[index];
        pivots.RemoveAt(index);
        speedMuls.RemoveAt(index);

        obj.transform.SetParent(transform, false); // 풀에 둘 때는 머신 밑으로
        ReturnToPool(obj);
        ReturnPivot(pivot);
        Sorting();
    }

    // ── 개별 회전 속도 ─────────────────────────────────────
    // mul: rotationSpeed에 곱함 (0 = 정지, 1 = 기본)
    public void SetSpeedMul(OrbitOrb obj, float mul)
    {
        int index = orbitOrbs.IndexOf(obj);
        if (index >= 0)
            speedMuls[index] = mul;
    }

    public float GetSpeedMul(OrbitOrb obj)
    {
        int index = orbitOrbs.IndexOf(obj);
        return index >= 0 ? speedMuls[index] : 1f;
    }

    // ── Sorting ────────────────────────────────────────────
    // 첫 번째 pivot의 현재 각도 기준으로 나머지 pivot 각도를 균등 배치, 오브는 pivot에서 radius만큼 떨어짐
    protected override void Sorting()
    {
        int count = orbitOrbs.Count;
        if (count == 0) return;

        float baseAngle = pivots[0].localEulerAngles.z;
        float angleBetween = 360f / count;
        for (int i = 0; i < count; i++)
        {
            pivots[i].localRotation = Quaternion.Euler(0f, 0f, baseAngle + angleBetween * i);
            orbitOrbs[i].transform.localPosition = new Vector2(radius, 0f);
        }
    }

    // ── Update ─────────────────────────────────────────────
    // 머신은 그대로, pivot마다 각자 회전
    protected override void Update()
    {
        float delta = DirectionSign * rotationSpeed * Time.deltaTime;
        for (int i = 0; i < pivots.Count; i++)
            pivots[i].Rotate(0f, 0f, delta * speedMuls[i]);
        AccumulateRotation(delta); // 기준 회전 속도로 한 바퀴
    }
}
