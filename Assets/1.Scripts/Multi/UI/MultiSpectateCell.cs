using TMPro;
using UnityEngine;
using UnityEngine.UI;

// (2026-11-27) 보기 화면 칸 하나 (MultiSpectatePanel이 템플릿을 칸 수만큼 복사): 미니언 그림 + lvN · 룬 그림 · 킹 표시 · 막힌 칸
public class MultiSpectateCell : MonoBehaviour
{
    public Image bgImage;
    public Image minionImage;
    public TMP_Text lvText;
    public GameObject evolvedMark;
    public Image runeImage;
    public GameObject kingMark;
    public GameObject blockedMark;

    public Vector2Int Index { get; set; }

    public void Set(MultiBoard.Cell cell)
    {
        bool hasMinion = cell != null && !string.IsNullOrEmpty(cell.minion);
        Sprite minion = hasMinion && MinionManager.Instance != null && MinionManager.Instance.minionDataDic.TryGetValue(cell.minion, out MinionData md) ? md.thum : null;
        if (minionImage != null)
        {
            minionImage.gameObject.SetActive(hasMinion);
            minionImage.sprite = minion;
            minionImage.preserveAspect = true;
        }
        if (lvText != null)
        {
            lvText.gameObject.SetActive(hasMinion);
            lvText.text = hasMinion ? $"lv{cell.lv}" : "";
        }
        if (evolvedMark != null)
            evolvedMark.SetActive(hasMinion && cell.evolved);
        bool hasRune = cell != null && !string.IsNullOrEmpty(cell.rune);
        if (runeImage != null)
        {
            RuneData rd = hasRune && RuneManager.Instance != null ? RuneManager.Instance.GetRuneData(cell.rune) : null;
            runeImage.gameObject.SetActive(rd != null && rd.thum != null);
            if (rd != null)
                runeImage.sprite = rd.thum;
        }
        if (kingMark != null)
            kingMark.SetActive(cell != null && cell.king);
        if (blockedMark != null)
            blockedMark.SetActive(cell != null && cell.blocked);
    }
}
