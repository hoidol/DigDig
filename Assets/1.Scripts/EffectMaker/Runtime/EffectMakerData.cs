using System;
using UnityEditor;
using UnityEngine;

namespace EffectMaker
{
    // 이펙트를 누가 쓰는지
    public enum CasterType
    {
        Minion,
        King,
    }

    // 이펙트가 생기는 자리
    public enum SpawnTarget
    {
        Caster,         // 시전자(미니언/킹) 위치
        NearestEnemy,   // 시전자와 가장 가까운 적
        AllEnemies,     // 적 전부
        TileCenter,     // 2x2 타일 가운데
        CasterToEnemy,  // 시전자 위치에서 가장 가까운 적 쪽으로 회전
    }

    // 설정 (EditorPrefs에 JSON으로 저장). 이 어셈블리는 UNITY_EDITOR에서만 컴파일됨
    [Serializable]
    public class EffectMakerSettings
    {
        const string PREFS_KEY = "EffectMaker_Settings";

        // 환경
        public CasterType casterType = CasterType.Minion;
        public string minionKey = "Flame";
        public int casterTile = 2;              // 2x2 칸 index (0 왼위, 1 오른위, 2 왼아래, 3 오른아래)
        public int enemyCount = 6;
        public float enemyRadius = 2.2f;
        public float enemySpeed = 40f;          // 도/초
        public float cameraSize = 3.2f;
        public Color bgColor = new Color(0.16f, 0.18f, 0.22f, 1f);

        // 이펙트
        public float interval = 2.5f;           // 스킬 발동 간격(초)
        public float effectScale = 1f;
        public SpawnTarget spawnTarget = SpawnTarget.Caster;
        public bool followTarget = true;        // 대상(시전자/적)을 따라다님
        public float offsetY = 0f;
        public float loopTime = 1.5f;           // 루프 이펙트를 켜 둘 시간
        public float maxLife = 6f;              // 이펙트 최대 유지 시간
        public float timeScale = 1f;
        public bool paused;

        // 후보 목록
        public string folders = "Assets/CartoonVFX9X;Assets/Hovl Studio;Assets/Resources/Effect;Assets/3.Prefabs/Effect;Assets/3.ParticleEffect";
        public string nameFilter = "";
        public bool onlyParticle = true;
        public int page;

        // 즐겨찾기 용도 (예: "지뢰 진화 B - 굴러가는 폭탄")
        public string usage = "";

        // 칸별 프리팹 경로 9개
        public string[] cellPaths = new string[EffectMakerStage.CELL_COUNT];

        public static EffectMakerSettings Load()
        {
            var settings = new EffectMakerSettings();
            string json = EditorPrefs.GetString(PREFS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try { JsonUtility.FromJsonOverwrite(json, settings); }
                catch (Exception e) { Debug.LogWarning($"[EffectMaker] 설정을 읽지 못해 기본값 사용: {e.Message}"); }
            }
            if (settings.cellPaths == null || settings.cellPaths.Length != EffectMakerStage.CELL_COUNT)
                Array.Resize(ref settings.cellPaths, EffectMakerStage.CELL_COUNT);
            return settings;
        }

        public void Save()
        {
            EditorPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(this));
        }
    }
}
