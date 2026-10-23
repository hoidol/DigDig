using System.Linq;
using UnityEngine;

public class EnemySpawnerContainer : MonoBehaviour
{

    public Wave[] waves;
    public void Init() 
    {        
        waves = GetComponentsInChildren<Wave>(true);

        for (int i = 0; i < waves.Length; i++)
        {
            waves[i].gameObject.SetActive(false);
        }
    }
    public Wave GetWave(WaveType waveType)
    {
        Wave prefab = waves.Where(e => e.waveType == waveType).FirstOrDefault();
        Wave wave = Instantiate(prefab);
        return wave;
    }

}