using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

// 로그인 버튼 공통: 진행 중 화면 막기 → 로그인 → 결과 토스트
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
            ToastCanvas.Toast("서버 연결 중입니다. 잠시 후 다시 시도해 주세요.");
            return;
        }
        if (auth.IsSigningIn)
            return;

        BlockCanvas.Instance.OpenCanvas($"{LoginName} 로그인 중...");
        button.interactable = false;

        bool success;
        try
        {
            success = await SignInAsync(auth);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            success = false;
        }

        BlockCanvas.Instance.CloseCanvas();
        if (this == null)
            return;
        button.interactable = true;

        ToastCanvas.Toast(success ? $"{LoginName} 로그인 완료" : $"{LoginName} 로그인 실패");
    }
}
