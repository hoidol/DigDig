using TMPro;
using UnityEngine;

public class DrawTileItemContainer : MonoBehaviour
{
    public TMP_Text infoText;
    public Animator animator;
    public void StartDraw()
    {
        // infoText.text = isSuccess ? "타일 강화 아이템을 얻었습니다!" : "타일 강화 아이템 획득에 실패했습니다.";
        infoText.text =string.Format(TranslateManager.GetText("TileItem_DrawChance"), DrawItemButton.GetDrawChance());
        
        animator.SetTrigger("Draw");
    
    }
}
