using UnityEngine;

//광석을 부수면 공격속도 빨라짐
public class LeatherGloves : Item
{
    Buff buff;


    public override void OnEquip(Player player)
    {
        buff = new Buff(StatType.AttackSpeed, 1f + 0.1f * count, StatOpType.Multiply);
        player.AddBuff(buff);
        GameEventBus.Subscribe<DestroyedStoneEvent>(OnOreStoneDestroyedEvent);
    }
    int overlapCount;
    int MAX_OVERLAP_COUNT = 5;
    float timer;
    float duraction = 5;
    public void OnOreStoneDestroyedEvent(DestroyedStoneEvent e)
    {
        overlapCount++;
        if (overlapCount > MAX_OVERLAP_COUNT)
            overlapCount = MAX_OVERLAP_COUNT;
        timer = duraction;
    }
    void Update()
    {
        if (timer <= 0)
        {
            overlapCount--;
            UpdateItem();
        }

        if (timer > 0)
            timer -= Time.deltaTime;
    }
    public override void UpdateItem()
    {
        Player player = Player.Instance;
        player.RemoveBuff(buff);
        if (count > 0)
        {
            buff = new Buff(StatType.AttackSpeed, 1f + 0.1f * count, StatOpType.Multiply);
            player.AddBuff(buff);
        }


    }

    public override void OnUnequip(Player player)
    {
        player.RemoveBuff(buff);
        GameEventBus.Unsubscribe<DestroyedStoneEvent>(OnOreStoneDestroyedEvent);
    }
}
