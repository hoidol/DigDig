using Cysharp.Threading.Tasks;

// 게스트(익명) 상태에서 누르면 게스트 계정에 Apple 계정을 연결(데이터 유지)
public class FirebaseAppleLoginButton : FirebaseLoginButtonBase
{
    protected override string LoginName => "Apple";
    protected override UniTask<bool> SignInAsync(FirebaseAuth auth) => auth.SignInWithAppleAsync();
}
