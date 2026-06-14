using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoSingleton<BulletManager>
{
    public Dictionary<string, BulletData> bulletDataDic;
    public BulletData[] bulletDatas;
    //public List<BulletData> mergedBulletData = new List<BulletData>();
    public MergeBulletData[] mergeBulletDatas;

    public static Dictionary<string, Bullet> bullets;
    Dictionary<string, Stack<PlayerBulletObject>> bulletPool = new();
    Dictionary<string, PlayerBulletObject> bulletPrefabDic = new();

    public PlayerBulletObject GetPlayerBulletObject(string key)
    {
        if (!bulletPrefabDic.ContainsKey(key))
            bulletPrefabDic[key] = Resources.Load<PlayerBulletObject>($"PlayerBulletObject/{key}");

        if (bulletPool.TryGetValue(key, out var stack) && stack.Count > 0)
        {
            var pooled = stack.Pop();
            pooled.gameObject.SetActive(true);
            return pooled;
        }

        return Instantiate(bulletPrefabDic[key]);
    }

    public void ReturnPlayerBulletObject(string key, PlayerBulletObject obj)
    {
        if (!bulletPool.ContainsKey(key))
            bulletPool[key] = new Stack<PlayerBulletObject>();

        obj.gameObject.SetActive(false);
        bulletPool[key].Push(obj);
    }

    public static Bullet Create(string key)
    {
        if (!bullets.TryGetValue(key, out var b))
            throw new ArgumentException($"BulletManager: 등록되지 않은 키 '{key}'");

        b.key = key;
        return b;
    }


    private void Awake()
    {
        bulletDatas = Resources.LoadAll<BulletData>("BulletData");
        bulletDataDic = new Dictionary<string, BulletData>();
        for (int i = 0; i < bulletDatas.Length; i++)
        {
            bulletDataDic.Add(bulletDatas[i].key, bulletDatas[i]);
        }

        mergeBulletDatas = Resources.LoadAll<MergeBulletData>("MergeBulletData");

        bullets = new()
        {
            { "Normal",    new NormalBullet() },
            { "Pierce",   new PierceBullet() },
            { "Condense", new CondenseBullet() },
            { "Giant",    new GiantBullet() },
            { "Iron",     new IronBullet() },
            { "Flame",    new FlameBullet() },
            { "Orbit",     new OrbitBullet() },
            { "Thunder",  new ThunderBullet() },
            { "Vampire",  new VampireBullet() },
            { "Split",   new SplitBullet() },
            { "Scatter",   new ScatterBullet() },
        };
    }

    float apearMergeBulletChance = 10f;

    // Grade별 누적 가중치 (GetBulletDatas 호출마다 누적)

    const float normalDrawChange = 70;
    const float rareDrawChange = 25;
    const float uniqueDrawChange = 5;


    [SerializeField] List<BulletData> canPickBulletDatas = new List<BulletData>();
    public BulletData DrawRandomBullet()
    {
        return BulletData.GetBulletData(UserManager.Instance.userData.equiptedBullets[UnityEngine.Random.Range(0, 5)].key);
    }

    public List<BulletData> GetBulletDatas(int count)
    {

        bool apearMergeBullet = UnityEngine.Random.Range(0f, 100f) < apearMergeBulletChance;
        if (apearMergeBullet)
            apearMergeBulletChance = 10f;
        else
            apearMergeBulletChance += 10f;

        var ownedMergeIngredientKeys = new HashSet<string>();
        if (apearMergeBullet)
        {
            foreach (var bulletKey in Player.Instance.weapon.bulletInventory.curBullets)
            {
                Bullet bullet = bullets[bulletKey];
                if (bullet.bulletData == null || bullet.bulletData.mergeKeys == null) continue;
                foreach (var key in bullet.bulletData.mergeKeys)
                    ownedMergeIngredientKeys.Add(key);
            }
        }

        // 가중치 풀 구성
        canPickBulletDatas.Clear();
        var pool = new List<BulletPickChance>();
        foreach (var data in bulletDataDic.Values)
        {
            float weight;
            if (apearMergeBullet)
            {
                weight = ownedMergeIngredientKeys.Contains(data.key) ? 100f : 10f;
            }
            else
            {
                weight = 10;
                // weight = data.grade switch
                // {
                //     Grade.Normal => curNormalDrawChance + normalDrawBonus,
                //     Grade.Rare => curRareDrawChance + rareDrawBonus,
                //     Grade.Unique => curUniqueDrawChance + uniqueDrawBonus,
                //     _ => 10f
                // };
            }
            pool.Add(new BulletPickChance { bulletData = data, chance = weight });
            canPickBulletDatas.Add(data);
        }

        // 가중치 기반 비복원 추출
        var result = new List<BulletData>();
        int pickCount = Mathf.Min(count, pool.Count);
        bool hasUnique = false;
        for (int i = 0; i < pickCount; i++)
        {
            float total = 0f;
            for (int k = 0; k < pool.Count; k++) total += pool[k].chance;

            float roll = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;
            for (int j = 0; j < pool.Count; j++)
            {
                cumulative += pool[j].chance;
                if (roll < cumulative)
                {
                    result.Add(pool[j].bulletData);
                    pool.RemoveAt(j);
                    if (pool[j].bulletData.grade == Grade.Unique)
                        hasUnique = true;
                    break;
                }
            }
        }

        // if (hasUnique)
        // {
        //     normalDrawBonus -= 10f;
        //     rareDrawBonus -= 2f;
        //     uniqueDrawBonus += 1;

        //     if (normalDrawBonus < 30)
        //         normalDrawBonus = 30;
        //     if (rareDrawBonus < 5)
        //         rareDrawBonus = 5;
        // }
        // else
        // {
        //     normalDrawBonus = 0f;
        //     rareDrawBonus = 0f;
        //     uniqueDrawBonus = 0;
        // }

        return result;
    }

    public struct BulletPickChance
    {
        public BulletData bulletData;
        public float chance;
    }

    public void Merge(MergeBulletData mergeBulletData)
    {
        Player.Instance.weapon.ReleaseBullet(mergeBulletData.resourceBulletKeys[0]);
        Player.Instance.weapon.ReleaseBullet(mergeBulletData.resourceBulletKeys[1]);

        Player.Instance.weapon.AddBullet(mergeBulletData.resultBulletKey);
    }

    public BulletData GetBulletData(string key)
    {
        return bulletDataDic[key];
    }

    public MergeBulletData GetMergeBulletData(string resultKey)
    {
        foreach (var data in mergeBulletDatas)
        {
            if (data.resultBulletKey == resultKey)
                return data;
        }
        return null;
    }



}
