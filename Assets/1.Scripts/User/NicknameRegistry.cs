using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-27) 닉네임 중복 확인: 서버 공용 DB(FirebaseDatabaseConfig.Shared) nicknames/{key} = uid 로 선점 (트랜잭션)
// key = 앞뒤 공백 뺀 소문자 (대소문자만 다른 닉네임도 같은 닉네임). 닉네임을 바꾸면 이전 key는 내 것일 때만 비움
// 에디터·테스트 = 메모리 (Firebase 없음). 보안 규칙 = firebase/shared.rules.json
public static class NicknameRegistry
{
    // Realtime Database key에 못 쓰는 글자 → 닉네임에도 못 씀
    public static readonly char[] FORBIDDEN_CHARS = { '.', '$', '#', '[', ']', '/' };

    public enum Result { Ok, Taken, Failed }

    // 테스트·에디터: true면 메모리 (기본 = 에디터이거나 Firebase가 없으면)
    public static bool? ForceFake;
    public static readonly Dictionary<string, string> fakeTaken = new Dictionary<string, string>();
    public const string FAKE_UID = "me";

    static bool UseFake => ForceFake ?? (Application.isEditor || FirebaseManager.Instance == null);

    public static string Key(string nickname) => (nickname ?? "").Trim().ToLowerInvariant();

    // 닉네임 선점 (이미 내 것이면 Ok). 성공하면 이전 닉네임 key를 비움
    public static async UniTask<Result> ClaimAsync(string nickname, string previous)
    {
        string key = Key(nickname);
        string prevKey = string.IsNullOrWhiteSpace(previous) || Guid.TryParse(previous, out _) ? null : Key(previous);
        if (string.IsNullOrEmpty(key))
            return Result.Failed;
        if (UseFake)
        {
            if (fakeTaken.TryGetValue(key, out string owner) && owner != FAKE_UID)
                return Result.Taken;
            fakeTaken[key] = FAKE_UID;
            if (prevKey != null && prevKey != key && fakeTaken.TryGetValue(prevKey, out string p) && p == FAKE_UID)
                fakeTaken.Remove(prevKey);
            return Result.Ok;
        }
        try
        {
            string uid = await FirebaseManager.Instance.firebaseAuth.EnsureSignedInAsync();
            if (string.IsNullOrEmpty(uid))
                return Result.Failed;
            var root = FirebaseDatabaseConfig.Shared.RootReference.Child("nicknames");
            bool taken = false;
            await root.Child(key).RunTransaction(data =>
            {
                string cur = data.Value as string;
                if (cur != null && cur != uid)
                {
                    taken = true;
                    return Firebase.Database.TransactionResult.Abort();
                }
                taken = false;
                data.Value = uid;
                return Firebase.Database.TransactionResult.Success(data);
            }).AsUniTask();
            if (taken)
                return Result.Taken;
            if (prevKey != null && prevKey != key)
            {
                await root.Child(prevKey).RunTransaction(data =>
                {
                    if (data.Value as string != uid)
                        return Firebase.Database.TransactionResult.Abort();
                    data.Value = null;
                    return Firebase.Database.TransactionResult.Success(data);
                }).AsUniTask();
            }
            return Result.Ok;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Nickname] 중복 확인 실패 : {e.Message}");
            return Result.Failed;
        }
    }
}
