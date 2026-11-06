using Cysharp.Threading.Tasks;

// 게스트(익명) 상태에서 누르면 게스트 계정에 Google 계정을 연결(데이터 유지)
public class FirebaseGoogleLoginButton : FirebaseLoginButtonBase
{
    protected override string LoginName => "Google";
    protected override UniTask<bool> SignInAsync(FirebaseAuth auth) => auth.SignInWithGoogleAsync(OnSignInCompleted);
}
