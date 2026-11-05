using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Extensions;

// Firebase 앱 초기화 담당. Auth/DB 등 다른 Firebase 기능은 IsInitialized(또는 WaitInitAsync) 이후에 사용한다.
public class FirebaseManager : MonoSingleton<FirebaseManager>
{
    public static event Action<bool> OnInitialized; // 성공 여부

    public FirebaseApp App { get; private set; }
    public bool IsInitialized { get; private set; }
    public bool IsInitFailed { get; private set; }
    public FirebaseAuth firebaseAuth;
    public FirebaseDatabase firebaseDB;

    UniTaskCompletionSource<bool> initTcs;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 코드로 생성되므로 하위 기능 컴포넌트도 여기서 붙인다
        if (firebaseAuth == null && !TryGetComponent(out firebaseAuth))
            firebaseAuth = gameObject.AddComponent<FirebaseAuth>();
        if (firebaseDB == null && !TryGetComponent(out firebaseDB))
            firebaseDB = gameObject.AddComponent<FirebaseDatabase>();

        InitAsync().Forget();
    }

    // 초기화가 끝날 때까지 대기. 성공 여부 반환 (여러 번 호출해도 초기화는 한 번만)
    public UniTask<bool> WaitInitAsync() => InitAsync();

    UniTask<bool> InitAsync()
    {
        if (initTcs != null)
            return initTcs.Task;

        initTcs = new UniTaskCompletionSource<bool>();
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            bool success = !task.IsFaulted && !task.IsCanceled && task.Result == DependencyStatus.Available;
            if (success)
            {
                App = FirebaseApp.DefaultInstance;
                IsInitialized = true;
                Debug.Log("[Firebase] 초기화 완료");
                firebaseAuth.Init();
            }
            else
            {
                IsInitFailed = true;
                string reason = task.IsFaulted ? task.Exception?.ToString() : task.Result.ToString();
                Debug.LogError($"[Firebase] 초기화 실패 : {reason}");
            }

            initTcs.TrySetResult(success);
            OnInitialized?.Invoke(success);
        });
        return initTcs.Task;
    }
}
