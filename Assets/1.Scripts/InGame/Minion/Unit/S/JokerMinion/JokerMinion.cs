using System.Collections.Generic;
using UnityEngine;

// [S] 조커: 필드의 조커를 선택 → 변신 버튼(JokerTransformButton) → 장착 슬라임 중 원하는 종 선택(JokerSelectCanvas, 일시정지)
// - 변신 결과는 같은 타일, 머지 레벨 유지. 단 levelDownChance(기본 30%) 확률로 실패 → 머지 레벨 -1, 가장 낮은 단계면 사라짐
// - 아껴 뒀다가 필요한 종이 생기면 변신하는 카드
// - 변신 전까지는 일반 탄 공격
public class JokerMinion : MinionGrowth1
{
    // 최대 머지 레벨에서는 쓸 수 있는 각성이 없어 진화하지 않음 (머지 레벨을 올리는 슬라임)
    public override bool CanEvolve => false;

    JokerMinionData jokerMinionData;
    AllyBulletSpec allyBulletSpec;

    public float levelDownChance; // 0~1, 변신 실패 확률

    public override void Spawn(Vector2 pos, int lv)
    {
        jokerMinionData = MinionManager.Instance.GetMinionData(key) as JokerMinionData;
        base.Spawn(pos, lv);
    }

    public override void InitMinion()
    {
        allyBulletSpec = new AllyBulletSpec();
        levelDownChance = jokerMinionData.levelDownChance;
        base.InitMinion();
    }

    public override AllyBulletObject GetBullet()
    {
        allyBulletSpec.damage = AttackPower;
        allyBulletSpec.StartBulletSpec();
        return allyBulletSpec.Instantiate(this);
    }

    // 변신 가능한 종: 장착 슬라임 중 조커 자신 제외
    public List<string> GetTransformKeys()
    {
        List<string> keys = new List<string>();
        foreach (UserMinion userMinion in UserDataManager.Instance.userMinionManager.userMinionData.equiptedMinions)
        {
            if (userMinion != null && !string.IsNullOrEmpty(userMinion.key) && userMinion.key != key && !keys.Contains(userMinion.key))
                keys.Add(userMinion.key);
        }
        return keys;
    }

    // 변신: levelDownChance 확률로 실패
    // - 성공: 같은 머지 레벨로 변신
    // - 실패: 머지 레벨 -1로 변신, 가장 낮은 단계(별 1, mergeLevel 0)면 조커가 사라짐
    public Minion TransformTo(string targetKey)
    {
        if (TileManager.Instance.GetTile(this) == null) // 필드에 없으면 (합성·제거됨) 무시
            return null;

        // 성공 확률 기준으로 판정 (연속 4회 실패 후엔 성공 +50%p, MinionChancePity)
        bool failed = !MinionChancePity.RollInGame(key, 1f - levelDownChance);
        if (!failed)
            return MinionFieldController.Instance.ReplaceMinion(this, targetKey, mergeLevel);

        if (mergeLevel <= 0)
        {
            ToastCanvas.Toast(TranslateManager.GetText("Joker_Fail_Vanish"));
            MinionFieldController.Instance.RemoveFieldMinion(this);
            return null;
        }

        ToastCanvas.Toast(TranslateManager.GetText("Joker_Fail_LevelDown"));
        return MinionFieldController.Instance.ReplaceMinion(this, targetKey, mergeLevel - 1);
    }
}
