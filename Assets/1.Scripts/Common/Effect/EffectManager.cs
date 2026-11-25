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



        Effect prefab = GetEffectPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[EffectManager] Resources/Effect에 {type} 이펙트 프리팹이 없음");
            return null;
        }
        Effect effect = Instantiate(prefab);
        effectDic[type].Add(effect);
        return effect;

    }
    public void Play(EffectType type, Vector2 pos)
    {
        Effect e = GetEffect(type);
        if (e != null)
            e.Play(pos);
    }
    // 위치에 튀어나오는 글씨 (Resources/Effect/PopTextEffect, 머지 "레벨업!"과 같은 연출). 글씨·색은 부를 때마다 지정
    public static void PlayPopText(Vector2 pos, string text, Color color)
    {
        if (Instance == null)
            return;
        Effect effect = Instance.GetEffect(EffectType.PopText);
        if (effect == null)
            return;
        TMPro.TMP_Text tmp = effect.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }
        effect.Play(pos);
    }

    // 자주 쓰는 글씨·색 프리셋 ("성공!" / "실패.."). 글씨 = 번역 key PopText_{종류}
    // 색은 회색 금지(밝은 타일 위에서 안 보임)
    public static readonly Color POP_TEXT_SUCCESS_COLOR = new Color(1f, 0.85f, 0.2f);
    public static readonly Color POP_TEXT_FAIL_COLOR = new Color(0.45f, 0.5f, 0.95f);
    public static readonly Color POP_TEXT_EVOLVE_COLOR = new Color(1f, 0.6f, 0.95f);

    public static Color GetPopTextColor(PopTextType type) => type switch
    {
        PopTextType.Success => POP_TEXT_SUCCESS_COLOR,
        PopTextType.Fail => POP_TEXT_FAIL_COLOR,
        PopTextType.Evolve => POP_TEXT_EVOLVE_COLOR,
        _ => Color.white,
    };

    public static void PlayPopText(Vector2 pos, PopTextType type)
    {
        PlayPopText(pos, TranslateManager.GetText($"PopText_{type}"), GetPopTextColor(type));
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
    // 프리팹에 번호로 저장되므로 항상 맨 뒤에만 추가할 것
    Hit, Explosion, StoneBreak, SmallExplosion, Thunder,
    PowerUp, // 미니언 스킬 강화(공속 버프 등) 동안 미니언을 따라다님 (Minion.PlayPowerUpEffect)
    Merge, // 미니언 머지 결과 위치 (MinionFieldController.Merge)
    Reinforce, // 인게임 강화(ReinforcePanel) 시 그 종 필드 미니언마다 "강화됨!" (Resources/Effect/ReinforcedEffect)
    Flame, // Flame 미니언 스킬 발동 시 미니언 위치 (Resources/Effect/FlameEffect, FlameMinion)
    // 다른 미니언에게 버프를 받는 동안 따라다님 (Buff.effectType, Minion.RefreshBuffEffects). 십자 버프 미니언(CrossBuffMinion)이 버프 스탯에 맞춰 지정
    AttackSpeedUp, AttackPowerUp, CritChanceUp,
    Stigma, // Stigma 미니언 스킬 발동 시 미니언 위치 (Resources/Effect/StigmaEffect, StigmaMinion)
    MortarExplosion, // Mortar 미니언 폭탄 착탄 지점 (Resources/Effect/MortarExplosion, MortarBulletObject.Explode)
    JokerCard, // Joker 미니언이 뽑은 카드 그림 + 글씨 (Resources/Effect/JokerCardEffect, JokerMinion.ShowCard)
    DarknessReady, // Darkness 미니언 스킬(변신)이 충전된 동안 몸 뒤 검은 연기 (Resources/Effect/DarknessReadyEffect, DarknessMinion)
    PopText, // 글씨만 튀어나오는 공용 이펙트 (Resources/Effect/PopTextEffect, EffectManager.PlayPopText. 다크니스 변환 성공/실패 등)
}
// EffectManager.PlayPopText 프리셋. 새 종류는 번역 key PopText_{종류} + GetPopTextColor에 추가
public enum PopTextType
{
    Success, // 성공!
    Fail,    // 실패..
    Evolve,  // 진화! (5성 진화, 2026-11-24)
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