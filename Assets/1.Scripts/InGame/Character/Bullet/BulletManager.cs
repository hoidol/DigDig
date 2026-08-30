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
    public Dictionary<string, AllyBulletObject> bulletPrefabDic = new();


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

        // await AddressableMgr.LoadAllByLabel<MergeBulletData>("MergeBulletData", (dates) =>
        // {
        //     mergeBulletDatas = dates;

        // });

        await AddressableMgr.LoadAllByLabel<GameObject>("CharacterBulletObject", (dates) =>
        {
            for (int i = 0; i < dates.Length; i++)
            {
                var pbo = dates[i].GetComponent<AllyBulletObject>();
                bulletPrefabDic.Add(pbo.key, pbo);
            }

        });
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
