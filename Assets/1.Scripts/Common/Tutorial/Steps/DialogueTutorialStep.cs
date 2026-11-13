using UnityEngine;

// 대화 블록: 화면 중앙 대화창에 대사를 하나씩 보여 주고, 화면을 터치하면 다음 대사. 마지막 대사 뒤에 다음 단계
public class DialogueTutorialStep : TutorialStep
{
    [Header("화자 (TutorialCanvas.speakers의 key). 이름·초상화가 대화창에 나옴")]
    public string speakerKey = "KingSlime";

    [Header("대사 목록 (번역 key면 번역문, 아니면 적은 글씨 그대로)")]
    [TextArea(2, 5)]
    public string[] lines;

    int lineIdx;

    protected override void OnEnter()
    {
        if (lines == null || lines.Length == 0 || Canvas == null)
        {
            Complete();
            return;
        }

        lineIdx = 0;
        ShowLine();
    }

    protected override void OnExit()
    {
        if (Canvas != null)
            Canvas.HideDialogue();
    }

    void ShowLine()
    {
        Canvas.ShowDialogue(speakerKey, GetText(lines[lineIdx]), OnTouched);
    }

    void OnTouched()
    {
        lineIdx++;
        if (lineIdx >= lines.Length)
        {
            Complete();
            return;
        }
        ShowLine();
    }

    public static string GetText(string line)
    {
        if (string.IsNullOrEmpty(line))
            return "";
        return TranslateManager.HasKey(line) ? TranslateManager.GetText(line) : line;
    }
}
