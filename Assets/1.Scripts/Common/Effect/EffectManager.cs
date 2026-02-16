using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoSingleton<EffectManager>//, ILuckyMineListener
{
    public Effect[] effectPrefabs;
    Dictionary<EffectType, List<Effect>> effectDic = new Dictionary<EffectType, List<Effect>>();
    public Transform effectCanvasTr;
    public void Awake()
    {
        effectPrefabs = Resources.LoadAll<Effect>("Effect");
    }


    public void Show(EffectType type, Vector3 point)
    {
        GetEffect(type).Show(point);
    }
    public void Show(EffectType type, Vector3 point, Color color)
    {
        GetEffect(type).Show(point, color);
    }

    public PoolingSystem pooingSystem;
    public void ShowBezierEffect(Vector3 startPoint, Vector3 endPoint, int count, Icon iconName)
    {
        Sprite icon = Resources.Load<Sprite>($"Icon/{iconName}");
        for (int i = 0; i < count; i++)
        {
            BezierImage bezierImage = pooingSystem.GetObject<BezierImage>();
            bezierImage.transform.parent = effectCanvasTr;
            bezierImage.transform.localScale = Vector3.one;
            bezierImage.image.sprite = icon;
            bezierImage.gameObject.SetActive(true);

            Bezier.Instance.MoveTo(bezierImage.transform, startPoint, endPoint, () =>
            {
                bezierImage.gameObject.SetActive(false);
            });
        }

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

    //public void StartLuckyMine()
    //{

    //}

    //public void EndLuckyMine()
    //{
    //    foreach(var effects in effectDic)
    //    {
    //        effects.Value.Clear();
    //    }
    //}


}

public enum EffectType
{
    Pick,
    Land,
    CannonExplosion,
    Demolish,
    StrongPick
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