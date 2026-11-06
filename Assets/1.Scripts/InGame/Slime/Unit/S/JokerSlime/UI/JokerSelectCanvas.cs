using System.Collections.Generic;
using UnityEngine;

// 조커 변신 선택: 장착 슬라임(조커 제외) 중 하나를 고르면 변신. 열려 있는 동안 일시정지
// 프리팹: panels에 JokerSelectPanel들(최대 장착 수만큼) 배치, 닫기 버튼은 CloseCanvas 연결
public class JokerSelectCanvas : CanvasUI<JokerSelectCanvas>
{
    public JokerSelectPanel[] panels;

    JokerSlime joker;
    float prevTimeScale = 1f;

    public void Open(JokerSlime joker)
    {
        this.joker = joker;
        if (panels == null || panels.Length == 0)
            panels = GetComponentsInChildren<JokerSelectPanel>(true);

        List<string> keys = joker.GetTransformKeys();
        for (int i = 0; i < panels.Length; i++)
        {
            bool show = i < keys.Count;
            panels[i].gameObject.SetActive(show);
            if (show)
                panels[i].Set(SlimeManager.Instance.GetSlimeData(keys[i]), joker.mergeLevel); // 표시는 유지 레벨 (하락은 확률)
        }

        prevTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f; // 배속 유지
        Time.timeScale = 0f;
        OpenCanvas();
    }

    public void Selected(string key)
    {
        JokerSlime target = joker;
        CloseCanvas();
        if (target != null)
            target.TransformTo(key);
    }

    public override void CloseCanvas()
    {
        joker = null;
        Time.timeScale = prevTimeScale;
        base.CloseCanvas();
    }
}
