using System.Collections.Generic;
using UnityEngine;

// 곡괭이 관리자 - count만큼 Pickaxe를 맵에 생성/관리
public class PickaxeItem : Item
{
    [SerializeField] Pickaxe pickaxePrefab;

    readonly List<Pickaxe> pickaxes = new();
    readonly HashSet<OreStone> claimedTargets = new();

    public override void OnUnequip(Player player)
    {
        foreach (var p in pickaxes)
            if (p != null) Destroy(p.gameObject);
        pickaxes.Clear();
        claimedTargets.Clear();
    }

    public override void UpdateItem()
    {
        //float mul = multiples[Mathf.Clamp(count - 1, 0, multiples.Length - 1)];

        // count보다 많으면 제거
        while (pickaxes.Count > count)
        {
            int last = pickaxes.Count - 1;
            if (pickaxes[last] != null)
            {
                pickaxes[last].StopMining();
                Destroy(pickaxes[last].gameObject);
            }
            pickaxes.RemoveAt(last);
        }

        // count보다 적으면 추가
        while (pickaxes.Count < count)
        {
            var p = Instantiate(pickaxePrefab, Player.Instance.transform.position, Quaternion.identity);
            p.Init(claimedTargets);
            p.StartMining();
            pickaxes.Add(p);
        }

    }
}
