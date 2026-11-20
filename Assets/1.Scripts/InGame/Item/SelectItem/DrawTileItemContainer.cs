using TMPro;
using UnityEngine;

public class DrawTileItemContainer : MonoBehaviour
{
    public TMP_Text infoText;
    public Animator animator;
    // chance = 이번 뽑기 성공 확률(%) (SelectItemCanvas.OpenDraw)
    public void StartDraw(int chance)
    {
        infoText.text = string.Format(TranslateManager.GetText("TileItem_DrawChance"), chance);
        
        animator.SetTrigger("Draw");
    
    }
}
