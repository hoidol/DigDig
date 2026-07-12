
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
public abstract class BossBehaviour : MonoBehaviour
{

    public string behaviourName;
    [HideInInspector] public Boss boss;
    void Awake()
    {
        boss = GetComponentInParent<Boss>();
    }
    public abstract UniTask StartBehaviour();
}