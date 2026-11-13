using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 이번 판에 적용되는 권능 (로비에서 해금된 묶음마다 고른 것 전부, UserAuthorityManager.GetActiveAuthorityKeys)
// 게임 코드는 AuthoritySystem.Get<XxxAuthority>()로 켜져 있는지 확인하고 효과를 적용한다 (꺼져 있으면 null)
public static class AuthoritySystem
{
    static readonly Dictionary<Type, Authority> actives = new Dictionary<Type, Authority>();

    public static IEnumerable<Authority> Actives => actives.Values;

    public static T Get<T>() where T : Authority => actives.TryGetValue(typeof(T), out Authority r) ? (T)r : null;
    public static bool Has<T>() where T : Authority => actives.ContainsKey(typeof(T));

    public static Type GetAuthorityType(string key) => Type.GetType(key + "Authority");

    // 게임 시작 전 (GameManager.Awake). 이전 판의 권능은 정리
    public static void Begin(IEnumerable<string> keys)
    {
        End();
        foreach (string key in keys)
        {
            AuthorityData data = AuthorityManager.Instance.GetAuthorityData(key);
            Type type = GetAuthorityType(key);
            if (data == null || type == null || !typeof(Authority).IsAssignableFrom(type))
            {
                Debug.LogWarning($"[AuthoritySystem] 권능 {key} 데이터({data != null}) 또는 클래스({key}Authority) 없음");
                continue;
            }
            var authority = (Authority)Activator.CreateInstance(type);
            authority.data = data;
            actives[type] = authority;
        }
        foreach (Authority authority in actives.Values.ToList())
            authority.OnBegin();
        if (actives.Count > 0)
            Debug.Log($"[AuthoritySystem] 권능 {actives.Count}개: {string.Join(", ", actives.Values.Select(r => r.data.key))}");
    }

    public static void End()
    {
        foreach (Authority authority in actives.Values.ToList())
            authority.OnEnd();
        actives.Clear();
        Enemy.ClearFreezeAll();
    }

    // 무료 소환 (혼혈의 문장 등): 빈 칸·최대 슬라임 수 확인, 소환 가격(소환 횟수)은 올리지 않음
    public static Minion SpawnFreeMinion(string key, int lv = 0)
    {
        Tile tile = TileManager.Instance.GetRandomEmptyTile();
        if (tile == null || MinionFieldController.Instance.minionMaxCount <= MinionFieldController.Instance.CountedMinionCount)
            return null;
        Minion minion = KingSlime.Instance.AddMinion(key, lv);
        tile.SetTileObject(minion);
        GameManager.Instance.minionSpawnCount--; // AddMinion → SpawnMinionEvent로 오른 소환 횟수를 되돌림
        return minion;
    }

    // 장착 슬라임 5종 데이터 (조건형 권능: 순혈의 왕관·왕의 인장·혼혈의 문장)
    public static List<MinionData> EquippedMinionDatas()
    {
        return UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions
            .Where(s => s != null && s.MinionData != null)
            .Select(s => s.MinionData)
            .ToList();
    }
}
