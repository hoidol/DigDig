using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnBonusEliteEnemyButton : MonoBehaviour
{
    public float cooltime = 30;
    public float cooltimer = 0;
    public int spawnCount = 0;
    bool isReady;
    public Image timeBar;
    public GameObject rootObj;
    void Start()
    {
        //30초 후 활성화

        rootObj.SetActive(false);
        StartCoroutine(WaitAcitive(50f));
    }

    IEnumerator WaitAcitive(float waitTime = 30f)
    {
        isReady = false;
        cooltime = waitTime;
        cooltimer = waitTime;
        yield return new WaitForSeconds(waitTime);    
        
        cooltimer = 0;
        rootObj.SetActive(true);
        ReadyElite();
    }

    public void Update()
    {
        if(isReady || rootObj.gameObject.activeSelf == false)
        {
            return;
        }
        
        if (cooltimer > 0)
        {
            cooltimer -= Time.deltaTime;
        }
        timeBar.fillAmount = cooltimer / cooltime;
    }

    void ReadyElite()
    {
        isReady =true;
        timeBar.fillAmount = 0;
    }

    public void OnClickedBtn()
    {
        if(!rootObj.activeSelf)
            return;
        if(isReady == false)
        {
            ToastCanvas.Toast("Not Ready");
            return;
        }

        StartCoroutine(WaitAcitive());

        Enemy enemyPrefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.BonusElite);
        EliteEnemy enemy = EnemySpawner.Instance.Instantiate(enemyPrefab) as EliteEnemy;
        enemy?.Spawn(EnemySpawner.Instance.GetSpawnTopPosition(),spawnCount);

        spawnCount++;
    }
    
}