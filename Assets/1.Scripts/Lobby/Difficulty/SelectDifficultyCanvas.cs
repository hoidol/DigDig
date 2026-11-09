using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectDifficultyCanvas : CanvasUI<SelectDifficultyCanvas>
{
    // 비워 두면 자식 중 난이도 버튼이 아닌 Button을 닫기 버튼으로 자동 연결
    // (씬 CloseButton의 onClick이 다른 캔버스로 잘못 연결돼 있어 코드에서 연결)
    public Button closeButton;

    void Awake()
    {
        if (closeButton == null)
        {
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                if (button.GetComponent<SelectDifficultyButton>() == null)
                {
                    closeButton = button;
                    break;
                }
            }
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners(); // 잘못 연결된 런타임 리스너 제거 (인스펙터 연결은 SetPersistentListenerState로)
            for (int i = 0; i < closeButton.onClick.GetPersistentEventCount(); i++)
                closeButton.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
            closeButton.onClick.AddListener(CloseCanvas);
        }
    }

    public override void OpenCanvas(Action closeCallback = null)
    {
        base.OpenCanvas(closeCallback);
        foreach (SelectDifficultyButton button in GetComponentsInChildren<SelectDifficultyButton>(true))
            button.UpdateButton();
    }

    public void SelectedDifficulty(DifficultyType difficultyType)
    {
        UserDataManager.difficultyType= difficultyType;
        GameEventBus.Publish(new ChangedDifficultyEvent(difficultyType));
        CloseCanvas();
        ToastCanvas.Toast(string.Format(TranslateManager.GetText("Difficulty_Changed"), DifficultyDisplayButton.GetDifficultyText(difficultyType)));
    }
}
public class ChangedDifficultyEvent
{
    DifficultyType difficultyType;
    public ChangedDifficultyEvent(DifficultyType difficultyType)
    {
        this.difficultyType = difficultyType;
    }
}
