using UnityEngine;

// 저금통: 영구 소환. 주변 골드를 자동 수집, 없으면 플레이어 오른쪽 상단 대기
public class PiggyBankItem : Item
{
    const float MOVE_SPEED = 4f;

    [SerializeField] PiggyBank piggyBankPrefab;

    PiggyBank spawnedBank;

    public override void OnEquip()
    {
        spawnedBank = Instantiate(piggyBankPrefab, Character.Instance.transform.position, Quaternion.identity);
        spawnedBank.Init(MOVE_SPEED);
    }

    public override void OnUnequip()
    {
        if (spawnedBank != null)
        {
            Destroy(spawnedBank.gameObject);
            spawnedBank = null;
        }
    }

    public override string GetDescription(int lv = 1,bool detail = false)
    {
        return "주변 골드를 자동으로 수집합니다.";
    }
}
