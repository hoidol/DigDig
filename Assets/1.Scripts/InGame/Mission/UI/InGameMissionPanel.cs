using System.Collections.Generic;
using TMPro;
using UnityEngine;

// (2026-11-23) 인게임 미션 목록 표시. 씬에 두고 lineTemplate(꺼 둔 TMP_Text 한 줄)을 연결 → 미션 수만큼 복사
// 줄 = "제목 2/5" (진행도가 있는 미션만), 완료 = 초록 + 완료 표시, 실패 = 흐리게
public class InGameMissionPanel : MonoBehaviour
{
    public TMP_Text lineTemplate;
    public Color progressColor = Color.white;
    public Color completeColor = new Color(0.45f, 1f, 0.45f);
    public Color failedColor = new Color(1f, 1f, 1f, 0.35f);

    readonly List<TMP_Text> lines = new();
    float refreshTimer;

    void OnEnable()
    {
        InGameMissionSystem.Changed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        InGameMissionSystem.Changed -= Refresh;
    }

    // 진행도(배치 수)는 상태가 안 바뀌어도 변하므로 가끔 갱신
    void Update()
    {
        refreshTimer += Time.unscaledDeltaTime;
        if (refreshTimer < 0.5f)
            return;
        refreshTimer = 0;
        Refresh();
    }

    void Refresh()
    {
        InGameMissionSystem system = InGameMissionSystem.Instance;
        if (system == null || lineTemplate == null)
            return;
        lineTemplate.gameObject.SetActive(false);

        while (lines.Count < system.entries.Count)
        {
            TMP_Text line = Instantiate(lineTemplate, lineTemplate.transform.parent);
            lines.Add(line);
        }
        for (int i = 0; i < lines.Count; i++)
        {
            bool show = i < system.entries.Count;
            lines[i].gameObject.SetActive(show);
            if (show)
                SetLine(lines[i], system.entries[i], system.Context);
        }
    }

    void SetLine(TMP_Text line, InGameMissionSystem.Entry entry, MissionContext ctx)
    {
        string text = entry.mission.GetTitle();
        switch (entry.state)
        {
            case InGameMissionSystem.State.Complete:
                text += " " + TranslateManager.GetText("Mission_Done");
                line.color = completeColor;
                break;
            case InGameMissionSystem.State.Failed:
                text += " " + TranslateManager.GetText("Mission_Failed");
                line.color = failedColor;
                break;
            default:
                var (cur, goal) = entry.mission.GetProgress(ctx);
                if (goal > 0)
                    text += $" {cur}/{goal}";
                line.color = progressColor;
                break;
        }
        line.text = text;
    }
}
