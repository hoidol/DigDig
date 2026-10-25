using TMPro;
using UnityEngine;

// 강화 종류 칸 (표시 방식은 기획 확정 후. 썸네일 + key 텍스트)
public class ReinforceBuffTypeSlot : ReinforceSlot
{
    public TMP_Text text;

    public override void SetSlot(string key, Sprite sprite)
    {
        base.SetSlot(key, sprite);
        if (text != null)
            text.text = key;
    }
}
