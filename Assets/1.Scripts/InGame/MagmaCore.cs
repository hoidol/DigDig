using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MagmaCore : MonoSingleton<MagmaCore>, IPicker
{
    public float maxHp;
    public float curHp;

    public int exp;

    public int lv;

    public Transform Transform => transform;

    Coroutine collectingCor;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectingCor != null)
            {
                StopCoroutine(collectingCor);
            }
            collectingCor = StartCoroutine(CoCollecting());
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            if (collectingCor != null)
            {
                StopCoroutine(collectingCor);
                collectingCor = null;
            }
        }
    }

    IEnumerator CoCollecting()
    {
        float timer = 0.2f;
        while (true)
        {
            if (timer <= 0)
            {
                string oreKey = Player.Instance.inventory.TakeOut();
                if (oreKey == null)
                    break;

                CollectOre(oreKey);
                timer = 0.2f;
            }
            timer -= Time.deltaTime;
            yield return null;
        }
    }

    public void CollectOre(string oreKey)
    {
        Ore ore = OreManager.Instance.GetOre();
        ore.Droped(Player.Instance.transform.position, oreKey);
        ore.Take(this);

        AddExp(ore.exp);
    }

    public void AddExp(int e)
    {
        this.exp += e;
        if (exp >= GetMaxExp())
        {
            Debug.Log("LevelUp!");
            LevelUp();
        }

    }
    bool levelUped;
    void LevelUp()
    {
        int remain = exp - GetMaxExp();
        exp = remain;
        lv++;
        levelUped = true;
        //강화 UI 활성화
    }
    [SerializeField] int maxExp;
    public int GetMaxExp(int l = -1)
    {
        if (l == -1)
            l = lv;

        if (maxExp == 0 || levelUped)
        {
            maxExp = 10 + lv * 5;
            levelUped = false;
        }

        return maxExp;
    }

    public void PickUp(IPickable pickable)
    {
        pickable.PickedUp();
    }
}
