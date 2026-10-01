using Cysharp.Threading.Tasks;
using UnityEngine;

public class ChangeItemPanel : OwnItemPanel
{
    
    // 선택한 슬롯의 아이템을 버리고 새 아이템으로 교체
    public override void OnClickedButton()
    {
        if (item == null)
            return;

        GetComponentInParent<ChangeItemCanvas>().Selected(idx).Forget();
    }
}
