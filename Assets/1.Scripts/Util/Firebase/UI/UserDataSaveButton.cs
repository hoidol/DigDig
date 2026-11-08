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
        BlockCanvas.Instance.OpenCanvas(TranslateManager.GetText("Saving"));
        button.interactable = false;
        bool success = await FirebaseManager.Instance.firebaseDB.SaveUserDataAsync();
        if (this == null)
            return;
        button.interactable = true;
        BlockCanvas.Instance.CloseCanvas();

        ToastCanvas.Toast(success ? TranslateManager.GetText("Save_Done") : TranslateManager.GetText("Save_Failed"));
    }
}
