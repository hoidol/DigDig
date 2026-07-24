using System.Collections.Generic;
using UnityEngine;

public class OrbitMachine : SubMachine
{
    public OrbitOrb orbitOrbPrefab;
    public float radius = 2f;
    public float rotationSpeed = 80f; //충분히느리게 

    public readonly List<OrbitOrb> orbitOrbs = new();
    readonly Queue<OrbitOrb> pool = new();

    // ── 풀링 ──────────────────────────────────────────────
    OrbitOrb GetFromPool()
    {
        if (pool.Count > 0)
        {
            OrbitOrb pooled = pool.Dequeue();
            pooled.gameObject.SetActive(true);
            return pooled;
        }
        return Instantiate(orbitOrbPrefab, transform);
    }

    void ReturnToPool(OrbitOrb obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    // ── 추가 / 제거 ────────────────────────────────────────
    public OrbitOrb AddOrbit()
    {
        OrbitOrb obj = GetFromPool();
        orbitOrbs.Add(obj);
        Sorting();
        return obj;
    }

    public void RemoveOrbitBullet(OrbitOrb obj)
    {
        orbitOrbs.Remove(obj);
        ReturnToPool(obj);
        Sorting();
    }

    // ── Sorting ────────────────────────────────────────────
    // 첫 번째 인덱스의 현재 회전 기준으로 나머지를 localPosition 균등 배치
    void Sorting()
    {
        int count = orbitOrbs.Count;
        if (count == 0) return;

        float angleBetween = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float rad = angleBetween * i * Mathf.Deg2Rad;
            orbitOrbs[i].transform.localPosition =
                new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        }
    }

    // ── Update ─────────────────────────────────────────────
    // 부모 오브젝트 자체를 회전 → 자식들이 자동으로 공전
    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
