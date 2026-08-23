using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MiniMeSpawner : MonoSingleton<MiniMeSpawner>
{
    readonly Dictionary<string, Stack<MiniMe>> pool = new(); // 적 종류 별 풀링

    [field: SerializeField] public int ActiveMiniMeCount { get; private set; }

    public readonly HashSet<MiniMe> activeminiMes = new();


    public MiniMe Instantiate(string key)
    {
        MiniMeData data = MiniMeManager.Instance.miniMeDataDic[key];

        if (!pool.ContainsKey(data.prefab.key))
            pool[data.prefab.key] = new Stack<MiniMe>();

        MiniMe miniMe = pool[data.prefab.key].Count > 0
            ? pool[data.prefab.key].Pop()
            : GameObject.Instantiate(data.prefab);


        miniMe.gameObject.SetActive(true);
        activeminiMes.Add(miniMe);
        ActiveMiniMeCount++;
        return miniMe;
    }

    void ReleaseMiniMe(MiniMe miniMe)
    {
        if (!pool.ContainsKey(miniMe.key))
            pool[miniMe.key] = new Stack<MiniMe>();

        activeminiMes.Remove(miniMe);
        miniMe.gameObject.SetActive(false);
        pool[miniMe.key].Push(miniMe);
        ActiveMiniMeCount = Mathf.Max(0, ActiveMiniMeCount - 1);
    }

}
