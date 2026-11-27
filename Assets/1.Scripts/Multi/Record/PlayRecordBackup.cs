using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

// (2026-11-27) 끝난 판 기록의 로컬 백업 (파일 "PlayRecordBackup", 최근 MAX_LOCAL판, 올린 뒤에도 남김) → 안 올린 것만 서버로 (PlayRecordStore)
// 판이 끝날 때 먼저 여기 저장 → 올리기. 실패하면 다음 판 끝·로비 진입 때 다시
public static class PlayRecordBackup
{
    public const string FILE_NAME = "PlayRecordBackup";
    public const int MAX_LOCAL = 20;

    [System.Serializable]
    public class Entry
    {
        public bool uploaded;
        public PlayRecord record;
    }

    [System.Serializable]
    public class Data
    {
        public List<Entry> entries = new List<Entry>();
    }

    static Data data;
    static bool uploading;

    public static Data Load()
    {
        if (data == null)
            data = SaveManager.LoadData<Data>(FILE_NAME) ?? new Data();
        data.entries ??= new List<Entry>();
        data.entries.RemoveAll(e => e == null || e.record == null);
        return data;
    }

    static void Save() => SaveManager.SaveData(FILE_NAME, Load());

    public static int PendingCount => Load().entries.Count(e => !e.uploaded);

    public static void Add(PlayRecord record)
    {
        if (record == null)
            return;
        Data d = Load();
        d.entries.Add(new Entry { record = record });
        while (d.entries.Count > MAX_LOCAL)
            d.entries.RemoveAt(0);
        Save();
    }

    // 안 올린 기록을 차례로 올림 (동시에 한 번만)
    public static async UniTask UploadPendingAsync()
    {
        if (uploading)
            return;
        uploading = true;
        try
        {
            foreach (Entry e in Load().entries.Where(x => !x.uploaded).ToList())
            {
                if (!await PlayRecordStore.Instance.UploadAsync(e.record))
                    break; // 서버 문제면 다음에
                e.uploaded = true;
                Save();
            }
        }
        finally
        {
            uploading = false;
        }
    }

    // 테스트
    public static void ClearForTest()
    {
        data = new Data();
        Save();
    }
}
