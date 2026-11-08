using Cysharp.Threading.Tasks;

public class FirebaseAnonyLoginButton : FirebaseLoginButtonBase
{
    protected override string LoginName => TranslateManager.GetText("Guest");
    protected override UniTask<bool> SignInAsync(FirebaseAuth auth) => auth.SignInAnonymouslyAsync(OnSignInCompleted);
}
