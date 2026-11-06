using System;
using UnityEditor;
using UnityEngine;

// 구글 시트 → Assets 아래 CSV(TSV) 다운로드 설정 (Tools/Data/Sheet Download Setting)
// 시트는 "링크가 있는 모든 사용자: 뷰어"로 공유되어 있어야 한다. (API 키 불필요)
// 탭 하나에 표가 여러 개면 range로 잘라서 각각 다른 CSV로 저장한다.
[CreateAssetMenu(fileName = "SheetDownloadSetting", menuName = "Data/SheetDownloadSetting")]
public class SheetDownloadSetting : ScriptableObject
{
    public const string DEFAULT_ASSET_PATH = "Assets/Json/SheetDownloadSetting.asset";

    [Tooltip("항목의 sheetId가 비어 있으면 사용. 시트 URL의 /d/ 와 /edit 사이 값")]
    public string defaultSheetId;
    //public string gId = "1244868266";
    // 슬라임 강화 데이터는 등급별 탭 → entries에 Json/SlimeEnhance/SlimeEnhance_{등급}.csv로 각각 등록
    public Entry[] entries = Array.Empty<Entry>();

    // 슬라임 전용 (sheetId는 defaultSheetId 사용, gid를 비우면 건너뜀)
    [Header("Slime")]
    [Tooltip("Slime 탭 gid → " + SLIME_CSV_PATH)]
    public string slimeGid = "1109237000";
    [Tooltip("Slime 탭에서 받을 범위 (A열 별명 제외)")]
    public string slimeRange = "B1:F";

    public const string SLIME_CSV_PATH = "Json/SlimeData.csv";

    [Serializable]
    public class Entry
    {
        public bool enabled = true;
        [Tooltip("Assets 기준 저장 경로 (ex. Json/EnemyData.csv)")]
        public string csvPath;
        [Tooltip("비우면 defaultSheetId 사용")]
        public string sheetId;
        [Tooltip("탭 ID. 시트 URL의 #gid= 뒤 숫자")]
        public string gid;
        [Tooltip("A1 표기 범위. 비우면 탭 전체 (ex. A1:H20, A:H, A3:H)")]
        public string range;
    }
}

[CustomEditor(typeof(SheetDownloadSetting))]
public class SheetDownloadSettingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        if (GUILayout.Button("Download From Sheet"))
        {
            SheetDownloader.Download((SheetDownloadSetting)target);
        }
    }
}
