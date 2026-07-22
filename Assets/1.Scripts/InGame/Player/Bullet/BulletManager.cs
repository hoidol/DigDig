using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BulletManager : MonoSingleton<BulletManager>, ILoadData
{
    public Dictionary<string, BulletData> bulletDataDic;
    public BulletData[] bulletDatas;
    public MergeBulletData[] mergeBulletDatas;

    // public static Dictionary<string, Bullet> bullets;
    Dictionary<string, Stack<PlayerBulletObject>> bulletPool = new();
    Dictionary<string, PlayerBulletObject> bulletPrefabDic = new();


    public UniTask LoadTask { get; private set; }

    void Awake()
    {
        // bullets = new()
        // {
        //      //Level1 ------------------------
        //     { "Normal",    new NormalBullet() },
        //     { "Pierce",   new PierceBullet() },
        //     { "Condense", new CondenseBullet() },
        //     { "Giant",    new GiantBullet() },
        //     { "Iron",     new IronBullet() },
        //     { "Flame",    new FlameBullet() },
        //     { "Orbit",     new OrbitBullet() },
        //     { "Thunder",  new ThunderBullet() },
        //     { "Vampire",  new VampireBullet() },
        //     { "Split",        new SplitBullet() },
        //     { "Scatter",      new ScatterBullet() },

        //     //Level2 ------------------------

        //     { "CuttingRay",   new CuttingRayBullet() },
        //     { "Titan",        new TitanBullet() },
        //     { "SteelSphere",  new SteelSphereBullet() },
        //     { "LightningRod", new LightningRodBullet() },
        //     { "LavaShell",    new LavaShellBullet() },
        // };
        LoadTask = LoadDataAsync();
    }

    async UniTask LoadDataAsync()
    {
        await AddressableMgr.LoadAllByLabel<BulletData>("BulletData", (dates) =>
        {
            bulletDatas = dates;
            bulletDatas = bulletDatas.OrderBy(e => e.order).ToArray();

            bulletDataDic = new Dictionary<string, BulletData>();
            for (int i = 0; i < bulletDatas.Length; i++)
            {
                bulletDataDic.Add(bulletDatas[i].key, bulletDatas[i]);
            }

        });

        await AddressableMgr.LoadAllByLabel<MergeBulletData>("MergeBulletData", (dates) =>
        {
            mergeBulletDatas = dates;

        });

        await AddressableMgr.LoadAllByLabel<GameObject>("PlayerBulletObject", (dates) =>
        {
            for (int i = 0; i < dates.Length; i++)
            {
                var pbo = dates[i].GetComponent<PlayerBulletObject>();
                bulletPrefabDic.Add(pbo.key, pbo);
            }

        });
    }

    float apearMergeBulletChance = 10f;



    public PlayerBulletObject GetPlayerBulletObject(string key)
    {
        // if (!bulletPrefabDic.ContainsKey(key))
        //     bulletPrefabDic[key] = Resources.Load<PlayerBulletObject>($"PlayerBulletObject/{key}");

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

    // public static Bullet Create(string key)
    // {
    //     if (!bullets.TryGetValue(key, out var b))
    //         throw new ArgumentException($"BulletManager: 등록되지 않은 키 '{key}'");

    //     b.key = key;
    //     return b;
    // }

    // [SerializeField] List<BulletData> canPickBulletDatas = new List<BulletData>();
    // public BulletData DrawRandomBullet()
    // {
    //     return BulletData.GetBulletData(UserManager.Instance.userBulletManager.userBulletData.equiptedBullets[UnityEngine.Random.Range(0, 5)].key);
    // }

    // public List<BulletData> GetBulletDatas(int count)
    // {

    //     bool apearMergeBullet = UnityEngine.Random.Range(0f, 100f) < apearMergeBulletChance;
    //     if (apearMergeBullet)
    //         apearMergeBulletChance = 10f;
    //     else
    //         apearMergeBulletChance += 10f;

    //     var ownedMergeIngredientKeys = new HashSet<string>();

    //     // 가중치 풀 구성
    //     canPickBulletDatas.Clear();
    //     var pool = new List<BulletPickChance>();
    //     foreach (var data in bulletDataDic.Values)
    //     {
    //         float weight;
    //         if (apearMergeBullet)
    //         {
    //             weight = ownedMergeIngredientKeys.Contains(data.key) ? 100f : 10f;
    //         }
    //         else
    //         {
    //             weight = 10;
    //         }
    //         pool.Add(new BulletPickChance { bulletData = data, chance = weight });
    //         canPickBulletDatas.Add(data);
    //     }

    //     // 가중치 기반 비복원 추출
    //     var result = new List<BulletData>();
    //     int pickCount = Mathf.Min(count, pool.Count);
    //     bool hasUnique = false;
    //     for (int i = 0; i < pickCount; i++)
    //     {
    //         float total = 0f;
    //         for (int k = 0; k < pool.Count; k++) total += pool[k].chance;

    //         float roll = UnityEngine.Random.Range(0f, total);
    //         float cumulative = 0f;
    //         for (int j = 0; j < pool.Count; j++)
    //         {
    //             cumulative += pool[j].chance;
    //             if (roll < cumulative)
    //             {
    //                 result.Add(pool[j].bulletData);
    //                 pool.RemoveAt(j);
    //                 if (pool[j].bulletData.grade == Grade.Unique)
    //                     hasUnique = true;
    //                 break;
    //             }
    //         }
    //     }


    //     return result;
    // }

    public struct BulletPickChance
    {
        public BulletData bulletData;
        public float chance;
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
