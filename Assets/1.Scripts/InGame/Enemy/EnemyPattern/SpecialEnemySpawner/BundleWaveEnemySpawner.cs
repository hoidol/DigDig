using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

// 화면 밖 랜덤 방향에 적을 뭉치(Bundle) 단위로 모아서, 일정 간격으로 여러 웨이브 소환
public class BundleWaveEnemySpawner : SpecialEnemySpawner
{
    [SerializeField] EnemyType enemyType = EnemyType.Melee;
    [SerializeField] int waveCount = 3;          // 웨이브 수
    [SerializeField] float waveInterval = 3f;    // 웨이브 간격(초)
    [SerializeField] int bundleCountPerWave = 3; // 웨이브당 뭉치 수
    [SerializeField] int enemyCountPerBundle = 8; // 뭉치당 적 수
    [SerializeField] float bundleRadius = 1.5f;  // 뭉치 퍼짐 반경
    [SerializeField] float distance = -1;        // 0 이하면 화면 밖 기본 거리 사용

    CancellationTokenSource cts;

    public override void Spawn()
    {
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(enemyType);
        if (enemyPrefab == null) return;

        EndSpawn();
        cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        SpawnWaves(enemyPrefab, cts.Token).Forget();
    }

    async UniTaskVoid SpawnWaves(Enemy enemyPrefab, CancellationToken token)
    {
        for (int w = 0; w < waveCount; w++)
        {
            SpawnWave(enemyPrefab);

            if (w < waveCount - 1)
            {
                bool canceled = await UniTask.Delay(TimeSpan.FromSeconds(waveInterval), cancellationToken: token)
                    .SuppressCancellationThrow();
                if (canceled) return;
            }
        }
    }

    void SpawnWave(Enemy enemyPrefab)
    {
        Vector2 center = Character.Instance.transform.position;
        float d = distance > 0 ? distance : CameraManager.Instance.mainCamera.orthographicSize + 1.5f + bundleRadius;

        // 뭉치끼리 겹치지 않도록 방향을 균등 분할 후 약간 흔듦
        float startAngle = Random.Range(0f, 360f);
        float step = 360f / Mathf.Max(1, bundleCountPerWave);

        for (int b = 0; b < bundleCountPerWave; b++)
        {
            float angle = (startAngle + step * b + Random.Range(-step * 0.25f, step * 0.25f)) * Mathf.Deg2Rad;
            Vector2 bundleCenter = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * d;

            for (int i = 0; i < enemyCountPerBundle; i++)
            {
                Vector2 pos = bundleCenter + Random.insideUnitCircle * bundleRadius;
                Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
                enemy?.Spawn(pos);
            }
        }
    }

    public override void EndSpawn()
    {
        if (cts == null) return;
        cts.Cancel();
        cts.Dispose();
        cts = null;
    }
}
