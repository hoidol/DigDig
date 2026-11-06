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
            ToastCanvas.Toast("서버 연결 중입니다. 잠시 후 다시 시도해 주세요.");
            return;
        }
        if (auth.IsSigningIn)
            return;

        BlockCanvas.Instance.OpenCanvas($"{LoginName} 로그인 중...");
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

    // 세 로그인 버튼 공통 완료 처리 : 성공하면 로그인 창 닫고 토스트, 실패하면 실패 토스트
    protected void OnSignInCompleted(bool success)
    {
        if (!success)
        {
            ToastCanvas.Toast($"{LoginName} 로그인 실패");
            return;
        }

        // RankingButton 등 다른 곳에서 이미 닫았을 수 있어 열려 있을 때만 닫음
        FirebaseLoginCanvas canvas = FirebaseLoginCanvas.Instance;
        if (canvas != null && canvas.gameObject.activeSelf)
            canvas.CloseCanvas();
        ToastCanvas.Toast("로그인 완료");
    }
}
