using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoSingleton<BulletSpawner> {
    
    Dictionary<string, Stack<AllyBulletObject>> bulletPool = new();

    public AllyBulletObject GetBulletObject(string key)
    {
        // if (!bulletPrefabDic.ContainsKey(key))
        //     bulletPrefabDic[key] = Resources.Load<PlayerBulletObject>($"PlayerBulletObject/{key}");

        if (bulletPool.TryGetValue(key, out var stack) && stack.Count > 0)
        {
            var pooled = stack.Pop();
            pooled.gameObject.SetActive(true);
            return pooled;
        }

        return Instantiate(BulletManager.Instance.bulletPrefabDic[key]);
    }

    public void ReturnPlayerBulletObject(string key, AllyBulletObject obj)
    {
        if (!bulletPool.ContainsKey(key))
            bulletPool[key] = new Stack<AllyBulletObject>();

        obj.gameObject.SetActive(false);
        bulletPool[key].Push(obj);
    }


}