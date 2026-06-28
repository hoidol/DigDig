
using UnityEngine;
using Cysharp.Threading.Tasks;
public class BossBehaviour : MonoBehaviour
{
    public Boss boss;
    void Awake()
    {
        boss = GetComponent<Boss>();
    }
    public async virtual UniTask StartBehaviour()
    {

    }
}