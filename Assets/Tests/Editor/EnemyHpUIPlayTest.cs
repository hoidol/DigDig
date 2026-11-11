using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

// 적 머리 위 체력바 (EnemyWorldCanvas + EnemyHpUI 템플릿 풀링)
// 씬에 EnemyWorldCanvas가 없으면 테스트에서 만들어서 확인
public class EnemyHpUIPlayTest : InGamePlayTestBase
{
    [UnityTest]
    public IEnumerator HpBar_FollowsEnemyHp()
    {
        if (!Application.isPlaying) { BeforeEnterPlayMode(); yield return new EnterPlayMode(); }
        yield return WaitGameStart();
        yield return Body();
        AssertNoErrors();
    }

    static EnemyWorldCanvas EnsureCanvas()
    {
        EnemyWorldCanvas canvas = EnemyWorldCanvas.Instance;
        if (canvas != null && canvas.hpUITemplate != null)
            return canvas;

        var go = new GameObject("EnemyWorldCanvas(Test)", typeof(RectTransform), typeof(Canvas));
        go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var template = new GameObject("EnemyHpUI", typeof(RectTransform));
        template.transform.SetParent(go.transform, false);
        template.SetActive(false); // 템플릿 Awake는 Bind에서 켜질 때
        EnemyHpUI ui = template.AddComponent<EnemyHpUI>();
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(template.transform, false);
        ui.thumImage = fill.GetComponent<Image>();
        canvas = go.AddComponent<EnemyWorldCanvas>(); // Awake에서 템플릿 찾음
        EnemyWorldCanvas.Instance = canvas;
        return canvas;
    }

    IEnumerator Body()
    {
        EnemyWorldCanvas canvas = EnsureCanvas();
        Assert.IsNotNull(canvas.hpUITemplate, "EnemyHpUI 템플릿 없음");
        Assert.IsNotNull(canvas.hpUITemplate.thumImage, "EnemyHpUI thumImage(체력바 이미지) 미연결");
        Assert.IsFalse(canvas.hpUITemplate.gameObject.activeSelf, "템플릿이 켜져 있음 (빈 체력바가 보임)");

        Enemy prefab = GameManager.Instance.stageData.GetEnemyPrefab(EnemyType.Normal);
        Enemy enemy = EnemyFieldController.Instance.Instantiate(prefab);
        enemy.Spawn(EnemyFieldController.Instance.GetSpawnPosition(), 0);
        yield return WaitFrames(2);

        Assert.IsTrue(canvas.HasHpUI(enemy), "스폰했는데 체력바가 붙지 않음");
        EnemyHpUI ui = FindUI(canvas, enemy);
        Assert.IsNotNull(ui, "체력바 오브젝트 없음");
        Assert.AreSame(canvas.transform, ui.transform.parent, "체력바가 EnemyWorldCanvas 바로 아래에 있지 않음");
        Assert.IsTrue(ui.gameObject.activeInHierarchy, "체력바가 안 보임");
        Assert.AreEqual(1f, ui.thumImage.fillAmount, 0.001f, "처음 체력바가 가득 차 있지 않음");
        Assert.AreEqual(Image.Type.Filled, ui.thumImage.type, "thumImage가 Filled가 아님");
        Assert.Less(Vector2.Distance(ui.transform.position, (Vector2)(enemy.HpPoint.position + ui.offset)), 0.01f, "체력바가 적 머리 위를 따라가지 않음");

        enemy.maxHp = enemy.curHp = 100f;
        enemy.TakeDamage(new DamageData { damage = 40f, attackType = AttackType.AllRounder });
        yield return WaitFrames(1);
        Assert.AreEqual(0.6f, ui.thumImage.fillAmount, 0.001f, "피해 후 체력바 비율");

        enemy.TakeDamage(KILL);
        yield return WaitFrames(1);
        Assert.IsFalse(canvas.HasHpUI(enemy), "죽었는데 체력바가 남아 있음");
        Assert.IsFalse(ui.gameObject.activeSelf, "죽었는데 체력바가 보임");

        // 다음 적은 풀의 체력바를 재사용
        Enemy next = EnemyFieldController.Instance.Instantiate(prefab);
        next.Spawn(EnemyFieldController.Instance.GetSpawnPosition(), 0);
        yield return WaitFrames(1);
        Assert.AreSame(ui, FindUI(canvas, next), "체력바를 재사용하지 않음");
        Assert.AreEqual(1f, ui.thumImage.fillAmount, 0.001f, "재사용한 체력바가 초기화되지 않음");

        // 부활 등으로 한꺼번에 회수돼도 체력바가 남지 않음
        EnemyFieldController.Instance.ClearAllEnemies();
        yield return WaitFrames(1);
        Assert.IsFalse(canvas.HasHpUI(next), "회수된 적의 체력바가 남아 있음");

        // 보스는 머리 위 체력바 대신 BossCanvas
        Boss bossPrefab = GameManager.Instance.stageData.boss;
        Assert.IsNotNull(bossPrefab, "스테이지 보스 프리팹 없음");
        Assert.IsFalse(bossPrefab.UseHpUI, "보스가 머리 위 체력바를 사용함");
    }

    static EnemyHpUI FindUI(EnemyWorldCanvas canvas, Enemy enemy)
    {
        foreach (EnemyHpUI ui in canvas.GetComponentsInChildren<EnemyHpUI>(true))
            if (ui.Target == enemy)
                return ui;
        return null;
    }
}
