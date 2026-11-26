using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-26) 로비 킹 관리 화면 [용사] 탭: HeroData.csv 용사를 3열 그리드로 (숨김 hidden 제외, CSV 순서)
// - 보유한 용사 = 누르면 선택 → 위쪽 킹슬라임 자리에 용사 정보(그림·이름·설명) + 능력치 칸이 그 용사 스탯으로 (KSManagementCanvas.ShowHero)
// - 선택한 용사를 다시 누르면 해제 → 킹슬라임 다시 보임. 보유하지 않은 용사는 눌러도 반응 없음 (어둡게 + "Lv.N")
// - 보유 = UserHeroManager (UserHero.own, 가격 없는 용사는 자동 보유, 가격 있는 용사는 상점 구매). 미보유 칸 = 해금 레벨 미달이면 "Lv.N", 아니면 가격
// - 그림·이름 = KingGrowthSetting.heroAbilities 중 그 key의 용사 카드(AbilityData thum·title)
// UI는 씬에 만들어 둠 (Tools/Lobby/Add Hero Tab To KSManagement). 칸은 씬의 slotTemplate 1개를 용사 수만큼 복사 → 칸 모양은 템플릿을 고치면 됨
public class HeroCollectionContainer : MonoBehaviour
{
    public RectTransform content;       // 칸이 놓일 곳 (GridLayoutGroup 3열)
    public HeroSlotPanel slotTemplate;  // 비우면 실행 중 기본 모양
    public const int COLUMN_COUNT = 3;

    public KSManagementCanvas owner;
    readonly List<HeroSlotPanel> slots = new List<HeroSlotPanel>();
    public string SelectedKey { get; private set; }

    public static bool IsOwned(HeroDataTable.HeroData hero, int userLevel) =>
        hero != null && !hero.hidden && UserDataManager.Instance != null && UserDataManager.Instance.userHeroManager.IsOwned(hero.key);

    public static int UserLevel => UserDataManager.Instance != null ? UserDataManager.Instance.userMyInfoManager.userMyInfoData.lv : 0;

    public static List<HeroDataTable.HeroData> ShownHeroes()
    {
        HeroDataTable table = HeroDataTable.Instance;
        return table != null ? table.heroDatas.Where(h => h != null && !h.hidden).ToList() : new List<HeroDataTable.HeroData>();
    }

    // 용사 카드 (그림·이름). 없으면 null
    public static AbilityData GetHeroCard(string key)
    {
        KingGrowthSetting setting = KingGrowthSetting.Instance;
        if (setting == null || setting.heroAbilities == null)
            return null;
        return setting.heroAbilities.FirstOrDefault(d => d != null && d.prefab is HeroKingAbility h && h.HeroDataKey == key);
    }

    public void OpenContainer()
    {
        Refresh();
    }

    public void Refresh()
    {
        List<HeroDataTable.HeroData> heroes = ShownHeroes();
        EnsureSlots(heroes.Count);
        int lv = UserLevel;
        if (SelectedKey != null && !heroes.Any(h => h.key == SelectedKey && IsOwned(h, lv)))
            SelectedKey = null;
        for (int i = 0; i < slots.Count; i++)
        {
            bool active = i < heroes.Count;
            slots[i].gameObject.SetActive(active);
            if (active)
                slots[i].Set(this, heroes[i], IsOwned(heroes[i], lv), heroes[i].key == SelectedKey);
        }
        UpdateCellSize();
    }

    public void OnClickedSlot(HeroDataTable.HeroData hero)
    {
        if (hero == null || !IsOwned(hero, UserLevel))
            return; // 보유하지 않은 용사는 반응 없음
        SoundManager.Play(SFXType.TabChange);
        SelectedKey = SelectedKey == hero.key ? null : hero.key; // 다시 누르면 해제
        Refresh();
        if (owner != null)
            owner.ShowHero(SelectedKey != null ? hero : null);
    }

    public void ClearSelection()
    {
        if (SelectedKey == null)
            return;
        SelectedKey = null;
        Refresh();
        if (owner != null)
            owner.ShowHero(null);
    }

    void EnsureSlots(int count)
    {
        if (slotTemplate == null)
        {
            Debug.LogWarning("[HeroCollectionContainer] 용사 칸 템플릿(slotTemplate) 연결 없음 → 메뉴 Tools/Lobby/Add Hero Tab To KSManagement");
            return;
        }
        while (slots.Count < count)
        {
            HeroSlotPanel slot = Instantiate(slotTemplate, content);
            slot.gameObject.name = $"HeroSlot_{slots.Count}";
            slots.Add(slot);
        }
        if (slotTemplate != null && slotTemplate.gameObject.scene.IsValid())
            slotTemplate.gameObject.SetActive(false);
    }

    // 3열에 맞게 칸 크기 (정사각형에 이름 줄)
    void UpdateCellSize()
    {
        GridLayoutGroup grid = content != null ? content.GetComponent<GridLayoutGroup>() : null;
        if (grid == null)
            return;
        Canvas.ForceUpdateCanvases();
        float width = content.rect.width;
        if (width <= 0f && content.parent is RectTransform p)
            width = p.rect.width;
        if (width <= 0f)
            return;
        float cell = (width - grid.padding.left - grid.padding.right - grid.spacing.x * (COLUMN_COUNT - 1)) / COLUMN_COUNT;
        grid.cellSize = new Vector2(cell, cell * 1.2f);
    }
}
