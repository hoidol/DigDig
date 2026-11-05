using UnityEngine;
using Cysharp.Threading.Tasks;

// 현재 유저 데이터를 서버(Firebase RTDB)에 저장
public class UserDataSaveButton : ButtonUI
{
    public override void OnClickedBtn()
    {
        SaveAsync().Forget();
    }

    async UniTaskVoid SaveAsync()
    {
        BlockCanvas.Instance.OpenCanvas("저장 중...");
        button.interactable = false;
        bool success = await FirebaseManager.Instance.firebaseDB.SaveUserDataAsync();
        if (this == null)
            return;
        button.interactable = true;
        BlockCanvas.Instance.CloseCanvas();

        ToastCanvas.Toast(success ? "저장 완료" : "저장 실패");
    }
}
