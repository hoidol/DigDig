using System.Collections.Generic;
using UnityEngine;

public class Snake : EventObject
{
    List<SnakeSuggest> snakeSuggests = new List<SnakeSuggest>();
    void Start()
    {
        //공격력 증가 / 체력 감소
        SnakeSuggest suggest = new SnakeSuggest();
        suggest.buff = new Buff(StatType.AttackPower, 1.3f, StatOpType.Multiply);
        suggest.nerf = new Buff(StatType.MaxHp, 0.7f, StatOpType.Multiply);
        snakeSuggests.Add(suggest);

        //크리티컬 확률 증가 / 체력 감소
        suggest = new SnakeSuggest();
        suggest.buff = new Buff(StatType.CritChance, 10f, StatOpType.Add);
        suggest.nerf = new Buff(StatType.MaxHp, 0.7f, StatOpType.Multiply);
        snakeSuggests.Add(suggest);

        //재장전 속도 증가 / 체력 감소
        suggest = new SnakeSuggest();
        suggest.buff = new Buff(StatType.ReloadSpeed, 1.3f, StatOpType.Multiply);
        suggest.nerf = new Buff(StatType.MaxHp, 0.7f, StatOpType.Multiply);
        snakeSuggests.Add(suggest);

        //탄 효율 증가 / 체력 감소
        suggest = new SnakeSuggest();
        suggest.buff = new Buff(StatType.AmmoEfficiency, 1.2f, StatOpType.Multiply);
        suggest.nerf = new Buff(StatType.MaxHp, 0.7f, StatOpType.Multiply);
        snakeSuggests.Add(suggest);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            interacting = true;
            Time.timeScale = 0;
            SnakeSuggest snakeSuggest = snakeSuggests[Random.Range(0, snakeSuggests.Count)];
            SnakeCanvas.Instance.OpenCanvas(snakeSuggest, () =>
            {
                Time.timeScale = 1;
                Destroy();
            });
        }
    }
}

public class SnakeSuggest
{
    public Buff buff; //버프
    public Buff nerf; //패널티 

    public string BuffTitle()
    {
        return "버프 타이틀";
    }
    public string BuffDesc()
    {
        return "버프 타이틀";
    }

    public string NerfTitle()
    {
        return "버프 타이틀";
    }
    public string NerfDesc()
    {
        return "버프 타이틀";
    }


}