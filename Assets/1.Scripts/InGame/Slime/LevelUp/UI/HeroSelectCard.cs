using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// (2026-11-27) 레벨업 Lv.1 용사 선택 카드 (일반 레벨업 카드 LevelUpStatPanel과 따로)
// 보여주는 것 = 용사 아이콘 · 이름 · "정예 버프 : 공격력 +10%" 한 줄 · 정예 버프 칸 3×3 그림 (그 밖의 글씨 없음)
// 씬: InGame LevelUpCanvas/Panel/Bg/HeroSelectContainer/HeroSelectCard (메뉴 Tools/InGame/Add Hero Select Cards To LevelUpCanvas)
//   카드는 1장만 두면 됨: 모자라면 LevelUpCanvas가 복사. 모양·위치는 씬에서 직접 수정
// 카드 어디를 눌러도 선택 (튀어나오는 중엔 무시)
public class HeroSelectCard : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text titleText;
    public TMP_Text eliteBuffText;   // "정예 버프 : …"
    public Image thumImage;
    public Image borderImage;        // 비워도 됨 (용사 색)
    public Image iconBgImage;        // 비워도 됨 (용사 색)
    [Header("정예 버프 칸 (3×3, 왼위부터 가로 순서, 가운데 = 슬라임)")]
    public Image[] shapeCells;
    public Color shapeEmptyColor = new Color(1f, 1f, 1f, 0.15f);

    AbilityData data;
    Vector2Int[] tiles;
    public AbilityData Data => data;
    public Vector2Int[] Tiles => tiles;

    public void Set(AbilityData data, Vector2Int[] tiles)
    {
        this.data = data;
        this.tiles = tiles;
        HeroSlimeAbility hero = data.prefab as HeroSlimeAbility;
        Color color = hero != null ? hero.tileColor : Color.white;
        color.a = 1f;

        if (titleText != null)
            titleText.text = data.title;
        if (eliteBuffText != null)
            eliteBuffText.text = hero != null ? $"정예 버프 : {hero.GetEliteBuffText(1)}" : "";
        if (thumImage != null)
        {
            thumImage.sprite = data.thum;
            thumImage.enabled = data.thum != null;
            thumImage.preserveAspect = true;
        }
        if (borderImage != null)
            borderImage.color = color;
        if (iconBgImage != null)
            iconBgImage.color = color;
        if (shapeCells != null && shapeCells.Length == 9)
            for (int i = 0; i < 9; i++)
            {
                if (i == 4 || shapeCells[i] == null)
                    continue; // 가운데 = 슬라임 (씬 그림 그대로)
                var offset = new Vector2Int(i % 3 - 1, i / 3 - 1);
                shapeCells[i].color = tiles != null && System.Array.IndexOf(tiles, offset) >= 0 ? color : shapeEmptyColor;
            }
    }

    public void PlayAppear(int order)
    {
        var rt = (RectTransform)transform;
        rt.DOKill();
        rt.localScale = Vector3.zero;
        rt.DOScale(1.1f, 0.2f + order * 0.15f).SetUpdate(true).OnComplete(() => rt.DOScale(1, 0.25f).SetUpdate(true));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging || transform.localScale.x < 0.5f)
            return;
        OnClickedSelect();
    }

    public void OnClickedSelect()
    {
        if (data == null || Slime.Instance == null)
            return;
        Vector2Int[] picked = tiles;
        Slime.Instance.abilityInventory.Add(data, a =>
        {
            if (a is HeroSlimeAbility hero && picked != null && hero.level == 1)
                hero.eliteOffsets = picked;
        });
        data = null; // 연타로 두 번 적용 방지
        InGame.LevelUpCanvas.Instance.CloseCanvas();
    }
}
