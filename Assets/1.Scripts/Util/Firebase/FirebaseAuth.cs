using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using FAuth = Firebase.Auth.FirebaseAuth; // 이 클래스 이름과 겹쳐서 별칭 사용

#if USE_GPGS && UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
#if USE_APPLE_AUTH && (UNITY_IOS || UNITY_STANDALONE_OSX)
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif

public enum LoginType
{
    None,
    Anonymous,
    Google,
    Apple,
}

// Firebase 로그인 담당. FirebaseManager 초기화 완료 후 Init이 호출된다.
// - 이전에 로그인한 유저가 있으면 자동 로그인(OnSignedIn).
// - 아직 로그인 UI가 없어서, 로그인 안 된 상태면 익명 로그인으로 uid를 확보한다(EnsureSignedInAsync).
// - 익명 상태에서 Google/Apple 로그인하면 익명 계정에 연결(Link)해 uid를 유지한다.
//   이미 다른 uid에 연결된 Google/Apple 계정이면 그 계정으로 전환(익명 계정 데이터는 버려짐).
//
// 플러그인 (Player Settings > Scripting Define Symbols에 추가해야 활성화)
// - Google : Google Play Games Plugin(GPGS v2) 설치 후 USE_GPGS
// - Apple  : com.lupidan.apple-signin-unity 설치 후 USE_APPLE_AUTH
public class FirebaseAuth : MonoBehaviour
{
    public static event Action<FirebaseUser> OnSignedIn;
    public static event Action OnSignedOut;

    FAuth auth;
    public bool IsReady => auth != null;
    public FirebaseUser CurrentUser => auth?.CurrentUser;
    public bool IsSignedIn => CurrentUser != null;
    public string UserId => CurrentUser?.UserId;
    public LoginType CurrentLoginType => GetLoginType(CurrentUser);

    bool isSigningIn;
    public bool IsSigningIn => isSigningIn;

    // 이 빌드/기기에서 로그인 버튼을 보여줄지 판단용
#if USE_GPGS && UNITY_ANDROID
    public bool IsGoogleSupported => true;
#else
    public bool IsGoogleSupported => false;
#endif
#if USE_APPLE_AUTH && (UNITY_IOS || UNITY_STANDALONE_OSX)
    public bool IsAppleSupported => appleAuthManager != null;
#else
    public bool IsAppleSupported => false;
#endif

#if USE_APPLE_AUTH && (UNITY_IOS || UNITY_STANDALONE_OSX)
    IAppleAuthManager appleAuthManager;
#endif

    // Firebase 초기화 완료 시 호출 (FirebaseManager)
    public void Init()
    {
        if (auth != null)
            return;

        auth = FAuth.DefaultInstance;

#if USE_GPGS && UNITY_ANDROID
        PlayGamesPlatform.Activate();
#endif
#if USE_APPLE_AUTH && (UNITY_IOS || UNITY_STANDALONE_OSX)
        if (AppleAuthManager.IsCurrentPlatformSupported)
            appleAuthManager = new AppleAuthManager(new PayloadDeserializer());
#endif

        if (auth.CurrentUser != null)
        {
            Debug.Log($"[FirebaseAuth] 자동 로그인 : {auth.CurrentUser.UserId} ({CurrentLoginType})");
            NotifySignedIn(auth.CurrentUser);
        }
        else
        {
            EnsureSignedInAsync().Forget(); // 로그인 UI 생기기 전까지 익명으로 미리 로그인
        }
    }

    void Update()
    {
#if USE_APPLE_AUTH && (UNITY_IOS || UNITY_STANDALONE_OSX)
        appleAuthManager?.Update(); // Apple 로그인 콜백 처리
#endif
    }

    // 로그인된 uid 반환. 로그인 안 돼 있으면 익명 로그인. 실패 시 null
    public async UniTask<string> EnsureSignedInAsync()
    {
        if (!await FirebaseManager.Instance.WaitInitAsync())
            return null;

        await UniTask.WaitWhile(() => isSigningIn); // 다른 로그인이 진행 중이면 끝날 때까지 대기
        if (IsSignedIn)
            return UserId;

        return await SignInAnonymouslyAsync() ? UserId : null;
    }

    // 로그인 함수 공통 : onComplete(성공 여부)는 성공·실패 모두 호출된다
    public UniTask<bool> SignInAnonymouslyAsync(Action<bool> onComplete = null) => WithCallback(SignInAnonymouslyCoreAsync(), onComplete);
    public UniTask<bool> SignInWithGoogleAsync(Action<bool> onComplete = null) => WithCallback(SignInWithGoogleCoreAsync(), onComplete);
    public UniTask<bool> SignInWithAppleAsync(Action<bool> onComplete = null) => WithCallback(SignInWithAppleCoreAsync(), onComplete);

    static async UniTask<bool> WithCallback(UniTask<bool> task, Action<bool> onComplete)
    {
        bool success = await task;
        try
        {
            onComplete?.Invoke(success);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseAuth] 로그인 콜백 오류 : {e}");
        }
        return success;
    }

    #region 익명
    async UniTask<bool> SignInAnonymouslyCoreAsync()
    {
        if (!CanStartSignIn())
            return false;

        isSigningIn = true;
        try
        {
            AuthResult result = await auth.SignInAnonymouslyAsync();
            return HandleSignedIn(result.User);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseAuth] 익명 로그인 실패 : {e}");
            return false;
        }
        finally
        {
            isSigningIn = false;
        }
    }
    #endregion

    #region Google (GPGS)
    async UniTask<bool> SignInWithGoogleCoreAsync()
    {
        if (!CanStartSignIn())
            return false;

#if USE_GPGS && UNITY_ANDROID
        isSigningIn = true;
        try
        {
            string authCode = await GetGpgsAuthCodeAsync();
            if (string.IsNullOrEmpty(authCode))
                return false;

            Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
            return await SignInWithCredentialAsync(credential);
        }
        finally
        {
            isSigningIn = false;
        }
#else
        Debug.LogWarning("[FirebaseAuth] Google 로그인 미지원 (Android + USE_GPGS 필요)");
        await UniTask.CompletedTask;
        return false;
#endif
    }

#if USE_GPGS && UNITY_ANDROID
    UniTask<string> GetGpgsAuthCodeAsync()
    {
        var tcs = new UniTaskCompletionSource<string>();
        PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
        {
            if (status != SignInStatus.Success)
            {
                Debug.LogError($"[FirebaseAuth] GPGS 로그인 실패 : {status}");
                tcs.TrySetResult(null);
                return;
            }
            PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
            {
                if (string.IsNullOrEmpty(code))
                    Debug.LogError("[FirebaseAuth] GPGS 서버 인증 코드 없음 (웹 클라이언트 ID 설정 확인)");
                tcs.TrySetResult(code);
            });
        });
        return tcs.Task;
    }
#endif
    #endregion

    #region Apple
    async UniTask<bool> SignInWithAppleCoreAsync()
    {
        if (!CanStartSignIn())
            return false;

#if USE_APPLE_AUTH && (UNITY_IOS || UNITY_STANDALONE_OSX)
        if (appleAuthManager == null)
        {
            Debug.LogWarning("[FirebaseAuth] 이 기기는 Apple 로그인을 지원하지 않음");
            return false;
        }

        isSigningIn = true;
        try
        {
            string rawNonce = GenerateNonce(32);
            var tcs = new UniTaskCompletionSource<IAppleIDCredential>();
            var args = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName, Sha256(rawNonce));
            appleAuthManager.LoginWithAppleId(args,
                credential => tcs.TrySetResult(credential as IAppleIDCredential),
                error =>
                {
                    Debug.LogError($"[FirebaseAuth] Apple 로그인 실패 : {error}");
                    tcs.TrySetResult(null);
                });

            IAppleIDCredential appleCredential = await tcs.Task;
            if (appleCredential?.IdentityToken == null)
                return false;

            string idToken = Encoding.UTF8.GetString(appleCredential.IdentityToken);
            string authCode = appleCredential.AuthorizationCode != null ? Encoding.UTF8.GetString(appleCredential.AuthorizationCode) : null;
            Credential credential = OAuthProvider.GetCredential("apple.com", idToken, rawNonce, authCode);
            return await SignInWithCredentialAsync(credential);
        }
        finally
        {
            isSigningIn = false;
        }
#else
        Debug.LogWarning("[FirebaseAuth] Apple 로그인 미지원 (iOS + USE_APPLE_AUTH 필요)");
        await UniTask.CompletedTask;
        return false;
#endif
    }
    #endregion

    public void SignOut()
    {
        if (auth == null || auth.CurrentUser == null)
            return;

        auth.SignOut(); // GPGS v2는 로그아웃 API가 없어 Firebase만 로그아웃
        Debug.Log("[FirebaseAuth] 로그아웃");
        OnSignedOut?.Invoke();
    }

    // 익명 상태면 Link(uid 유지), 아니면 그대로 로그인
    async UniTask<bool> SignInWithCredentialAsync(Credential credential)
    {
        try
        {
            FirebaseUser current = auth.CurrentUser;
            if (current != null && current.IsAnonymous)
            {
                try
                {
                    AuthResult linked = await current.LinkWithCredentialAsync(credential);
                    Debug.Log("[FirebaseAuth] 익명 계정에 연결 완료");
                    return HandleSignedIn(linked.User);
                }
                catch (FirebaseAccountLinkException e) when (e.UserInfo.UpdatedCredential.IsValid())
                {
                    // 이미 다른 계정에 연결된 자격 증명 → 그 계정으로 전환
                    Debug.Log("[FirebaseAuth] 기존 계정으로 전환");
                    credential = e.UserInfo.UpdatedCredential;
                }
            }

            AuthResult result = await auth.SignInAndRetrieveDataWithCredentialAsync(credential);
            return HandleSignedIn(result.User);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseAuth] 자격 증명 로그인 실패 : {e}");
            return false;
        }
    }

    bool CanStartSignIn()
    {
        if (auth == null)
        {
            Debug.LogWarning("[FirebaseAuth] Firebase 초기화 전");
            return false;
        }
        if (isSigningIn)
        {
            Debug.LogWarning("[FirebaseAuth] 로그인 진행 중");
            return false;
        }
        return true;
    }

    bool HandleSignedIn(FirebaseUser user)
    {
        if (user == null)
            return false;

        Debug.Log($"[FirebaseAuth] 로그인 성공 : {user.UserId} ({GetLoginType(user)})");
        NotifySignedIn(user);
        return true;
    }

    // 로그인 확정 시 공통 처리 (자동 로그인 포함)
    void NotifySignedIn(FirebaseUser user)
    {
        if (UserDataManager.Instance != null)
            UserDataManager.Instance.SetUid(user.UserId);
        OnSignedIn?.Invoke(user);
    }

    static LoginType GetLoginType(FirebaseUser user)
    {
        if (user == null)
            return LoginType.None;
        if (user.IsAnonymous)
            return LoginType.Anonymous;

        foreach (IUserInfo info in user.ProviderData)
        {
            switch (info.ProviderId)
            {
                case "apple.com": return LoginType.Apple;
                case "playgames.google.com":
                case "google.com": return LoginType.Google;
            }
        }
        return LoginType.None;
    }

    static string GenerateNonce(int length)
    {
        const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(bytes);

        var sb = new StringBuilder(length);
        foreach (byte b in bytes)
            sb.Append(charset[b % charset.Length]);
        return sb.ToString();
    }

    static string Sha256(string input)
    {
        using (var sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
