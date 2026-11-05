using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

// 유저 데이터 클라우드 저장/불러오기 (Realtime Database).
// 경로 : users/{uid}/save = { json : UserCloudData JSON 문자열, savedAt : 서버 시간(ms) }
// JSON을 문자열 하나로 저장하는 이유 : RTDB는 빈 배열/null을 지우고 배열을 맵으로 바꿀 수 있어서, 로컬 저장(JsonUtility) 형식을 그대로 보존하려고.
// (Firebase.Database.FirebaseDatabase와 이름이 같아서 SDK 쪽은 전체 이름으로 쓴다)
public class FirebaseDatabase : MonoBehaviour
{
    const string USERS_PATH = "users";
    const string SAVE_PATH = "save";
    const string JSON_KEY = "json";
    const string SAVED_AT_KEY = "savedAt";

    public bool IsBusy { get; private set; }

    async UniTask<Firebase.Database.DatabaseReference> GetSaveRefAsync()
    {
        string uid = await FirebaseManager.Instance.firebaseAuth.EnsureSignedInAsync();
        if (string.IsNullOrEmpty(uid))
            return null;

        return Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference
            .Child(USERS_PATH).Child(uid).Child(SAVE_PATH);
    }

    // 현재 유저 데이터를 서버에 저장. 성공 여부 반환
    public async UniTask<bool> SaveUserDataAsync()
    {
        if (IsBusy)
            return false;
        IsBusy = true;
        try
        {
            var saveRef = await GetSaveRefAsync();
            if (saveRef == null)
                return false;

            string json = JsonUtility.ToJson(UserDataManager.Instance.GetCloudData());
            var dic = new System.Collections.Generic.Dictionary<string, object>
            {
                { JSON_KEY, json },
                { SAVED_AT_KEY, Firebase.Database.ServerValue.Timestamp },
            };
            await saveRef.SetValueAsync(dic).AsUniTask();
            Debug.Log("[FirebaseDatabase] 유저 데이터 저장 완료");
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
            var saveRef = await GetSaveRefAsync();
            if (saveRef == null)
                return false;

            var snapshot = await saveRef.GetValueAsync().AsUniTask();
            string json = snapshot.Exists ? snapshot.Child(JSON_KEY).Value as string : null;
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[FirebaseDatabase] 서버에 저장된 유저 데이터 없음");
                return false;
            }

            UserCloudData cloudData = JsonUtility.FromJson<UserCloudData>(json);
            UserDataManager.Instance.ApplyCloudData(cloudData);
            Debug.Log("[FirebaseDatabase] 유저 데이터 불러오기 완료");
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
