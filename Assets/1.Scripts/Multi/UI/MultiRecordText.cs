using TMPro;
using UnityEngine;

// (2026-11-27) 로비 멀티 전적 글씨 "우승 N · 클리어 N" (판수는 저장만, 표시 안 함). 멀티 게임 버튼 아래 (Tools/Lobby/Add Multi Game Buttons)
[RequireComponent(typeof(TMP_Text))]
public class MultiRecordText : MonoBehaviour
{
    void OnEnable()
    {
        if (UserDataManager.Instance == null)
            return;
        UserData user = UserDataManager.Instance.userData;
        GetComponent<TMP_Text>().text = string.Format(TranslateManager.GetText("Multi_Record"), user.multiChampion, user.multiClear);
    }
}
