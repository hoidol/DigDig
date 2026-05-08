using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EffectManager : MonoSingleton<EffectManager>//, ILuckyMineListener
{
    public Effect[] effectPrefabs;
    Dictionary<EffectType, List<Effect>> effectDic = new Dictionary<EffectType, List<Effect>>();
    public Transform effectCanvasTr;
    public void Awake()
    {
        effectPrefabs = Resources.LoadAll<Effect>("Effect");
    }


    public Effect Instantiate(EffectType type)
    {
        return GetEffect(type);
    }


    public Effect GetEffect(EffectType type)
    {
        if (effectDic.ContainsKey(type))
        {
            for (int i = 0; i < effectDic[type].Count; i++)
            {
                if (effectDic[type][i] == null)
                {
                    effectDic[type].RemoveAt(i);
                    i--;
                    continue;
                }
                if (effectDic[type][i].gameObject.activeSelf)
                    continue;

                effectDic[type][i].gameObject.SetActive(true);
                return effectDic[type][i];
            }
        }
        else
        {
            effectDic.Add(type, new List<Effect>());
        }



        Effect effect = Instantiate(GetEffectPrefab(type));
        effectDic[type].Add(effect);
        return effect;

    }
    public void Play(EffectType type, Vector2 pos)
    {
        Effect e = GetEffect(type);
        e.Play(pos);
    }
    Effect GetEffectPrefab(EffectType effectType)
    {
        for (int i = 0; i < effectPrefabs.Length; i++)
        {
            if (effectPrefabs[i].effectType == effectType)
            {
                return effectPrefabs[i];
            }
        }
        return null;
    }




}

public enum EffectType
{
    Hit, Explosion, OreStoneBreak, SmallExplosion, Spark
}
public enum Icon
{
    Gold,
    Dia,
    LuckyCoin,
    Iron, Exp,
    IronMineTicket,
    GoldMineTicket

}