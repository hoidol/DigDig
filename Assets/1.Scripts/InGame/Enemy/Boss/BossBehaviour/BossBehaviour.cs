
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
public abstract class BossBehaviour : MonoBehaviour
{
    
    public string behaviourName;
    public Boss boss;
    void Awake()
    {
        boss = GetComponent<Boss>();
    }
    public  abstract UniTask StartBehaviour();
}