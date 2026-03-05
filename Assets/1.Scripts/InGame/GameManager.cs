using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public int destroyOreStone { get; private set; }

    public void AddDestroyOreStone(int amount = 1)
    {
        destroyOreStone += amount;

        // 필요하면 여기서 UI 업데이트, 세이브, 업적 체크 등도 같이 처리
        // UpdateDestroyOreStoneUI();
    }

    void Start()
    {
        // Joystick joystick = FindFirstObjectByType<Joystick>();
        // joystick.gameObject.SetActive(false);
        // Player.Instance.gameObject.SetActive(false);
        // PlayerInLobby.Instance.StartGame(() =>
        // {
        //     Player.Instance.gameObject.SetActive(true);
        //     joystick.gameObject.SetActive(true);
        //     StartCoroutine(CoSpawn());
        // });

        //StartCoroutine(CoSpawn());
    }

    IEnumerator CoSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);
            Enemy enemy = EnemyManager.Instance.GetEnemy(EnemyType.Ranged);
            // float length = (MagmaCore.Instance.transform.position - Player.Instance.transform.position).magnitude + Camera.main.orthographicSize * 2;
            Vector2 randomPos = Random.insideUnitCircle.normalized * 15;

            enemy.Spawn(randomPos);

        }
    }

    public void EndGame(bool clear)
    {

    }
}
