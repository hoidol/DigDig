using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

// 로그인 버튼 공통: 진행 중 화면 막기 → 로그인 → 완료 콜백(OnSignInCompleted)
public abstract class FirebaseLoginButtonBase : ButtonUI
{
    protected abstract string LoginName { get; }
    protected abstract UniTask<bool> SignInAsync(FirebaseAuth auth);

    public override void OnClickedBtn()
    {
        LoginAsync().Forget();
    }

    async UniTaskVoid LoginAsync()
    {
        FirebaseAuth auth = FirebaseManager.Instance != null ? FirebaseManager.Instance.firebaseAuth : null;
        if (auth == null || !auth.IsReady)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Server_Connecting_Retry"));
            return;
        }
        if (auth.IsSigningIn)
            return;

        BlockCanvas.Instance.OpenCanvas(string.Format(TranslateManager.GetText("Login_InProgress"), LoginName));
        button.interactable = false;
        try
        {
            await SignInAsync(auth);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            BlockCanvas.Instance.CloseCanvas();
            if (this != null)
                button.interactable = true;
        }
    }

    // 세 로그인 버튼 공통 완료 처리 : 성공하면 토스트(설정 창은 그대로 두고 버튼만 갱신), 실패하면 실패 토스트
    protected void OnSignInCompleted(bool success)
    {
        if (!success)
        {
            ToastCanvas.Toast(string.Format(TranslateManager.GetText("Login_Failed"), LoginName));
            return;
        }

        ToastCanvas.Toast(TranslateManager.GetText("Login_Done"));
    }
}
