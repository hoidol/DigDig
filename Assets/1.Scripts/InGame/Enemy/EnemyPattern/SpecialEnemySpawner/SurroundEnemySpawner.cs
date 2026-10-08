using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// 플레이어를 원형으로 포위하도록 적을 일정 간격으로 소환
public class SurroundEnemySpawner : SpecialEnemySpawner
{
    [SerializeField] EnemyType enemyType = EnemyType.Melee;
    [SerializeField] int count = 40;
    [SerializeField] float radius = -1; // 0 이하면 화면 밖 기본 거리 사용
    
    public float delay=2;
    public float interval =3f;
    public int spawnCount=1;
    public override void Spawn()
    {
        StartCoroutine(SpawnCoroutine());
    }
    IEnumerator SpawnCoroutine()
    {
        WaveWarningCanvas.Instance.StartWaveWarning(WaveWarningType.Surround, Vector2.zero);
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(interval);
        }
    }   
    void SpawnEnemy()
    {
        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(enemyType);
        if (enemyPrefab == null || count <= 0) return;

        Vector2 center = Character.Instance.transform.position;
        float r = radius > 0 ? radius : CameraManager.Instance.mainCamera.orthographicSize + 1.5f;
        float startAngle = Random.Range(0f, 360f);
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = (startAngle + step * i) * Mathf.Deg2Rad;
            Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;

            Enemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab);
            enemy?.Spawn(pos + Random.insideUnitCircle*0.3f);
        }
    }
    public override void EndSpawn()
    {

    }
}
