using Cysharp.Threading.Tasks;

public class FirebaseAnonyLoginButton : FirebaseLoginButtonBase
{
    protected override string LoginName => "게스트";
    protected override UniTask<bool> SignInAsync(FirebaseAuth auth) => auth.SignInAnonymouslyAsync(OnSignInCompleted);
}
