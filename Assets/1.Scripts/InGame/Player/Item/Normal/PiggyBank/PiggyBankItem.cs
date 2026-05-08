using UnityEngine;

// count에 따라 최대 획득 개수/이동속도 증가, 쿨타임 후 PiggyBank 1개 소환
public class PiggyBankItem : TriggerItem
{
    [SerializeField] PiggyBank piggyBankPrefab;

    static readonly int[] maxPickCounts = { 20, 35, 50 };
    static readonly float[] moveSpeeds = { 4f, 6f, 8f };

    PiggyBank spawnedBank;

    public override void OnEquip(Player player)
    {
        coolTime = 40f;
        base.OnEquip(player);
    }

    public override void UpdateItem()
    {
        if (spawnedBank != null)
            spawnedBank.UpdateStats(maxPickCounts[Index], moveSpeeds[Index]);
    }

    public override void OnUnequip(Player player)
    {
        base.OnUnequip(player);
        DestroyBank();
    }

    public override void OnTrigger()
    {
        if (spawnedBank != null) return;

        spawnedBank = Instantiate(piggyBankPrefab, Player.Instance.transform.position, Quaternion.identity);
        spawnedBank.Init(maxPickCounts[Index], moveSpeeds[Index], OnBankFinished);
    }

    void OnBankFinished()
    {
        spawnedBank = null;
    }

    void DestroyBank()
    {
        if (spawnedBank != null)
        {
            Destroy(spawnedBank.gameObject);
            spawnedBank = null;
        }
    }

    int Index => Mathf.Clamp(count - 1, 0, 2);
}
