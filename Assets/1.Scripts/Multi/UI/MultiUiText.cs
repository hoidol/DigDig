using System.Linq;

// (2026-11-27) 멀티 UI 글씨 (다른 유저 목록·보기·결과 공용)
public static class MultiUiText
{
    // "[불똥이 lv3][뭉치 lv1]" (최대 max개, 넘치면 +N)
    public static string Minions(string minions, int max = 8)
    {
        var list = MultiGame.ParseMinions(minions);
        if (list.Count == 0)
            return "-";
        string text = string.Concat(list.Take(max).Select(m => $"[{MinionName(m.key)} lv{m.lv}]"));
        return list.Count > max ? $"{text} +{list.Count - max}" : text;
    }

    public static string MinionName(string key)
    {
        MinionData data = MinionManager.Instance != null && MinionManager.Instance.minionDataDic.TryGetValue(key, out MinionData d) ? d : null;
        return data != null ? data.Title : key;
    }

    public static string HeroName(string heroKey)
    {
        if (string.IsNullOrEmpty(heroKey))
            return "";
        AbilityData card = HeroCollectionContainer.GetHeroCard(heroKey);
        return card != null && !string.IsNullOrEmpty(card.title) ? card.title : heroKey;
    }

    // "Lv.5 기사" / 용사 전 "Lv.0"
    public static string King(MultiPlayerState p)
    {
        string hero = HeroName(p.heroKey);
        return string.IsNullOrEmpty(hero) ? $"Lv.{p.kingLv}" : $"Lv.{p.kingLv} {hero}";
    }

    // (2026-11-27) "WAVE 12" (씬 StatusRow_Template 디자인에 맞춤)
    public static string Wave(MultiPlayerState p) => string.Format(TranslateManager.GetText("Multi_StatusWave"), p.wave);

    // 결과·목록 상태: "도착 12:34" / 그 외 "WAVE 30" (2026-11-27 탈락은 기록만 하고 화면엔 안 보임 → 진행 중과 같은 모양)
    public static string Status(MultiPlayerState p, long startAtMs)
    {
        if (p.HasReached)
            return string.Format(TranslateManager.GetText("Multi_StatusReached"), GameResultStats.FormatTime((p.reachedAt - startAtMs) / 1000f));
        return string.Format(TranslateManager.GetText("Multi_StatusWave"), p.wave);
    }
}
