using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

// 유저 데이터 클라우드 저장/불러오기 (Realtime Database).
// 경로 : (유저 샤드 인스턴스) users/{uid}/
//          data/{항목} = JSON 문자열   (항목 = 각 매니저의 UserDataFileName : UserData, UserStageData ...)
//          savedAt     = 마지막 저장 서버 시간(ms)
// - 저장은 **바뀐 항목만** 올린다(마지막으로 올리거나 받은 JSON과 비교). 한 번의 다중 경로 업데이트라 항목들이 원자적으로 반영됨.
// - 불러오기는 서버에 있는 항목만 로컬에 덮어쓴다.
// 샤드는 처음 저장할 때 배정되어 shardMap/{uid}에 고정된다 (FirebaseDatabaseConfig 참고)
// 항목을 JSON 문자열로 저장하는 이유 : RTDB는 빈 배열/null을 지우고 배열을 맵으로 바꿀 수 있어서, 로컬 저장(JsonUtility) 형식을 그대로 보존하려고.
// (Firebase.Database.FirebaseDatabase와 이름이 같아서 SDK 쪽은 전체 이름으로 쓴다)
public class FirebaseDatabase : MonoBehaviour
{
    const string USERS_PATH = "users";
    const string DATA_PATH = "data";
    const string SAVED_AT_KEY = "savedAt";

    // 서버와 같다고 알고 있는 항목 JSON (uid별). 이것과 다른 항목만 업로드
    readonly Dictionary<string, string> syncedSections = new Dictionary<string, string>();
    string syncedUid;

    const string SHARD_PREFS_KEY = "UserDataShard_"; // + uid. 배정은 바뀌지 않으므로 기기에 캐시

    public bool IsBusy { get; private set; }

    string cachedShardUid;
    int cachedShard = -1;

    // users/{uid} 참조. assignIfMissing : 배정된 샤드가 없으면 새로 배정 (저장 시 true, 불러오기 시 false → 데이터 없음으로 처리)
    async UniTask<(Firebase.Database.DatabaseReference userRef, string uid)> GetUserRefAsync(bool assignIfMissing)
    {
        string uid = await FirebaseManager.Instance.firebaseAuth.EnsureSignedInAsync();
        if (string.IsNullOrEmpty(uid))
            return (null, null);

        int shard = await GetShardAsync(uid, assignIfMissing);
        if (shard < 0)
            return (null, uid);

        if (syncedUid != uid)
        {
            syncedSections.Clear(); // 계정이 바뀌면 비교 기준 초기화 → 다음 저장은 전체 업로드
            syncedUid = uid;
        }

        var userRef = FirebaseDatabaseConfig.UserData(shard).RootReference.Child(USERS_PATH).Child(uid);
        return (userRef, uid);
    }

    async UniTask<int> GetShardAsync(string uid, bool assignIfMissing)
    {
        if (cachedShardUid == uid && IsValidShard(cachedShard))
            return cachedShard;

        int shard = PlayerPrefs.GetInt(SHARD_PREFS_KEY + uid, -1);
        if (!IsValidShard(shard))
        {
            shard = await LoadOrAssignShardAsync(uid, assignIfMissing);
            if (!IsValidShard(shard))
                return -1;
            PlayerPrefs.SetInt(SHARD_PREFS_KEY + uid, shard);
            PlayerPrefs.Save();
        }

        cachedShardUid = uid;
        cachedShard = shard;
        return shard;
    }

    // shardMap/{uid}를 읽고, 없으면 트랜잭션으로 배정 (여러 기기가 동시에 배정해도 먼저 쓴 값 하나로 고정)
    async UniTask<int> LoadOrAssignShardAsync(string uid, bool assignIfMissing)
    {
        var shardRef = FirebaseDatabaseConfig.ShardMap.RootReference
            .Child(FirebaseDatabaseConfig.SHARD_MAP_PATH).Child(uid);

        var snapshot = await shardRef.GetValueAsync().AsUniTask();
        if (snapshot.Exists)
            return Convert.ToInt32(snapshot.Value);
        if (!assignIfMissing)
            return -1;

        int pick = FirebaseDatabaseConfig.PickShardForNewUser(uid);
        if (!IsValidShard(pick))
        {
            Debug.LogError("[FirebaseDatabase] 배정 가능한 샤드 없음 (FirebaseDatabaseConfig 확인)");
            return -1;
        }

        var result = await shardRef.RunTransaction(data =>
        {
            if (data.Value == null)
                data.Value = pick;
            return Firebase.Database.TransactionResult.Success(data);
        }).AsUniTask();

        int shard = Convert.ToInt32(result.Value);
        Debug.Log($"[FirebaseDatabase] 유저 데이터 샤드 배정 : {shard}");
        return shard;
    }

    static bool IsValidShard(int shard) => shard >= 0 && shard < FirebaseDatabaseConfig.USERDATA_URLS.Length;

    // 현재 유저 데이터를 서버에 저장. 성공 여부 반환
    public async UniTask<bool> SaveUserDataAsync()
    {
        if (IsBusy)
            return false;
        IsBusy = true;
        try
        {
            var (userRef, _) = await GetUserRefAsync(assignIfMissing: true);
            if (userRef == null)
                return false;

            Dictionary<string, string> sections = UserDataManager.Instance.GetCloudSections();
            var updates = new Dictionary<string, object>();
            foreach (var pair in sections)
            {
                if (!syncedSections.TryGetValue(pair.Key, out string synced) || synced != pair.Value)
                    updates[$"{DATA_PATH}/{pair.Key}"] = pair.Value;
            }

            if (updates.Count == 0)
            {
                Debug.Log("[FirebaseDatabase] 바뀐 항목 없음, 저장 생략");
                return true;
            }

            updates[SAVED_AT_KEY] = Firebase.Database.ServerValue.Timestamp;
            await userRef.UpdateChildrenAsync(updates).AsUniTask();

            foreach (var pair in sections)
                syncedSections[pair.Key] = pair.Value;
            Debug.Log($"[FirebaseDatabase] 유저 데이터 저장 완료 ({updates.Count - 1}개 항목)");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 유저 데이터 저장 실패 : {e}");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // 서버의 유저 데이터를 불러와 로컬에 덮어씀. 저장된 데이터가 없거나 실패하면 false (로컬 데이터 유지)
    public async UniTask<bool> LoadUserDataAsync()
    {
        if (IsBusy)
            return false;
        IsBusy = true;
        try
        {
            var (userRef, uid) = await GetUserRefAsync(assignIfMissing: false);
            if (userRef == null)
            {
                Debug.LogWarning("[FirebaseDatabase] 서버에 저장된 유저 데이터 없음 (배정된 샤드 없음)");
                return false;
            }

            var snapshot = await userRef.Child(DATA_PATH).GetValueAsync().AsUniTask();
            var sections = new Dictionary<string, string>();
            foreach (var child in snapshot.Children)
            {
                if (child.Value is string json && !string.IsNullOrEmpty(json))
                    sections[child.Key] = json;
            }

            if (sections.Count == 0)
            {
                Debug.LogWarning("[FirebaseDatabase] 서버에 저장된 유저 데이터 없음");
                return false;
            }

            UserDataManager.Instance.ApplyCloudSections(sections);
            UserDataManager.Instance.SetUid(uid); // 서버 UserData가 uid 기록 전에 저장된 것일 수 있어 다시 맞춤

            // 받은 항목은 로컬과 서버가 같아짐 → 다음 저장에서 안 바뀌었으면 생략
            syncedSections.Clear();
            foreach (var pair in sections)
                syncedSections[pair.Key] = pair.Value;
            Debug.Log($"[FirebaseDatabase] 유저 데이터 불러오기 완료 ({sections.Count}개 항목)");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 유저 데이터 불러오기 실패 : {e}");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
